using Client.Models;
using CrossLibrary;
using CrossLibrary.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using static CrossLibrary.Globals;

namespace Client.Services.Communication
{
    internal class Store
    {
        public IPEndPoint destination = new(IPAddress.Parse(ServerDestination.Ip), ServerDestination.Port);
        public Prop user;
        public Prop currentProp;

        public ObservableCollection<Prop> contacts = new();
        public ObservableCollection<Prop> groups = new();
        public ObservableCollection<Prop> invites = new();

        public Dictionary<int, LinkedList<Message>> groupMessages = new();
        public Dictionary<int, LinkedList<Message>> contactMessages = new();
        public ObservableCollection<Message> currentMessages = new();

        public Queue<byte[]> pendingFiles = new();

        // function from interface to confirm sign
        public Action<string> ConfirmSign { get; set; }

        public Action<string> DenySign { get; set; }

        public IProtocolService udpHandler;
        public IProtocolService tcpHandler;

        public void Clear()
        {
            contactMessages.Clear();
            groupMessages.Clear();
            currentMessages.Clear();
            contacts.Clear();
            groups.Clear();
            invites.Clear();
        }
    }
}