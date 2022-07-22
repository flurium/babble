using Client.Models;
using Client.Services.Communication;
using Client.Services.Network.Base;
using CrossLibrary;
using CrossLibrary.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace Client.Services.Network.Udp
{
    public class UdpHandler : ProtocolHandler
    {
        private Dictionary<Command, Action<Transaction>> handlers;

        public UdpHandler(string ip, int port, Store store) : base(ip, port, store)
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
                { Command.GetClientAddress, GetClientAddressHandle },
                { Command.Exception, ExceptionHandle }
            };
        }

        protected override ProtocolService CreateProtocolService(string ip, int port, Action<string> handle) => new UdpService(ip, port, handle);

        protected override void Handle(string str)
        {
            try
            {
                Transaction res = JsonConvert.DeserializeObject<Transaction>(str);
                handlers[res.Command](res);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Sorry, something went wrong"));
            }
        }

        private Prop GetProp(Transaction res) => new() { Id = res.Data.Id, Name = res.Data.Name };

        private void ExceptionHandle(Transaction tran)
        {
            string message = (string)tran.Data;
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
        }

        private void GetFileMessageSizeHandle(Transaction res)
        {
            long size = res.Data.Size;
            store.tcpHandler.UpdateBufferSize(size);
        }

        private void GetClientAddressHandle(Transaction res)
        {
            string ip = res.Data.Ip;
            int port = res.Data.Port;

            //store.tcpHandler.UpdateDestination(destination);
            store.tcpHandler.Send(store.pendingSendFile, new(IPAddress.Parse(ip), port));
        }

        private void AddToCollection(Prop prop, ref ObservableCollection<Prop> collection) => collection.Add(prop);

        /// <summary>
        /// Handler for incoming props from the server.
        /// Create new entries in "dictionary" and "collection"
        /// </summary>
        private void NewChatHandle(Transaction res, ref Dictionary<int, LinkedList<Message>> dictionary, ref ObservableCollection<Prop> collection)
        {
            Prop chat = GetProp(res);

            Delegate addDelegate = AddToCollection;
            object[] addParams = new object[] { chat, collection };

            dictionary.Add(chat.Id, new());
            Application.Current.Dispatcher.Invoke(addDelegate, addParams);
        }

        private void GetContactHandle(Transaction res) => NewChatHandle(res, ref store.contactMessages, ref store.contacts);

        private void CreateGroupHandle(Transaction res) => NewChatHandle(res, ref store.groupMessages, ref store.groups);

        private void EnterGroupHandle(Transaction res) => NewChatHandle(res, ref store.groupMessages, ref store.groups);

        /// <summary>
        /// Deleting a contact handler.
        /// From the "Dictionary contactMessages" and "ObservableCollection<Prop> Contacts".
        /// </summary>
        /// <remarks>Accepts response from the contact id in the Data.Id</remarks>
        /// <param name="res"></param>
        private void RemoveContactHandle(Transaction res)
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

        /// <summary>
        /// Accepting an invitation from the server.
        /// Placement of the invitation in  "ObservableCollection<Prop> Invites"
        /// </summary>
        /// <param name="res"></param>
        private void GetInviteHandle(Transaction res)
        {
            Prop invite = GetProp(res);
            Application.Current.Dispatcher.Invoke(() => store.invites.Add(invite));
        }

        private void SendInviteHandle(Transaction res)
        {
            string message = res.Data.Message;
            MessageBox.Show(message);
        }

        /// <summary>
        /// Inserting a message into "dictionary" and " ObservableCollection<Message> CurrentMessages"
        /// </summary>
        private void GetMessageHandle(Transaction res, ref Dictionary<int, LinkedList<Message>> dictionary)
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

        private void GetMessageFromContactHandle(Transaction res) => GetMessageHandle(res, ref store.contactMessages);

        private void GetMessageFromGroupHandle(Transaction res) => GetMessageHandle(res, ref store.groupMessages);

        private void UpdateCollectionProp(Prop prop, int i, ref ObservableCollection<Prop> collection) => collection[i] = prop;

        /// <summary>
        /// Processing a message from the server about renaming.
        /// Search for a prop in the ObservableCollection<Prop> collection and change the Name
        /// </summary>
        /// <param name="res"></param>
        private void RenameHandle(Transaction res, ref ObservableCollection<Prop> collection)
        {
            Prop newProp = GetProp(res);

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

        private void RenameContactHandle(Transaction res) => RenameHandle(res, ref store.contacts);

        private void RenameGroupHandle(Transaction res) => RenameHandle(res, ref store.groups);

        /// <summary>
        /// Login processing from the server.
        /// Obtaining data about contacts, groups and invites, entering into the appropriate arrays.
        /// </summary>
        /// <param name="res"></param>
        private void SignInHandle(Transaction res)
        {
            bool isOk = res.Data.IsOk;

            if (isOk)
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

                store.ConfirmSign(store.user.Name);
            }
            else
            {
                string message = res.Data.Message;
                store.DenySign(message);
            }
        }

        /// <summary>
        /// Processing a registration request from the server.
        /// Creating a new user.
        /// </summary>
        /// <param name="res"></param>
        private void SignUpHandle(Transaction res)
        {
            bool isOk = res.Data.IsOk;
            if (isOk)
            {
                store.user = GetProp(res);
                store.ConfirmSign(store.user.Name);
            }
            else
            {
                string message = res.Data.Message;
                store.DenySign(message);
            }
        }
    }
}