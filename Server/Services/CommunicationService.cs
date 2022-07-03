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
    private Dictionary<int, IPEndPoint> clients = new();
    private DatabaseService db = new();
    private Dictionary<Command, Action<Request, IPEndPoint>> handlers = new();
    private Socket listeningSocket; // todo: nullable
    private string localIp = "127.0.0.1";
    private int localPort = 5001;
    private bool run = false;

    public async void AcceptInviteHandle(Request req, IPEndPoint ip)
    {
      // ip = ip of user to
      // req.Data = contact id
      try
      {
        Contact contact = await db.AcceptInviteAsync(req.Data);

        // to user to
        SendData(new Response { Command = Command.GetContact, Status = Status.OK, Data = new Prop { Id = contact.UserFromId, Name = contact.NameAtUserTo } }, ip);

        // to user from
        if (clients.ContainsKey(contact.UserFromId))
        {
          // only if user from online
          SendData(
            new Response { Command = Command.GetContact, Status = Status.OK, Data = new Prop { Id = contact.UserToId, Name = contact.NameAtUserFrom } },
            clients[contact.UserFromId]
          );
        }
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public void AddGroupHandle(Request req, IPEndPoint ip)
    { }

    public void DisconnectHandle(Request req, IPEndPoint ip)
    {
      clients.Remove(req.Data);
    }

    public void LeaveGroupHandle(Request req, IPEndPoint ip)
    { }

    public void RemoveContactHandle(Request req, IPEndPoint ip)
    {
      try
      {
        db.RemoveContact(req.Data.From, req.Data.To);
        SendData(new Response { Command = Command.RemoveContact, Status = Status.OK, Data = "Contact is removed" }, ip);
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = Command.RemoveContact, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public void RenameContactHandle(Request req, IPEndPoint ip)
    {
      try
      {
        db.RenameContact(req.Data.From, req.Data.To, req.Data.newName);
        SendData(new Response { Command = Command.RenameContact, Status = Status.OK, Data = "Contact renamed" }, ip);
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public async void RenameGroupHandle(Request req, IPEndPoint ip)
    {
      try
      {
        await db.RenameGroupAsync(req.Data.IdGroup, req.Data.NewName);
        SendData(new Response { Command = Command.RenameGroup, Status = Status.OK, Data = "Group renamed" }, ip);
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public void Run()
    {
      // запуск
      run = true;
      listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      Task listenongTask = new Task(Listen);
    }

    public async void SendInviteHandle(Request req, IPEndPoint ip)
    {
      try
      {
        Contact contact = await db.SendInviteAsync(req.Data.From, req.Data.To);

        // to who sended request
        SendData(new Response { Command = Command.SendInvite, Status = Status.OK, Data = "Invite is sent successfully" }, ip);

        // to who will get invite
        IPEndPoint toIp;
        if (clients.TryGetValue(req.Data.To, out toIp))
        {
          SendData(new Response { Command = Command.GetInvite, Status = Status.OK, Data = contact }, toIp);
        }
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public void SendMessageToContactHandle(Request req, IPEndPoint ip)
    {
      try
      {
        // to user who sent
        //SendData(new Response { Command = Command.SendMessageToContact, Status = Status.OK, Data = "Message sent" }, ip);

        // to user for whom sent
        IPEndPoint toIp;
        if (clients.TryGetValue(req.Data.To, out toIp))
        {
          SendData(new Response { Command = Command.GetMessageFromContact, Status = Status.OK, Data = req.Data.Message }, toIp);
        }
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public void SendMessageToGroupHandle(Request req, IPEndPoint ip)
    {
      try
      {
        IPEndPoint toIp;
        IEnumerable<int> ids = db.GetGroupMembersIds(req.Data.Id);
        foreach (int id in ids)
        {
          if (clients.TryGetValue(id, out toIp))
          {
            SendData(new Response { Command = Command.GetMessageFromGroup, Status = Status.OK, Data = new { Id = req.Data.Id, Message = req.Data.Message } }, toIp);
          }
        }
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
    }

    public void SignInHandle(Request req, IPEndPoint ip)
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

    public void SignUpHandle(Request req, IPEndPoint ip)
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

    private void Handle(string reqStr, IPEndPoint ip)
    {
      // allways: Command, Data
      Request req = JsonConvert.DeserializeObject<Request>(reqStr);
      try
      {
        handlers[req.Command](req, ip);
      }
      catch (Exception ex)
      {
        SendData(new Response { Command = req.Command, Status = Status.Bad, Data = ex.Message }, ip);
      }
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
            Console.WriteLine(string.Format("{0}:{1} = {2}", clientIp.ToString(), clientFullIp.Port, request));
            Handle(request, clientFullIp);
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

    private void SendData(Response response, IPEndPoint ip)
    {
      string responseStr = JsonConvert.SerializeObject(response);
      byte[] data = Encoding.Unicode.GetBytes(responseStr);

      // send to one
      listeningSocket.SendTo(data, ip);
    }

    public CommunicationService()
    {
      handlers.Add(Command.SignIn, SignInHandle);
      handlers.Add(Command.SignUp, SignUpHandle);
      handlers.Add(Command.SendMessageToContact, SendMessageToContactHandle);
      handlers.Add(Command.SendMessageToGroup, SendMessageToGroupHandle);
      handlers.Add(Command.SendInvite, SendInviteHandle);
      handlers.Add(Command.AcceptInvite, AcceptInviteHandle);
      handlers.Add(Command.RenameContact, RenameContactHandle);
      handlers.Add(Command.RemoveContact, RemoveContactHandle);
      handlers.Add(Command.AddGroup, AddGroupHandle);
      handlers.Add(Command.LeaveGroup, LeaveGroupHandle);
      handlers.Add(Command.Disconnect, DisconnectHandle);
      handlers.Add(Command.RenameGroup, RenameGroupHandle);
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