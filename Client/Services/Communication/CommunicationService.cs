using Client.Models;
using Client.Services.Communication.States;
using Client.Services.Network;
using CrossLibrary;
using CrossLibrary.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using static CrossLibrary.Globals;

namespace Client.Services.Communication
{
    internal class CommunicationService
    {
        private State state;

        private Store store = new();

        private int localPort;

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
            IProtocolService udpHandler = new UdpHandler("127.0.0.1", localPort, store);

            // init tcp service
            IProtocolService tcpHandler = new TcpHandler("127.0.0.1", localPort, store);

            // put protocol services to store
            store.udpHandler = udpHandler;
            store.tcpHandler = tcpHandler;
        }

        public ObservableCollection<Prop> Contacts { get => store.contacts; }
        public ObservableCollection<Message> CurrentMessages { get => store.currentMessages; }

        public ObservableCollection<Prop> Groups { get => store.groups; }

        public ObservableCollection<Prop> Invites { get => store.invites; }

        public void SetState(State state)
        {
            this.state = state;
            state.SetCommunicationService(store);
        }

        public void AcceptInvite(int id)
        {
            Transaction req = new() { Command = Command.AcceptInvite, Data = new { Id = id } };
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
            Transaction req = new() { Command = Command.CreateGroup, Data = new { Group = groupName, User = store.user.Id } };
            SendData(req);
        }

        public void Disconnect()
        {
            Transaction req = new() { Command = Command.Disconnect, Data = new { store.user.Id } };
            SendData(req);

            store.Clear();

            // stop services
            //tcpService.Stop();
            //udpService.Stop();

            run = false;
        }

        public void Leave() => state.Leave();

        public void EnterGroup(string groupName)
        {
            Transaction req = new() { Command = Command.EnterGroup, Data = new { Group = groupName, User = store.user.Id } };
            SendData(req);
        }

        public void Rename(string newName) => state.Rename(newName);

        public void SendInvite(string name)
        {
            Transaction req = new() { Command = Command.SendInvite, Data = new { To = name, From = store.user.Id } };
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
            get => store.currentProp;
            set
            {
                store.currentProp = value;
                state.RefreshMessages();
            }
        }

        public void SignIn(string name, string password, Action<string> confirm, Action<string> deny)
        {
            try
            {
                store.ConfirmSign = confirm;
                store.DenySign = deny;

                Transaction req = new() { Command = Command.SignIn, Data = new { Name = name, Password = password } };

                if (!run) OpenConnection();

                SendData(req);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SignUp(string name, string password, Action<string> confirm, Action<string> deny)
        {
            try
            {
                store.ConfirmSign = confirm;
                store.DenySign = deny;

                Transaction req = new() { Command = Command.SignUp, Data = new { Name = name, Password = Hasher.Hash(password) } };

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
            Task.Run(store.udpHandler.Start);

            // run tcp service
            store.tcpHandler.Start();
        }

        //internal void SendData(Request req) => udpService.Send(req);
        public void SendData(Transaction req)
        {
            store.udpHandler.Send(req.ToStrBytes(), store.destination);
        }
    }
}