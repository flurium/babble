using CrossLibrary;
using CrossLibrary.Network;
using Newtonsoft.Json;
using Server.Models;
using Server.Services.Communication;
using Server.Services.Exceptions;
using Server.Services.Network.Base;
using System.Net;

namespace Server.Services.Network.Udp
{
    internal class UdpHandler : ProtocolHandler
    {
        private Dictionary<Command, Action<Transaction>> handlers;
        private readonly Store store;

        public UdpHandler(string ip, int port, Store store) : base(ip, port)
        {
            this.store = store;
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

        protected override ProtocolService CreateProtocolService(string ip, int port, Action<string> handle) => new UdpService(ip, port, handle);

        protected override void Handle(string str)
        {
            Console.WriteLine(str);
            Transaction req = JsonConvert.DeserializeObject<Transaction>(str);
            try
            {
                handlers[req.Command](req);
            }
            catch (InfoException ex)
            {
                Send(new Transaction { Command = Command.Exception, Data = ex.Message });
                Logger.LogTransaction(req, ex);
            }
            catch (Exception ex)
            {
                Send(new Transaction { Command = Command.Exception, Data = "Sorry, something went wrong." });
                Logger.LogTransaction(req, ex);
            }
        }

        /// <summary>
        /// Handler for accepting invite by user id.
        /// Send two responses
        /// <see cref="DatabaseService.AcceptInviteAsync(int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains id</param>
        /// <param name="ip">UserTo IP</param>
        private async void AcceptInviteHandle(Transaction req)
        {
            // ip = ip of user to
            // req.Data = contact id
            int id = req.Data.Id;
            Contact contact = await store.db.AcceptInviteAsync(id);

            // to user to
            Send(new Transaction { Command = Command.GetContact, Data = new Prop { Id = contact.UserFromId, Name = contact.NameAtUserTo } });

            // to user from
            if (store.clients.ContainsKey(contact.UserFromId))
            {
                // only if user from is online
                Send(
                  new Transaction { Command = Command.GetContact, Data = new Prop { Id = contact.UserToId, Name = contact.NameAtUserFrom } },
                  store.clients[contact.UserFromId]
                );
            }
        }

        /// <summary>
        /// Handler for creating new group by user id and name of group.
        /// <see cref="DatabaseService.CreateGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void CreateGroupHandle(Transaction req)
        {
            int uid = req.Data.User;
            string name = req.Data.Group;
            Prop group = await store.db.CreateGroupAsync(uid, name);
            Send(new Transaction { Command = Command.CreateGroup, Data = group });
        }

        /// <summary>
        /// Handler for adding new user to existing group.
        /// <see cref="DatabaseService.AddUserToGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void EnterGroupHandle(Transaction req)
        {
            int uid = req.Data.User;
            string groupName = req.Data.Group;
            Prop group = await store.db.AddUserToGroupAsync(uid, groupName);

            Send(new Transaction { Command = Command.EnterGroup, Data = group });
        }

        /// <summary>
        /// Handler for removing user from existing group.
        /// <see cref="DatabaseService.RemoveUserFromGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        /// <param name="ip">UserTo IP</param>
        private async void LeaveGroupHandle(Transaction req)
        {
            int uid = req.Data.Id;
            string name = req.Data.Name;
            await store.db.RemoveUserFromGroupAsync(uid, name);
            Send(new Transaction { Command = Command.LeaveGroup, Data = "Group is removed" });
        }

        /// <summary>
        /// Handler for disconnecting.
        /// </summary>
        /// <param name="req">Request contains user id</param>
        /// <param name="ip">User IP</param>
        private void DisconnectHandle(Transaction req)
        {
            int id = req.Data.Id;
            store.clients.Remove(id);
        }

        /// <summary>
        /// Handler for removing contact from user's contact list.
        /// <see cref="DatabaseService.RemoveContact(int, int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
        /// <param name="ip">UserTo IP</param>
        private async void RemoveContactHandle(Transaction req)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            await store.db.RemoveContact(from, to);
            Send(new Transaction { Command = Command.RemoveContact, Data = "Contact is removed" });
        }

        /// <summary>
        /// Handler for renaming contact in user's contact list.
        /// <see cref="DatabaseService.RenameContact(int, int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids and new contact name</param>
        /// <param name="ip">UserTo IP</param>
        private void RenameContactHandle(Transaction req)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            string newName = req.Data.NewName;
            store.db.RenameContact(from, to, newName);
            Send(new Transaction { Command = Command.RenameContact, Data = new Prop { Id = to, Name = newName } });
        }

        /// <summary>
        /// Handler for renaming group in user's group list.
        /// <see cref="DatabaseService.RenameGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new group name</param>
        /// <param name="ip">UserTo IP</param>
        private async void RenameGroupHandle(Transaction req)
        {
            int id = req.Data.Id;
            string name = req.Data.NewName;
            await store.db.RenameGroupAsync(id, name);
            Send(new Transaction { Command = Command.RenameGroup, Data = new Prop { Id = id, Name = name } });
        }

        /// <summary>
        /// Handler for sending invitations between users.
        /// Send two responses
        /// <see cref="DatabaseService.SendInviteAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
        /// <param name="ip">UserTo IP</param>
        private async void SendInviteHandle(Transaction req)
        {
            int from = req.Data.From;
            string to = req.Data.To;
            Contact contact = await store.db.SendInviteAsync(from, to);

            // to who sent request
            Send(new Transaction { Command = Command.SendInvite, Data = new { Message = "Invite was send successfully" } });

            // to who will get invite
            IPEndPoint toIp;
            if (store.clients.TryGetValue(contact.UserToId, out toIp!))
            {
                Send(new Transaction { Command = Command.GetInvite, Data = new Prop { Id = contact.Id, Name = contact.UserFrom.Name } }, toIp);
            }
        }

        /// <summary>
        /// Handler for sending messages between users.
        /// </summary>
        /// <param name="req">Request contains two user ids and text message</param>
        /// <param name="ip">UserTo IP</param>
        private void SendMessageToContactHandle(Transaction req)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            string message = req.Data.Message;

            Transaction transaction = new() { Command = Command.GetMessageFromContact, Data = new { Id = from, Message = message } };

            IPEndPoint toIp;
            if (store.clients.TryGetValue(to, out toIp!))
            {
                Send(transaction, toIp);
            }
            else
            {
                if (store.pending.ContainsKey(to))
                {
                    store.pending[to].AddLast(transaction);
                }
                else
                {
                    LinkedList<Transaction> messages = new();
                    messages.AddLast(transaction);
                    store.pending.Add(to, messages);
                }
            }
        }

        /// <summary>
        /// Handler for sending messages from users to group.
        /// </summary>
        /// <param name="req">Request contains user id, group id and text message</param>
        /// <param name="ip">UserTo IP</param>
        private void SendMessageToGroupHandle(Transaction req)
        {
            int from = req.Data.From;
            int group = req.Data.To;
            string message = req.Data.Message;

            Transaction transaction = new() { Command = Command.GetMessageFromGroup, Data = new { Id = group, Message = message } };

            IPEndPoint toIp;
            IEnumerable<int> ids = store.db.GetGroupMembersIds(group);
            foreach (int id in ids)
            {
                if (id != from)
                {
                    if (store.clients.TryGetValue(id, out toIp!))
                    {
                        Send(transaction, toIp);
                    }
                    else
                    {
                        if (store.pending.ContainsKey(id))
                        {
                            store.pending[id].AddLast(transaction);
                        }
                        else
                        {
                            LinkedList<Transaction> messages = new();
                            messages.AddLast(transaction);
                            store.pending.Add(id, messages);
                        }
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
        private void SignInHandle(Transaction req)
        {
            string name = req.Data.Name;
            string password = req.Data.Password;

            User? user = store.db.GetUser(name);

            if (user == null)
            {
                Send(new Transaction { Command = Command.SignIn, Data = new { IsOk = false, Message = "User not found" } });
                return;
            }

            if (!Hasher.Verify(password, user.Password))
            {
                Send(new Transaction { Command = Command.SignIn, Data = new { IsOk = false, Message = "Wrong password" } });
                return;
            }

            // add user to connected clients
            store.clients.Add(user.Id, protocol.RemoteIpEndPoint);

            Transaction res = new()
            {
                Command = req.Command,
                Data = new
                {
                    IsOk = true,
                    User = new Prop { Id = user.Id, Name = user.Name },
                    Groups = store.db.GetUserGroups(user.Id),
                    Invites = store.db.GetInvites(user.Id),
                    Contacts = store.db.GetContacts(user.Id)
                }
            };
            Send(res);

            // send pending messages
            LinkedList<Transaction> messages;
            if (store.pending.TryGetValue(user.Id, out messages))
            {
                foreach (var message in messages)
                {
                    Send(message);
                }
                store.pending.Remove(user.Id);
            }
        }

        /// <summary>
        /// Processing a registration request.
        /// Creating new user.
        /// <see cref="DatabaseService.AddUser(string, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains new user name and password</param>
        /// <param name="ip">UserTo IP</param>
        private void SignUpHandle(Transaction req)
        {
            try
            {
                string name = req.Data.Name;
                string password = req.Data.Password;
                User user = store.db.AddUser(name, password);

                // add user to connected clients
                store.clients.TryAdd(user.Id, protocol.RemoteIpEndPoint);

                Send(new Transaction
                {
                    Command = Command.SignUp,
                    Data = new
                    {
                        IsOk = true,
                        Id = user.Id,
                        Name = user.Name
                    }
                });
            }
            catch (Exception ex)
            {
                Send(new Transaction
                {
                    Command = Command.SignUp,
                    Data = new
                    {
                        IsOk = false,
                        Message = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Send data size to clientTo.
        /// Send clientTo address to clientFrom
        /// </summary>
        private void GetFileMessageSizeHandle(Transaction req)
        {
            int to = req.Data.To;
            long size = req.Data.Size;

            IPEndPoint toIp;
            if (store.clients.TryGetValue(to, out toIp))
            {
                // send data size to clientTo
                Send(new Transaction
                {
                    Command = Command.GetFileMessageSize,
                    Data = new { Size = size }
                }, toIp);

                // send clientTo destination to clientFrom
                Send(new Transaction
                {
                    Command = Command.GetClientAddress,
                    Data = new Destination { Ip = toIp.Address.ToString(), Port = toIp.Port }
                });
            }
            else
            {
            }
        }
    }
}