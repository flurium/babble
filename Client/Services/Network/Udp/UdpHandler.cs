using Client.Models;
using Client.Services.Network.Base;
using Client.Services.Network.Udp;
using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Client.Services
{
    public class UdpHandler : ProtocolHandler
    {
        private Dictionary<Command, Action<Response>> handlers;

        public UdpHandler(int port, Store store) : base(port, store)
        {
            handlers = new()
                {
                    { Command.GetMessageFromContact, GetMessageFromContactHandle },
                    { Command.GetMessageFromGroup, GetMessageFromGroupHandle },
                    { Command.SignIn, SignInHandle },
                    { Command.SignUp, SignUpHandle },
                    { Command.SendInvite, SendInviteHandle },
                    { Command.GetInvite, GetInviteHandle },
                    { Command.GetContact, GetContactHandle },
                    { Command.CreateGroup, CreateGroupHandle },
                    { Command.EnterGroup, EnterGroupHandle },
                    { Command.RemoveContact, RemoveContactHandle },
                    { Command.RenameContact, RenameContactHandle },
                    { Command.RenameGroup, RenameGroupHandle },
                    { Command.GetFileMessageSize,  GetFileMessageSizeHandle },
                    { Command.GetClientAddress, GetClientAddressHandle }
                };
        }

        protected override ProtocolService CreateProtocolService(int port, Action<string> handle) => new UdpService(port, handle);

        protected override void Handle(string str)
        {
            try
            {
                Response res = JsonConvert.DeserializeObject<Response>(str);
                handlers[res.Command](res);
            }
            catch (Exception ex)
            {
                // TODO:
                // Show error
                throw;
            }
        }

        private Prop GetProp(Response res) => new() { Id = res.Data.Id, Name = res.Data.Name };

        private void GetFileMessageSizeHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                long size = res.Data.Size;
                store.tcpHandler.UpdateBufferSize(size);
            }
            else
            {
            }
        }

        private void GetClientAddressHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Destination destination = new()
                {
                    Ip = res.Data.Ip,
                    Port = res.Data.Port
                };
                store.tcpHandler.UpdateDestination(destination);
                store.tcpHandler.Send(store.pendingSendFile);
            }
        }

        private void AddToCollection(Prop prop, ref ObservableCollection<Prop> collection) => collection.Add(prop);

        /// <summary>
        /// Handler for incoming props from the server.
        /// Create new entries in "dictionary" and "collection"
        /// </summary>
        private void NewChatHandle(Response res, ref Dictionary<int, LinkedList<Message>> dictionary, ref ObservableCollection<Prop> collection)
        {
            if (res.Status == Status.OK)
            {
                Prop chat = GetProp(res);

                Delegate addDelegate = AddToCollection;
                object[] addParams = new object[] { chat, collection };

                dictionary.Add(chat.Id, new());
                Application.Current.Dispatcher.Invoke(addDelegate, addParams);
            }
            else
            {
                // show error
                string message = (string)res.Data;
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
            }
        }

        private void GetContactHandle(Response res) => NewChatHandle(res, ref store.contactMessages, ref store.contacts);

        private void CreateGroupHandle(Response res) => NewChatHandle(res, ref store.groupMessages, ref store.groups);

        private void EnterGroupHandle(Response res) => NewChatHandle(res, ref store.groupMessages, ref store.groups);

        /// <summary>
        /// Deleting a contact handler.
        /// From the "Dictionary contactMessages" and "ObservableCollection<Prop> Contacts".
        /// </summary>
        /// <remarks>Accepts response from the contact id in the Data.Id</remarks>
        /// <param name="res"></param>
        private void RemoveContactHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Prop contact = new()
                {
                    Id = res.Data.Id
                };

                foreach (var cont in store.contacts)
                {
                    if (cont.Id == contact.Id)
                    {
                        store.contacts.Remove(cont);
                        store.contactMessages.Remove(cont.Id);
                        break;
                    }
                }

                Application.Current.Dispatcher.Invoke(() => store.contacts.Add(contact));
            }
            else
            {
                string message = (string)res.Data;
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
            }
        }

        /// <summary>
        /// Accepting an invitation from the server.
        /// Placement of the invitation in  "ObservableCollection<Prop> Invites"
        /// </summary>
        /// <param name="res"></param>
        private void GetInviteHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Prop invite = GetProp(res);
                Application.Current.Dispatcher.Invoke(() => store.invites.Add(invite));
            }
            else
            {
                // show error
            }
        }

        private void SendInviteHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                string message = res.Data.Message;
                MessageBox.Show(message);
            }
            else
            {
                string message = res.Data;
                MessageBox.Show(message);
            }
        }

        /// <summary>
        /// Inserting a message into "dictionary" and " ObservableCollection<Message> CurrentMessages"
        /// </summary>
        private void GetMessageHandle(Response res, ref Dictionary<int, LinkedList<Message>> dictionary)
        {
            int id = res.Data.Id;
            string text = res.Data.Message;
            Message message = new() { Text = text, IsIncoming = true };

            foreach (var propMessage in dictionary)
            {
                if (propMessage.Key == id)
                {
                    propMessage.Value.AddLast(message);
                    if (store.currentProp.Id == id)
                    {
                        Application.Current.Dispatcher.Invoke(() => store.currentMessages.Add(message));
                    }
                    return;
                }
            }
        }

        private void GetMessageFromContactHandle(Response res) => GetMessageHandle(res, ref store.contactMessages);

        private void GetMessageFromGroupHandle(Response res) => GetMessageHandle(res, ref store.groupMessages);

        private void UpdateCollectionProp(Prop prop, int i, ref ObservableCollection<Prop> collection) => collection[i] = prop;

        /// <summary>
        /// Processing a message from the server about renaming.
        /// Search for a prop in the ObservableCollection<Prop> collection and change the Name
        /// </summary>
        /// <param name="res"></param>
        private void RenameHandle(Response res, ref ObservableCollection<Prop> collection)
        {
            if (res.Status == Status.OK)
            {
                Prop newProp = GetProp(res);
                //int id = res.Data.Id;
                //string newName = res.Data.Name;

                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].Id == newProp.Id)
                    {
                        Delegate updateDelegate = UpdateCollectionProp;
                        object[] updateParams = new object[] { newProp, i, collection };

                        Application.Current.Dispatcher.Invoke(updateDelegate, updateParams);
                        break;
                    }
                }
            }
            else
            {
            }
        }

        private void RenameContactHandle(Response res) => RenameHandle(res, ref store.contacts);

        private void RenameGroupHandle(Response res) => RenameHandle(res, ref store.groups);

        /// <summary>
        /// Login processing from the server.
        /// Obtaining data about contacts, groups and invites, entering into the appropriate arrays.
        /// </summary>
        /// <param name="res"></param>
        private void SignInHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                store.user = new()
                {
                    Id = res.Data.User.Id,
                    Name = res.Data.User.Name
                };

                var resGroups = res.Data.Groups;
                foreach (var group in resGroups)
                {
                    Prop groupProp = new() { Id = group.Id, Name = group.Name };
                    store.groupMessages.Add(groupProp.Id, new());
                    Application.Current.Dispatcher.Invoke(() => store.groups.Add(groupProp));
                }

                var invites = res.Data.Invites;
                foreach (var invite in invites)
                {
                    Prop inviteProp = new() { Id = invite.Id, Name = invite.Name };
                    Application.Current.Dispatcher.Invoke(() => store.invites.Add(inviteProp));
                }

                var resContacts = res.Data.Contacts;
                foreach (var contact in resContacts)
                {
                    Prop contactProp = new() { Id = contact.Id, Name = contact.Name };
                    store.contactMessages.Add(contactProp.Id, new());
                    Application.Current.Dispatcher.Invoke(() => store.contacts.Add(contactProp));
                }

                store.ConfirmSign();
            }
            else
            {
                string message = res.Data;
                store.DenySign(message);
            }
        }

        /// <summary>
        /// Processing a registration request from the server.
        /// Creating a new user.
        /// </summary>
        /// <param name="res"></param>
        private void SignUpHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                store.user = GetProp(res);
                store.ConfirmSign();
            }
            else
            {
                string message = res.Data;
                store.DenySign(message);
            }
        }
    }
}