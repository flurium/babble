using CrossLibrary;
using CrossLibrary.Network;
using Newtonsoft.Json;
using Server.Models;
using Server.Services.Communication;
using Server.Services.Exceptions;
using System.Net;
using static CrossLibrary.Globals;

namespace Server.Services.Network
{
    internal class UdpHandler : ProtocolHandler<Store>
    {
        private Dictionary<Command, Action<Transaction>> handlers;

        public UdpHandler(string ip, int port, Store store) : base(ip, port, store)
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
                { Command.ContactFileSize, ContactFileSizeHandle },
                { Command.GroupFileSize, GroupFileSizeHandle },
            };
        }

        protected override ProtocolService CreateProtocolService(string ip, int port, Action<string> handle) => new UdpService(ip, port, handle);

        private void Send(Transaction transaction)
        {
            //        // BAD: TO JSON 2 TIMES
            //        // FIX !!!!!!!!!!!

            string json = transaction.ToJson();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(json);
            Console.ForegroundColor = ConsoleColor.Cyan;
            protocol.Send(transaction.ToStrBytes());
        }

        public void Send(Transaction transaction, IPEndPoint endPoint)
        {
            protocol.Send(transaction.ToStrBytes(), endPoint);
        }

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
        /// <see cref="Database.DatabaseService.AcceptInviteAsync(int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains id</param>
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
        /// <see cref="Database.DatabaseService.CreateGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new name of group</param>
        private async void CreateGroupHandle(Transaction req)
        {
            int uid = req.Data.User;
            string name = req.Data.Group;
            Prop group = await store.db.CreateGroupAsync(uid, name);
            Send(new Transaction { Command = Command.CreateGroup, Data = group });
        }

        /// <summary>
        /// Handler for adding new user to existing group.
        /// <see cref="Database.DatabaseService.AddUserToGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
        private async void EnterGroupHandle(Transaction req)
        {
            int uid = req.Data.User;
            string groupName = req.Data.Group;
            Prop group = await store.db.AddUserToGroupAsync(uid, groupName);

            Send(new Transaction { Command = Command.EnterGroup, Data = group });
        }

        /// <summary>
        /// Handler for removing user from existing group.
        /// <see cref="Database.DatabaseService.RemoveUserFromGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and name of group</param>
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
        private void DisconnectHandle(Transaction req)
        {
            int id = req.Data.Id;
            store.clients.Remove(id);
        }

        /// <summary>
        /// Handler for removing contact from user's contact list.
        /// <see cref="Database.DatabaseService.RemoveContact(int, int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
        private async void RemoveContactHandle(Transaction req)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            await store.db.RemoveContact(from, to);
            Send(new Transaction { Command = Command.RemoveContact, Data = "Contact is removed" });
        }

        /// <summary>
        /// Handler for renaming contact in user's contact list.
        /// <see cref="Database.DatabaseService.RenameContact(int, int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids and new contact name</param>
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
        /// <see cref="Database.DatabaseService.RenameGroupAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user id and new group name</param>
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
        /// <see cref="Database.DatabaseService.SendInviteAsync(int, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains two user ids</param>
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
        private void SendMessageToContactHandle(Transaction req)
        {
            int from = req.Data.From;
            int to = req.Data.To;
            string message = req.Data.Message;
            DateTime time = req.Data.Time;

            Transaction transaction = new()
            {
                Command = Command.GetMessageFromContact,
                Data = new
                {
                    Id = from,
                    Message = message,
                    Time = time,
                    User = store.db.GetUser(from)?.Name
                }
            };

            IPEndPoint toIp;
            if (store.clients.TryGetValue(to, out toIp!))
            {
                Send(transaction, toIp);
            }
            else
            {
                Guid guid = Guid.NewGuid();
                store.pendingTransactions.Add(guid, new() { Count = 1, Transaction = transaction });

                if (store.pending.ContainsKey(to))
                {
                    store.pending[to].AddLast(guid);
                }
                else
                {
                    LinkedList<Guid> guids = new();
                    guids.AddLast(guid);
                    store.pending.Add(to, guids);
                }
            }
        }

        /// <summary>
        /// Handler for sending messages from users to group.
        /// </summary>
        /// <param name="req">Request contains user id, group id and text message</param>
        private void SendMessageToGroupHandle(Transaction req)
        {
            int from = req.Data.From;
            int group = req.Data.To;
            string message = req.Data.Message;
            DateTime time = req.Data.Time;

            Transaction transaction = new()
            {
                Command = Command.GetMessageFromGroup,
                Data = new
                {
                    Id = group,
                    Message = message,
                    Time = time,
                    User = store.db.GetUser(from)?.Name
                }
            };

            bool isGoToPending = false;
            Guid guid = Guid.NewGuid();
            PendingTransaction pendingTransaction = new() { Count = 0 };

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
                        pendingTransaction.Count += 1;
                        if (!isGoToPending)
                        {
                            pendingTransaction.Transaction = transaction;
                            isGoToPending = true;
                        }

                        if (store.pending.ContainsKey(id))
                        {
                            store.pending[id].AddLast(guid);
                        }
                        else
                        {
                            LinkedList<Guid> guids = new();
                            guids.AddLast(guid);
                            store.pending.Add(id, guids);
                        }
                    }
                }
            }

            if (isGoToPending)
            {
                store.pendingTransactions.Add(guid, pendingTransaction);
            }
        }

        /// <summary>
        /// Login processing, password verifying.
        /// Sending data about contacts, groups and invites.
        /// <see cref="Database.DatabaseService.GetUser(string)">Appropriate method from DBService</see>
        /// <see cref="Database.DatabaseService.GetUserGroups(int)">Appropriate method from DBService</see>
        /// <see cref="Database.DatabaseService.GetInvites(int)">Appropriate method from DBService</see>
        /// <see cref="Database.DatabaseService.GetContacts(int)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains user name and password</param>
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
                    User = new Prop { Id = user.Id, Name = user.Name }
                }
            };
            Send(res);

            var groups = store.db.GetUserGroups(user.Id);
            foreach (var group in groups)
            {
                Send(new Transaction { Command = Command.EnterGroup, Data = group });
            }

            var invites = store.db.GetInvites(user.Id);
            foreach (var invite in invites)
            {
                Send(new Transaction { Command = Command.GetInvite, Data = invite });
            }

            var contacts = store.db.GetContacts(user.Id);
            foreach (var contact in contacts)
            {
                Send(new Transaction { Command = Command.GetContact, Data = contact });
            }

            // send pending messages
            if (store.pending.TryGetValue(user.Id, out LinkedList<Guid> guids))
            {
                foreach (var guid in guids)
                {
                    PendingTransaction pendingTransaction = store.pendingTransactions[guid];
                    pendingTransaction.Count -= 1;

                    // GetMessageFromContact
                    if (pendingTransaction.Transaction.Command == Command.GetMessageFromContact || pendingTransaction.Transaction.Command == Command.GetMessageFromGroup)
                    {
                        Send(pendingTransaction.Transaction);
                    }
                    else if (pendingTransaction.Transaction.Command == Command.SendFileMessageToContact || pendingTransaction.Transaction.Command == Command.SendFileMessageToGroup)
                    {
                        store.tcpHandler.Send(pendingTransaction.Transaction.ToStrBytes(), protocol.RemoteIpEndPoint);
                    }

                    if (pendingTransaction.Count == 0)
                    {
                        store.pendingTransactions.Remove(guid);
                    }
                    else
                    {
                        store.pendingTransactions[guid] = pendingTransaction;
                    }
                }
                store.pending.Remove(user.Id);
            }
        }

        /// <summary>
        /// Processing a registration request.
        /// Creating new user.
        /// <see cref="Database.DatabaseService.AddUser(string, string)">Appropriate method from DBService</see>
        /// </summary>
        /// <param name="req">Request contains new user name and password</param>
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
                        user.Id,
                        user.Name
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
                        ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Send data size to clientTo.
        /// Send clientTo address to clientFrom.
        /// </summary>
        private void FileSizeHandle(int to, long size)
        {
            IPEndPoint toIp;
            if (store.clients.TryGetValue(to, out toIp))
            {
                // send data size to clientTo
                Send(new Transaction
                {
                    Command = Command.ContactFileSize,
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
                // get file message to server
                store.tcpHandler.UpdateBufferSize(size);
                store.pendingClients.Enqueue(new() { to });

                // send server destination to clientFrom
                Send(new Transaction
                {
                    Command = Command.GetClientAddress,
                    Data = ServerDestination
                });
            }
        }

        private void ContactFileSizeHandle(Transaction req)
        {
            int to = req.Data.To;
            long size = req.Data.Size;
            FileSizeHandle(to, size);
        }

        private void GroupFileSizeHandle(Transaction transaction)
        {
            int to = transaction.Data.To;
            long size = transaction.Data.Size;
            int from = transaction.Data.From;

            List<int> pendingGroupClients = new List<int>();
            bool isServerIn = false;
            LinkedList<Destination> destinations = new();
            IPEndPoint toEndPoint;

            var ids = store.db.GetGroupMembersIds(to);
            foreach (int id in ids)
            {
                if (id != from)
                {
                    if (store.clients.TryGetValue(id, out toEndPoint))
                    {
                        destinations.AddLast(new Destination { Ip = toEndPoint.Address.ToString(), Port = toEndPoint.Port });

                        // send data size to clientTo
                        Send(new Transaction
                        {
                            Command = Command.ContactFileSize,
                            Data = new { Size = size }
                        }, toEndPoint);
                    }
                    else
                    {
                        if (!isServerIn)
                        {
                            destinations.AddLast(ServerDestination);
                            isServerIn = true;
                            store.tcpHandler.UpdateBufferSize(size);
                        }
                        pendingGroupClients.Add(id);
                    }
                }
            }

            if (isServerIn)
            {
                store.pendingClients.Enqueue(pendingGroupClients);
            }

            Send(new Transaction
            {
                Command = Command.GetGroupAddress,
                Data = new { Destinations = destinations }
            });
        }
    }
}