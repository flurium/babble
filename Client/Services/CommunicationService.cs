using Client.Models;
using Client.Services.Network.Udp;
using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using static CrossLibrary.Globals;

namespace Client.Services
{
    public partial class CommunicationService
    {
        private CommunicationState state;

        // key = contact id
        private Dictionary<int, LinkedList<Message>> contactMessages = new();

        // key = group id
        private Dictionary<int, LinkedList<Message>> groupMessages = new();

        private int localPort;
        private Prop currentProp;

        private UdpHandler udpHandler;
        //private UdpService udpService;

        private byte[] pendingSendFile;
        private bool run = false;

        public CommunicationService()
        {
            // open port
            Random rnd = new();
            do
            {
                localPort = rnd.Next(3000, 49000);
            } while (localPort == ServerDestination.Port);

            // init udp service
            udpHandler = new(localPort, this);
            //udpService = new(localPort, Handle);

            // init tcp service
            tcpService = new(localPort, TcpHandle);
        }

        // function from interface to confirm sign
        public Action ConfirmSign { get; set; }

        // ObservableCollections must not be recreated

        private ObservableCollection<Prop> contacts = new();
        public ObservableCollection<Prop> Contacts { get => contacts; }

        public ObservableCollection<Message> CurrentMessages { get; } = new();
        public Action<string> DenySign { get; set; }

        private ObservableCollection<Prop> groups = new();
        public ObservableCollection<Prop> Groups { get => groups; }

        public ObservableCollection<Prop> Invites { get; } = new();

        public Prop User { get; private set; }

        public void SetState(CommunicationState state)
        {
            this.state = state;
            state.SetCommunicationService(this);
        }

        public void AcceptInvite(int id)
        {
            Request req = new() { Command = Command.AcceptInvite, Data = new { Id = id } };
            SendData(req);

            foreach (var invite in Invites)
            {
                if (invite.Id == id)
                {
                    Invites.Remove(invite);
                    return;
                }
            }
        }

        public void CreateGroup(string groupName)
        {
            Request req = new() { Command = Command.CreateGroup, Data = new { Group = groupName, User = User.Id } };
            SendData(req);
        }

        public void Disconnect()
        {
            Request req = new() { Command = Command.Disconnect, Data = new { Id = User.Id } };
            SendData(req);

            contactMessages.Clear();
            groupMessages.Clear();
            contacts.Clear();
            CurrentMessages.Clear();
            groups.Clear();
            Invites.Clear();

            // stop services
            //tcpService.Stop();
            //udpService.Stop();

            run = false;
        }

        public void Leave(int id) => state.Leave(id);

        public void EnterGroup(string groupName)
        {
            Request req = new() { Command = Command.EnterGroup, Data = new { Group = groupName, User = User.Id } };
            SendData(req);
        }

        public void Rename(string newName) => state.Rename(newName);

        public void SendInvite(string name)
        {
            Request req = new() { Command = Command.SendInvite, Data = new { To = name, From = User.Id } };
            SendData(req);
        }

        public void SendMessage(string messageStr, List<string>? filePaths)
        {
            if (filePaths == null)
            {
                state.SendMessage(messageStr);
            }
            else
            {
                state.SendFileMessage(messageStr, filePaths);
            }
        }

        public Prop CurrentProp
        {
            get => currentProp;
            set
            {
                currentProp = value;
                state.RefreshMessages();
            }
        }

        public void SignIn(string name, string password)
        {
            try
            {
                Request req = new() { Command = Command.SignIn, Data = new { Name = name, Password = password } };

                if (!run) OpenConnection();

                SendData(req);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SignUp(string name, string password)
        {
            try
            {
                Request req = new() { Command = Command.SignUp, Data = new { Name = name, Password = Hasher.Hash(password) } };

                if (!run) OpenConnection();

                SendData(req);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenConnection()
        {
            run = true;
            // run udp service
            udpHandler.Start();
            //udpService.Start();

            // run tcp service
            tcpService.Start();
        }

        //internal void SendData(Request req) => udpService.Send(req);
        public void SendData(Request req) => udpHandler.Send(req.ToStrBytes());

        private void SendData(byte[] data) => udpHandler.Send(data);
    }
}