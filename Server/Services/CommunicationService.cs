using CrossLibrary;
using Newtonsoft.Json;
using Server.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Services
{
  public class CommunicationService
  {
    private DatabaseService db = new DatabaseService();
    private int localPort = 5001;
    private string localIp = "127.0.0.1";
    private bool run = false;
    private Socket listeningSocket;
    private Dictionary<int, IPEndPoint> clients = new Dictionary<int, IPEndPoint>();

    private Dictionary<Command, Action<dynamic>> handlers = new Dictionary<Command, Action<dynamic>>();

    public void Run()
    {
      // запуск
      run = true;
      listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      Task listenongTask = new Task(Listen);
    }

    private void Listen()
    {
      IPAddress ip;
      if (IPAddress.TryParse(localIp, out ip))
      {
        IPEndPoint localIp = new IPEndPoint(ip, localPort);
        listeningSocket.Bind(localIp);
        try
        {
          while (run)
          {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[1024];
            EndPoint clientIp = new IPEndPoint(IPAddress.Any, 0);

            do
            {
              bytes = listeningSocket.ReceiveFrom(data, ref clientIp);
              builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (listeningSocket.Available > 0);
            IPEndPoint clientFullIp = (IPEndPoint)clientIp;

            string request = builder.ToString();
            Handle(request, clientFullIp);

            //Console.WriteLine(string.Format("{0}:{1} - {2}", clientFullIp.Address.ToString(), clientFullIp.Port, request));

            //SendData(builder.ToString(), clientFullIp.Port);
          }
        }
        catch (SocketException socketEx)
        {
          if (socketEx.ErrorCode != 10004)
            Console.WriteLine(socketEx.Message + socketEx.ErrorCode);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
        finally
        {
          CloseConnection();
        }
      }
    }

    private void Handle(string reqStr, IPEndPoint ip)
    {
      // allways: Command, Data
      dynamic? req = JsonConvert.DeserializeObject(reqStr);
      if (req != null)
      {
        try
        {
          if (req.Command == Command.SignIn)
          {
            SignInHandle(req, ip);
          }
          else if (req.Command == Command.SignUp)
          {
            SignUpHandle(req, ip);
          }
          else
          {
            handlers[req.Command](req);
          }
        }
        catch (Exception ex)
        {
          SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
        }
      }
    }



    private void SendData(Response response, IPEndPoint ip)
    {
      string responseStr = JsonConvert.SerializeObject(response);
      byte[] data = Encoding.Unicode.GetBytes(responseStr);

      // send to one
      listeningSocket.SendTo(data, ip);

      //foreach (var client in clients)
      //{
      //  if (client.Key.Port == port) // send only to one who asked
      //  {
      //    listeningSocket.SendTo(data, client.Key);
      //    clients[client.Key] = DateTime.Now; // update time
      //  }
      //}
    }


    // Handlers
    public void SignUpHandle(dynamic req, IPEndPoint ip)
    {
      Response res;
      try
      {
        User user = db.AddUser(req.Data.Name, req.Data.Password);
        // add user to connected clients
        clients.TryAdd(user.Id, ip);

        res = new Response
        {
          Command = Command.SignUp,
          Status = Status.OK,
          Data = new Prop
          {
            Id = user.Id,
            Name = user.Name
          }
        };
      }
      catch (Exception ex)
      {
        res = new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message };
      }

      SendData(res, ip);
    }

    public void SignInHandle(dynamic req, IPEndPoint ip)
    {
      Response res;
      User? user = db.GetUser(req.Data.Name);
      if (user != null)
      {
        if (Hasher.Verify(req.Data.Password, user.Password))
        {
          // add user to connected clients
          clients.TryAdd(user.Id, ip);

          res = new Response
          {
            Command = req.Command,
            Status = Status.OK,
            Data = new
            {
              User = new Prop { Id = user.Id, Name = user.Name },
              Groups = db.GetUserGroups(user.Id),
              Invites = db.GetInvites(user.Id),
              Contacts = db.GetContacts(user.Id)
            }
          };
        }
        else
        {
         res = new Response { Command = req.Command, Status = Status.Bad, Data = "Wrong password" };
        }
      }
      else
      {
        res = new Response { Command = req.Command, Status = Status.Bad, Data = "User not found" };
      }

      SendData(res, ip);
    }
    public void SendMessageToContactHandle(dynamic req) { }
    public void SendMessageToGroupHandle(dynamic req) { }
    public void SendInviteHandle(dynamic req)
    {
      //  try
      //  {
      //    //await db.SendInviteAsync(obj.Data.Id, obj.Data.NameTo);
      //    SendData(
      //      new Response
      //      {
      //        Command = req.Command,
      //        Status = Status.OK,
      //        Data = null
      //      }, port);
      //  }
      //  catch (Exception ex)
      //  {
      //    SendData(
      //new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, port);
      //  }
    }
    public void AcceptInviteHandle(dynamic req)
    {
      //  IPEndPoint port; // tmp
      //  try
      //  {
      //    //Contact contact = await db.AcceptInviteAsync(obj.Data.Id);
      //    //await db.AcceptInviteAsync(obj.Data.Id);
      //    SendData(
      //      new Response
      //      {
      //        Command = req.Command,
      //        Status = Status.OK,
      //        //Data = new Prop {
      //        //    Id =  contact.UserFromId,
      //        //    Name = contact.NameAtUserTo
      //        //}
      //      }, port);

      //    //SendData(
      //    //  new Response
      //    //  {
      //    //      Command = obj.Command,
      //    //      Status = Status.OK,
      //    //      Data = new Prop {
      //    //          Id = contact.UserToId,
      //    //          Name = contact.NameAtUserFrom
      //    //      }
      //    //  }, clients[contact.UserFromId].port);
      //  }
      //  catch (Exception ex)
      //  {
      //    SendData(
      //new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, port);
      //  }
    }
    public void RenameContactHandle(dynamic req) { }
    public void RemoveContactHandle(dynamic req) { }
    public void AddGroupHandle(dynamic req) { }
    public void LeaveGroupHandle(dynamic req) { }

    public void DisconnectHandle(dynamic req)
    {
      clients.Remove(req.Data);
    }




    public CommunicationService()
    {
      //handlers.Add(Command.SignIn, SignInHandle);
      //handlers.Add(Command.SignUp, SignUpHandle);
      handlers.Add(Command.SendMessageToContact, SendMessageToContactHandle);
      handlers.Add(Command.SendMessageToGroup, SendMessageToGroupHandle);
      handlers.Add(Command.SendInvite, SendInviteHandle);
      handlers.Add(Command.AcceptInvite, AcceptInviteHandle);
      handlers.Add(Command.RenameContact, RenameContactHandle);
      handlers.Add(Command.RemoveContact, RemoveContactHandle);
      handlers.Add(Command.AddGroup, AddGroupHandle);
      handlers.Add(Command.LeaveGroup, LeaveGroupHandle);
      handlers.Add(Command.Disconnect, DisconnectHandle);
    }

    private void CloseConnection()
    {
      if (listeningSocket != null)
      {
        listeningSocket.Shutdown(SocketShutdown.Both);
        listeningSocket.Close();
        listeningSocket = null;
      }
    }
  }
}