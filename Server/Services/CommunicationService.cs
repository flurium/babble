using CrossLibrary;
using Newtonsoft.Json;
using Server.Models;
using Server.Services.Database;
using Server.Services.Exceptions;
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
        private readonly Dictionary<int, LinkedList<Transaction>> pendingMessages = new();
        private readonly DatabaseService db = new();
        private readonly Dictionary<Command, Action<Transaction, IPEndPoint>> handlers = new();
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
        private async void AcceptInviteHandle(Transaction req, IPEndPoint ip)
        {
            // ip = ip of user to
            // req.Data = contact id
            int id = req.Data.Id;
            Contact contact = await db.AcceptInviteAsync(id);

            // to user to
            SendData(new Transaction { Command = Command.GetContact, Data = new Prop { Id = contact.UserFromId, Name = contact.NameAtUserTo } }, ip);

            // to user from
            if (clients.ContainsKey(contact.UserFromId))
            {
                // only if user from is online
                SendData(
                  new Transaction { Command = Command.GetContact, Data = new Prop { Id = contact.UserToId, Name = contact.NameAtUserFrom } },
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
        private async void CreateGroupHandle(Transaction req, IPEndPoint ip)
        {
            int uid = req.Data.User;
            string name = req.Data.Group;
            Prop group = await db.CreateGroupAsync(uid, name);
            SendData(new Transaction { Command = Command.CreateGroup, Data = group }, ip);
        }

        /// <summary>
        /// Handler for adding new user to existing group.
        /// <see cref="DatabaseService.AddUserToGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void EnterGroupHandle(Transaction req, IPEndPoint ip)
        {
            int uid = req.Data.User;
            string groupName = req.Data.Group;
            Prop group = await db.AddUserToGroupAsync(uid, groupName);

            SendData(new Transaction { Command = Command.EnterGroup, Data = group }, ip);
        }

        /// <summary>
        /// Handler for removing user from existing group.
        /// <see cref="DatabaseService.RemoveUserFromGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void LeaveGroupHandle(Transaction req, IPEndPoint ip)
        {
            int uid = req.Data.Id;
            string name = req.Data.Name;
            await db.RemoveUserFromGroupAsync(uid, name);
            SendData(new Transaction { Command = Command.LeaveGroup, Data = "Group is removed" }, ip);
        }

        /// <summary>
        /// Handler for disconnecting.
        /// </summary>
        /// <param name="req">Request contains user id</param>
        /// <param name="ip">User IP</param>
        private void DisconnectHandle(Transaction req, IPEndPoint ip)
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
        private async void RemoveContactHandle(Transaction req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            await db.RemoveContact(from, to);
            SendData(new Transaction { Command = Command.RemoveContact, Data = "Contact is removed" }, ip);
        }

        /// <summary>
        /// Handler for renaming contact in user's contact list.
        /// <see cref="DatabaseService.RenameContact(int, int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids and new contact name</param>
        /// <param name="ip">UserTo IP</param>
        private void RenameContactHandle(Transaction req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            string newName = req.Data.NewName;
            db.RenameContact(from, to, newName);
            SendData(new Transaction { Command = Command.RenameContact, Data = new Prop { Id = to, Name = newName } }, ip);
        }

        /// <summary>
        /// Handler for renaming group in user's group list.
        /// <see cref="DatabaseService.RenameGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new group name</param>
        /// <param name="ip">UserTo IP</param>
        private async void RenameGroupHandle(Transaction req, IPEndPoint ip)
        {
            int id = req.Data.Id;
            string name = req.Data.NewName;
            await db.RenameGroupAsync(id, name);
            SendData(new Transaction { Command = Command.RenameGroup, Data = new Prop { Id = id, Name = name } }, ip);
        }

        /// <summary>
        /// Handler for sending invitations between users.
        /// Send two responses
        /// <see cref="DatabaseService.SendInviteAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
        /// <param name="ip">UserTo IP</param>
        private async void SendInviteHandle(Transaction req, IPEndPoint ip)
        {
            int from = req.Data.From;
            string to = req.Data.To;
            Contact contact = await db.SendInviteAsync(from, to);

            // to who sent request
            SendData(new Transaction { Command = Command.SendInvite, Data = new { Message = "Invite was send successfully" } }, ip);

            // to who will get invite
            IPEndPoint toIp;
            if (clients.TryGetValue(contact.UserToId, out toIp!))
            {
                SendData(new Transaction { Command = Command.GetInvite, Data = new Prop { Id = contact.Id, Name = contact.UserFrom.Name } }, toIp);
            }
        }

        /// <summary>
        /// Handler for sending messages between users.
        /// </summary>
        /// <param name="req">Request contains two user ids and text message</param>
        /// <param name="ip">UserTo IP</param>
        private void SendMessageToContactHandle(Transaction req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            string message = req.Data.Message;

            Transaction transaction = new() { Command = Command.GetMessageFromContact, Data = new { Id = from, Message = message } };

            IPEndPoint toIp;
            if (clients.TryGetValue(to, out toIp!))
            {
                SendData(transaction, toIp);
            }
            else
            {
                if (pendingMessages.ContainsKey(to))
                {
                    pendingMessages[to].AddLast(transaction);
                }
                else
                {
                    LinkedList<Transaction> messages = new();
                    messages.AddLast(transaction);
                    pendingMessages.Add(to, messages);
                }
            }
        }

        /// <summary>
        /// Handler for sending messages from users to group.
        /// </summary>
        /// <param name="req">Request contains user id, group id and text message</param>
        /// <param name="ip">UserTo IP</param>
        private void SendMessageToGroupHandle(Transaction req, IPEndPoint ip)
        {
            int from = req.Data.From;
            int group = req.Data.To;
            string message = req.Data.Message;

            Transaction transaction = new() { Command = Command.GetMessageFromGroup, Data = new { Id = group, Message = message } };

            IPEndPoint toIp;
            IEnumerable<int> ids = db.GetGroupMembersIds(group);
            foreach (int id in ids)
            {
                if (id != from)
                {
                    if (clients.TryGetValue(id, out toIp!))
                    {
                        SendData(transaction, toIp);
                    }
                    else
                    {
                        pendingMessages[id].AddLast(transaction);
                    }
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
        private void SignInHandle(Transaction req, IPEndPoint ip)
        {
            string name = req.Data.Name;
            string password = req.Data.Password;

            User? user = db.GetUser(name);

            if (user == null)
            {
                SendData(new Transaction { Command = Command.SignIn, Data = new { IsOk = false, Message = "User not found" } }, ip);
                return;
            }

            if (!Hasher.Verify(password, user.Password))
            {
                SendData(new Transaction { Command = Command.SignIn, Data = new { IsOk = false, Message = "Wrong password" } }, ip);
                return;
            }

            // add user to connected clients
            clients.Add(user.Id, ip);

            Transaction res = new()
            {
                Command = req.Command,
                Data = new
                {
                    IsOk = true,
                    User = new Prop { Id = user.Id, Name = user.Name },
                    Groups = db.GetUserGroups(user.Id),
                    Invites = db.GetInvites(user.Id),
                    Contacts = db.GetContacts(user.Id)
                }
            };
            SendData(res, ip);

            LinkedList<Transaction> messages;
            if (pendingMessages.TryGetValue(user.Id, out messages))
            {
                foreach (var message in messages)
                {
                    SendData(message, ip);
                }
                pendingMessages.Remove(user.Id);
            }
        }

        /// <summary>
        /// Processing a registration request.
        /// Creating new user.
        /// <see cref="DatabaseService.AddUser(string, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains new user name and password</param>
        /// <param name="ip">UserTo IP</param>
        private void SignUpHandle(Transaction req, IPEndPoint ip)
        {
            try
            {
                string name = req.Data.Name;
                string password = req.Data.Password;
                User user = db.AddUser(name, password);

                // add user to connected clients
                clients.TryAdd(user.Id, ip);

                SendData(new Transaction
                {
                    Command = Command.SignUp,
                    Data = new
                    {
                        IsOk = true,
                        Id = user.Id,
                        Name = user.Name
                    }
                }, ip);
            }
            catch (Exception ex)
            {
                SendData(new Transaction
                {
                    Command = Command.SignUp,
                    Data = new
                    {
                        IsOk = false,
                        Message = ex.Message
                    }
                }, ip);
            }
        }

        /// <summary>
        /// Send data size to clientTo.
        /// Send clientTo address to clientFrom
        /// </summary>
        private void GetFileMessageSizeHandle(Transaction req, IPEndPoint ip)
        {
            int to = req.Data.To;
            long size = req.Data.Size;

            IPEndPoint toIp;
            if (clients.TryGetValue(to, out toIp))
            {
                // send data size to clientTo
                SendData(new Transaction
                {
                    Command = Command.GetFileMessageSize,
                    Data = new { Size = size }
                }, toIp);

                // send clientTo destination to clientFrom
                SendData(new Transaction
                {
                    Command = Command.GetClientAddress,
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
            Transaction req = JsonConvert.DeserializeObject<Transaction>(reqStr);
            try
            {
                handlers[req.Command](req, ip);
            }
            catch (InfoException ex)
            {
                SendData(new Transaction { Command = Command.Exception, Data = ex.Message }, ip);
                logger.LogRequest(req, ex);
            }
            catch (Exception ex)
            {
                SendData(new Transaction { Command = Command.Exception, Data = "Sorry, something went wrong." }, ip);
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

        private void SendData(Transaction response, IPEndPoint ip)
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