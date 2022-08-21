﻿using Client.Models;
using Client.Services.Communication;
using CrossLibrary;
using CrossLibrary.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace Client.Services.Network
{
    internal class UdpHandler : ProtocolHandler<Store>
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
                { Command.ContactFileSize,  GetFileMessageSizeHandle },
                { Command.GetClientAddress, GetClientAddressHandle },
                { Command.GetGroupAddress, GetGroupAddressHandle },
                { Command.Exception, ExceptionHandle },
                { Command.LeaveGroup, LeaveGroupHandle }
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
            catch
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

        private void LeaveGroupHandle(Transaction transaction)
        {
            int id = transaction.Data.Id;
            foreach (var group in store.groups)
            {
                if (group.Id == id)
                {
                    Application.Current.Dispatcher.Invoke(() => store.groups.Remove(group));
                    store.groupMessages.Remove(id);
                    break;
                }
            }
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
            store.tcpHandler.Send(store.pendingFiles.Dequeue(), new(IPAddress.Parse(ip), port));
        }

        private void GetGroupAddressHandle(Transaction transaction)
        {
            string ip;
            int port;
            byte[] file = store.pendingFiles.Dequeue();

            var destinations = transaction.Data.Destinations;
            foreach (var destination in destinations)
            {
                ip = destination.Ip;
                port = destination.Port;
                store.tcpHandler.Send(file, new(IPAddress.Parse(ip), port));
            }
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
            int id = res.Data.Id;
            foreach (var contact in store.contacts)
            {
                if (contact.Id == id)
                {
                    Application.Current.Dispatcher.Invoke(() => store.contacts.Remove(contact));
                    store.contactMessages.Remove(contact.Id);
                    break;
                }
            }
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
            DateTime time = res.Data.Time;
            string? user = res.Data.User;
            Message message = new() { Text = text, IsIncoming = true, Time = time.ToLocalTime().ToShortTimeString(), User = user };

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