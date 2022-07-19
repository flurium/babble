using CrossLibrary;
using Newtonsoft.Json;
using Server.Models;
using Server.Services.Database;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static CrossLibrary.Globals;

namespace Server.Services
{
    /// <summary>
    /// Receiving requests from the client,
    /// communicating with the database and
    /// sending responses back to the client
    /// </summary>
    public class CommunicationService
    {
        private readonly Dictionary<int, IPEndPoint> clients = new();
        private readonly DatabaseService db = new();
        private readonly Dictionary<Command, Action<Request, IPEndPoint>> handlers = new();
        private Socket? listeningSocket;
        private readonly bool run = false;
        private readonly ILogger logger = new Logger();

        /// <summary>
        /// Handler for accepting invite by user id.
        /// Send two responses
        /// <see cref="DatabaseService.AcceptInviteAsync(int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains id</param>
        /// <param name="ip">UserTo IP</param>
        private async void AcceptInviteHandle(Request req, IPEndPoint ip)
        {
            // ip = ip of user to
            // req.Data = contact id
            int id = req.Data.Id;
            Contact contact = await db.AcceptInviteAsync(id);

            // to user to
            SendData(new Response { Command = Command.GetContact, Status = Status.OK, Data = new Prop { Id = contact.UserFromId, Name = contact.NameAtUserTo } }, ip);

            // to user from
            if (clients.ContainsKey(contact.UserFromId))
            {
                // only if user from is online
                SendData(
                  new Response { Command = Command.GetContact, Status = Status.OK, Data = new Prop { Id = contact.UserToId, Name = contact.NameAtUserFrom } },
                  clients[contact.UserFromId]
                );
            }
        }

        /// <summary>
        /// Handler for creating new group by user id and name of group.
        /// <see cref="DatabaseService.CreateGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void CreateGroupHandle(Request req, IPEndPoint ip)
        {
            int uid = req.Data.User;
            string name = req.Data.Group;
            Prop group = await db.CreateGroupAsync(uid, name);
            SendData(new Response { Command = Command.CreateGroup, Status = Status.OK, Data = group }, ip);
        }

        /// <summary>
        /// Handler for adding new user to existing group.
        /// <see cref="DatabaseService.AddUserToGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void EnterGroupHandle(Request req, IPEndPoint ip)
        {
            int uid = req.Data.User;
            string groupName = req.Data.Group;
            Prop group = await db.AddUserToGroupAsync(uid, groupName);

            SendData(new Response { Command = Command.EnterGroup, Status = Status.OK, Data = group }, ip);
        }

        /// <summary>
        /// Handler for removing user from existing group.
        /// <see cref="DatabaseService.RemoveUserFromGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void LeaveGroupHandle(Request req, IPEndPoint ip)
        {
            int uid = req.Data.Id;
            string name = req.Data.Name;
            await db.RemoveUserFromGroupAsync(uid, name);

            SendData(new Response { Command = Command.LeaveGroup, Status = Status.OK, Data = "Group is removed" }, ip);
        }

        /// <summary>
        /// Handler for disconnecting.
        /// </summary>
        /// <param name="req">Request contains user id</param>
        /// <param name="ip">User IP</param>
        private void DisconnectHandle(Request req, IPEndPoint ip)
        {
            int id = req.Data.Id;
            clients.Remove(id);
        }

        /// <summary>
        /// Handler for removing contact from user's contact list.
        /// <see cref="DatabaseService.RemoveContact(int, int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
        /// <param name="ip">UserTo IP</param>
        private async void RemoveContactHandle(Request req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            await db.RemoveContact(from, to);
            SendData(new Response { Command = Command.RemoveContact, Status = Status.OK, Data = "Contact is removed" }, ip);
        }

        /// <summary>
        /// Handler for renaming contact in user's contact list.
        /// <see cref="DatabaseService.RenameContact(int, int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids and new contact name</param>
        /// <param name="ip">UserTo IP</param>
        private void RenameContactHandle(Request req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            string newName = req.Data.NewName;
            db.RenameContact(from, to, newName);
            SendData(new Response { Command = Command.RenameContact, Status = Status.OK, Data = new Prop { Id = to, Name = newName } }, ip);
        }

        /// <summary>
        /// Handler for renaming group in user's group list.
        /// <see cref="DatabaseService.RenameGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new group name</param>
        /// <param name="ip">UserTo IP</param>
        private async void RenameGroupHandle(Request req, IPEndPoint ip)
        {
            int id = req.Data.Id;
            string name = req.Data.NewName;
            await db.RenameGroupAsync(id, name);
            SendData(new Response { Command = Command.RenameGroup, Status = Status.OK, Data = new Prop { Id = id, Name = name } }, ip);
        }

        /// <summary>
        /// Handler for sending invitations between users.
        /// Send two responses
        /// <see cref="DatabaseService.SendInviteAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
        /// <param name="ip">UserTo IP</param>
        private async void SendInviteHandle(Request req, IPEndPoint ip)
        {
            int from = req.Data.From;
            string to = req.Data.To;
            Contact contact = await db.SendInviteAsync(from, to);

            // to who sent request
            SendData(new Response { Command = Command.SendInvite, Status = Status.OK, Data = new { Message = "Invite was send successfully" } }, ip);

            // to who will get invite
            IPEndPoint toIp;
            if (clients.TryGetValue(contact.UserToId, out toIp!))
            {
                SendData(new Response { Command = Command.GetInvite, Status = Status.OK, Data = new Prop { Id = contact.Id, Name = contact.UserFrom.Name } }, toIp);
            }
        }

        /// <summary>
        /// Handler for sending messages between users.
        /// </summary>
        /// <param name="req">Request contains two user ids and text message</param>
        /// <param name="ip">UserTo IP</param>
        private void SendMessageToContactHandle(Request req, IPEndPoint ip)
        {
            // to user who sent
            //SendData(new Response { Command = Command.SendMessageToContact, Status = Status.OK, Data = "Message sent" }, ip);

            // to user for whom sent

            int from = req.Data.From;
            int to = req.Data.To;
            string message = req.Data.Message;

            IPEndPoint toIp;
            if (clients.TryGetValue(to, out toIp!))
            {
                SendData(new Response { Command = Command.GetMessageFromContact, Status = Status.OK, Data = new { Id = from, Message = message } }, toIp);
            }
        }

        /// <summary>
        /// Handler for sending messages from users to group.
        /// </summary>
        /// <param name="req">Request contains user id, group id and text message</param>
        /// <param name="ip">UserTo IP</param>
        private void SendMessageToGroupHandle(Request req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int group = req.Data.To;
            string message = req.Data.Message;

            IPEndPoint toIp;
            IEnumerable<int> ids = db.GetGroupMembersIds(group);
            foreach (int id in ids)
            {
                if (id != from && clients.TryGetValue(id, out toIp!))
                {
                    SendData(new Response { Command = Command.GetMessageFromGroup, Status = Status.OK, Data = new { Id = group, Message = message } }, toIp);
                }
            }
        }

        /// <summary>
        /// Login processing, password verifying.
        /// Sending data about contacts, groups and invites.
        /// <see cref="DatabaseService.GetUser(string)">Appropriate method from DBService</see>
        /// <see cref="DatabaseService.GetUserGroups(int)">Appropriate method from DBService</see>
        /// <see cref="DatabaseService.GetInvites(int)">Appropriate method from DBService</see>
        /// <see cref="DatabaseService.GetContacts(int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user name and password</param>
        /// <param name="ip">UserTo IP</param>
        private void SignInHandle(Request req, IPEndPoint ip)
        {
            string name = req.Data.Name;
            string password = req.Data.Password;

            Response res;
            User? user = db.GetUser(name);
            if (user != null)
            {
                if (Hasher.Verify(password, user.Password))
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
                    res = new Response { Command = Command.SignIn, Status = Status.Bad, Data = "Wrong password" };
                }
            }
            else
            {
                res = new Response { Command = Command.SignIn, Status = Status.Bad, Data = "User not found" };
            }

            SendData(res, ip);
        }

        /// <summary>
        /// Processing a registration request.
        /// Creating new user.
        /// <see cref="DatabaseService.AddUser(string, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains new user name and password</param>
        /// <param name="ip">UserTo IP</param>
        private void SignUpHandle(Request req, IPEndPoint ip)
        {
            string name = req.Data.Name;
            string password = req.Data.Password;
            User user = db.AddUser(name, password);

            // add user to connected clients
            clients.TryAdd(user.Id, ip);

            SendData(new Response
            {
                Command = Command.SignUp,
                Status = Status.OK,
                Data = new Prop
                {
                    Id = user.Id,
                    Name = user.Name
                }
            }, ip);
        }

        /// <summary>
        /// Send data size to clientTo.
        /// Send clientTo address to clientFrom
        /// </summary>
        private void GetFileMessageSizeHandle(Request req, IPEndPoint ip)
        {
            int to = req.Data.To;
            long size = req.Data.Size;

            IPEndPoint toIp;
            if (clients.TryGetValue(to, out toIp))
            {
                // send data size to clientTo
                SendData(new Response
                {
                    Command = Command.GetFileMessageSize,
                    Status = Status.OK,
                    Data = new { Size = size }
                }, toIp);

                // send clientTo destination to clientFrom
                SendData(new Response
                {
                    Command = Command.GetClientAddress,
                    Status = Status.OK,
                    Data = new Destination { Ip = toIp.Address.ToString(), Port = toIp.Port }
                }, ip);
            }
            else
            {
            }
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
                logger.LogRequest(req, ex);
            }
        }

        public void Listen()
        {
            if (listeningSocket != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                IPAddress ip;
                if (IPAddress.TryParse(ServerDestination.Ip, out ip!))
                {
                    IPEndPoint localIpEndPoint = new(ip, ServerDestination.Port);
                    listeningSocket.Bind(localIpEndPoint);
                    try
                    {
                        while (run)
                        {
                            StringBuilder builder = new();
                            int bytes = 0;
                            byte[] data = new byte[1024];
                            EndPoint clientIp = new IPEndPoint(IPAddress.Any, 0);

                            do
                            {
                                bytes = listeningSocket.ReceiveFrom(data, ref clientIp);
                                builder.Append(CommunicationEncoding.GetString(data, 0, bytes));
                            } while (listeningSocket.Available > 0);
                            IPEndPoint clientFullIp = (IPEndPoint)clientIp;

                            string request = builder.ToString();
                            Console.WriteLine(string.Format("{0} = {1}", clientIp.ToString(), request));
                            Handle(request, clientFullIp);
                        }
                    }
                    catch (SocketException socketEx)
                    {
                        if (socketEx.ErrorCode != 10004)
                            Console.WriteLine(socketEx.Message + socketEx.ErrorCode);

                        logger.LogException(socketEx);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        logger.LogException(ex);
                    }
                    finally
                    {
                        CloseConnection();
                    }
                }
            }
        }

        private void SendData(Response response, IPEndPoint ip)
        {
            if (listeningSocket != null)
            {
                string responseStr = JsonConvert.SerializeObject(response);
                byte[] data = CommunicationEncoding.GetBytes(responseStr);

                // send to one
                listeningSocket.SendTo(data, ip);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(responseStr);
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
        }

        public CommunicationService()
        {
            run = true;
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // init handlers
            handlers = new()
            {
                { Command.SignIn, SignInHandle },
                { Command.SignUp, SignUpHandle },
                { Command.SendMessageToContact, SendMessageToContactHandle },
                { Command.SendMessageToGroup, SendMessageToGroupHandle },
                { Command.SendInvite, SendInviteHandle },
                { Command.AcceptInvite, AcceptInviteHandle },
                { Command.RenameContact, RenameContactHandle },
                { Command.RemoveContact, RemoveContactHandle },
                { Command.CreateGroup, CreateGroupHandle },
                { Command.LeaveGroup, LeaveGroupHandle },
                { Command.EnterGroup, EnterGroupHandle },
                { Command.Disconnect, DisconnectHandle },
                { Command.RenameGroup, RenameGroupHandle },
                { Command.GetFileMessageSize, GetFileMessageSizeHandle },
            };
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