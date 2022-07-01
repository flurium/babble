using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using CrossLibrary;
using Server.Models;

namespace Server.Services
{
    //public struct Response
    //{
    //    public Command Command { get; set; }
    //    public Status Status { get; set; }
    //    public dynamic Data { get; set; }
    //}


    public class CommunicationService
    {
        private DatabaseService db = new DatabaseService();
        private int localPort = 5001;
        private string localIp = "127.0.0.1";
        private bool run = false;
        private Socket listeningSocket;
        private Dictionary<int, IPEndPoint> clients = new Dictionary<int, IPEndPoint>();

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
                        WorkData(request, clientFullIp.Port);


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

        private void WorkData(string request, int port)
        {
            // allways: Command, Data
            dynamic? obj = JsonConvert.DeserializeObject(request);
            if (obj != null)
            {
                try
                {
                    if (obj.Command == Command.SignIn)
                    {
                        // get user
                        //Hasher
                        User? user = db.GetUser(obj.Data.Name);
                        if (user != null)
                        {
                            if (Hasher.Verify(obj.Data.Password, user.Password))
                            {
                                SendData(
                              new Response
                              {
                                  Command = obj.Command,
                                  Status = Status.OK,
                                  Data = new
                                  {
                                      User = user,
                                      Groups = db.GetUserGroups(user.Id),
                                      Invites = db.GetInvites(user.Id),
                                      Contacts = db.GetContacts(user.Id)
                                  }
                              }, port);

                            }
                            else
                            {
                                SendData(
                            new Response {Command = obj.Command, Status = Status.Bad, Data = "wrong password" }, port);
                            }
                        }
                        else
                        {
                            SendData(
                        new Response {Command = obj.Command, Status = Status.Bad, Data = "user not found" }, port);
                        }
                    }

                    if (obj.Command == Command.SignUp)
                    {
                        // add user
                        //try catch block???
                        try
                        {
                            User user = db.AddUser(obj.Data.Name, obj.Data.Password);
                            SendData(
                              new Response
                              {
                                  Command = obj.Command,
                                  Status = Status.OK,
                                  Data = new Prop
                                  {
                                      Id = user.Id,
                                      Name = user.Name
                                  }
                              }, port);
                        }
                            
                        
                        catch (Exception ex)
                        {
                            SendData(
                        new Response { Command = obj.Command, Status = Status.Bad, Data = ex.Message }, port);
                        }
                    }

                    if (obj.Command == Command.SendInvite)
                    {
                       try 
                        {                      
                            //await db.SendInviteAsync(obj.Data.Id, obj.Data.NameTo);
                            SendData(
                              new Response
                              {
                                  Command = obj.Command,
                                  Status = Status.OK,
                                  Data = null
                              }, port);
                        }
                        catch (Exception ex)
                        {
                            SendData(
                        new Response { Command = obj.Command, Status = Status.Bad, Data = ex.Message }, port);
                        }
                    }

                    if (obj.Command == Command.AcceptInvite)
                    {
                        try
                        {   
                            //Contact contact = await db.AcceptInviteAsync(obj.Data.Id);
                            //await db.AcceptInviteAsync(obj.Data.Id);
                            SendData(
                              new Response
                              {
                                  Command = obj.Command,
                                  Status = Status.OK,
                                  //Data = new Prop {
                                  //    Id =  contact.UserFromId,
                                  //    Name = contact.NameAtUserTo
                                  //}
                              }, port);

                            //SendData(
                            //  new Response
                            //  {
                            //      Command = obj.Command,
                            //      Status = Status.OK,
                            //      Data = new Prop {
                            //          Id = contact.UserToId,
                            //          Name = contact.NameAtUserFrom
                            //      }
                            //  }, clients[contact.UserFromId].port);
                        }
                        catch (Exception ex)
                        {
                            SendData(
                        new Response { Command = obj.Command, Status = Status.Bad, Data = ex.Message }, port);
                        }
                    }

                    //if (obj.Command == "removeuser")
                    //{
                    //    // remove user
                    //    db.RemoveUserAsync(obj.Data.User.Id);
                    //    SendData(
                    //      new Response
                    //      {
                    //          Status = "ok",
                    //          Data = "User removed"
                    //      }, port);
                    //}
                }
                catch (Exception ex)
                {
                    SendData(
                        new Response {Command = obj.Command, Status = Status.Bad, Data = ex.Message }, port);
                }
            }
        }

        private void SendData(Response response, int port)
        {

            string responseStr = JsonConvert.SerializeObject(response);
            byte[] data = Encoding.Unicode.GetBytes(responseStr);

            foreach (var client in clients)
            {
                //if (client.Key.Port == port) // send only to one who asked
                {
                    //listeningSocket.SendTo(data, client.Key);
                    //clients[client.Key] = DateTime.Now; // update time
                }
            }
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