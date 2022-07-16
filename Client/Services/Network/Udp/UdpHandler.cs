using Client.Models;
using Client.Services;
using Client.Services.Network.Base;
using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Client.Services.Network.Udp
{

    public class UdpHandler : ProtocolHandler
    {
        private Dictionary<Command, Action<Response>> handlers;
        private CommunicationService cs;

        public UdpHandler(int port, CommunicationService cs) : base(port)
        {
            this.cs = cs;

            // Init handlers
            handlers = new()
            {
                {Command.GetMessageFromContact, GetMessageFromContactHandle},
                {Command.GetMessageFromGroup, GetMessageFromGroupHandle},
                {Command.SignIn, SignInHandle },
                {Command.SignUp, SignUpHandle},
                {Command.SendInvite, SendInviteHandle},
                {Command.GetInvite, GetInviteHandle},
                {Command.GetContact, GetContactHandle},
                {Command.CreateGroup, CreateGroupHandle},
                {Command.EnterGroup, EnterGroupHandle},
                {Command.RemoveContact, RemoveContactHandle},
                {Command.RenameContact, RenameContactHandle},
                {Command.RenameGroup, RenameGroupHandle}
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

        private void AddToCollection(Prop prop, ref ObservableCollection<Prop> collection) => collection.Add(prop);

        private Prop GetProp(Response res) => new() { Id = res.Data.Id, Name = res.Data.Name };

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

        /// <summary>
        /// Handler for incoming contacts from the server.
        /// Create new entries in "Dictionary contactMessages" and "ObservableCollection<Prop> Contacts"
        /// </summary>
        /// <param name="res"></param>
        private void GetContactHandle(Response res) => NewChatHandle(res, ref cs.contactMessages, ref contacts);

        /// <summary>
        /// Group сreation handler. Adding a group to
        /// "Dictionary groupMessages" and "ObservableCollection<Prop> Groups"
        /// </summary>
        /// <param name="res"></param>
        private void CreateGroupHandle(Response res) => NewChatHandle(res, ref groupMessages, ref groups);

        /// <summary>
        /// Group exit handler.
        /// Removing a group from "Dictionary groupMessages" and "ObservableCollection<Prop> Groups"
        /// </summary>
        /// <param name="res"></param>
        private void EnterGroupHandle(Response res) => NewChatHandle(res, ref groupMessages, ref groups);
    }
}