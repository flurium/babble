using Client.Models;
using Client.Network;
using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Client.Services
{
    public partial class CommunicationService
    {
        private const string remoteIp = "127.0.0.1";
        private const int remotePort = 5001;
        private CommunicationState state;

        // key = contact id
        private readonly Dictionary<int, LinkedList<Message>> contactMessages = new();

        // key = group id
        private readonly Dictionary<int, LinkedList<Message>> groupMessages = new();

        private readonly Dictionary<Command, Action<Response>> handlers;
        private readonly int localPort;
        private Prop currentProp;

        private UdpService udpService;

        private byte[] pendingSendFile;
        private bool run = false;

        public CommunicationService()
        {
            // open port
            Random rnd = new();
            do
            {
                localPort = rnd.Next(3000, 49000);
            } while (localPort == 5001); // 5001 = server port

            // init udp service
            udpService = new(remoteIp, remotePort, localPort, Handle);

            // init tcp service
            tcpService = new(localPort, TcpHandle);

            // Init handlers
            handlers = new()
      {
        {Command.SignIn, SignInHandle },
        {Command.SignUp, SignUpHandle},
        {Command.SendInvite, SendInviteHandle},
        {Command.GetInvite, GetInviteHandle},
        {Command.GetContact, GetContactHandle},
        {Command.GetMessageFromContact, GetMessageFromContactHandle},
        {Command.GetMessageFromGroup, GetMessageFromGroupHandle},
        {Command.CreateGroup, CreateGroupHandle},
        {Command.EnterGroup, EnterGroupHandle},
        {Command.RemoveContact, RemoveContactHandle},
        {Command.RenameContact, RenameContactHandle},
        {Command.RenameGroup, RenameGroupHandle}
      };
        }

        // function from interface to confirm sign
        public Action ConfirmSign { get; set; }

        // ObservableCollections must not be recreated
        public ObservableCollection<Prop> Contacts { get; } = new();

        public ObservableCollection<Message> CurrentMessages { get; } = new();
        public Action<string> DenySign { get; set; }
        public ObservableCollection<Prop> Groups { get; } = new();

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
            Contacts.Clear();
            CurrentMessages.Clear();
            Groups.Clear();
            Invites.Clear();

            // stop services
            tcpService.Stop();
            udpService.Stop();

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

        public void SetCurrentProp(Prop prop)
        {
            currentProp = prop;
            state.RefreshMessages();
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

        // Treatment
        private void Handle(string resStr)
        {
            Response res = JsonConvert.DeserializeObject<Response>(resStr);
            try
            {
                handlers[res.Command](res);
            }
            catch (Exception ex)
            {
                // Show error
            }
        }

        private void OpenConnection()
        {
            run = true;
            // run udp service
            udpService.Start();

            // run tcp service
            tcpService.Start();
        }

        private void SendData(Request req) => udpService.Send(req);

        private void SendData(byte[] data) => udpService.Send(data);
    }
}