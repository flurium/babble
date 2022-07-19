using Client.Models;
using Client.Services.Network.Base;
using CrossLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client.Services
{
    public class Store
    {
        public Prop user;
        public Prop currentProp;

        public ObservableCollection<Prop> contacts = new();
        public ObservableCollection<Prop> groups = new();
        public ObservableCollection<Prop> invites = new();

        public Dictionary<int, LinkedList<Message>> groupMessages = new();
        public Dictionary<int, LinkedList<Message>> contactMessages = new();
        public ObservableCollection<Message> currentMessages = new();

        public byte[] pendingSendFile;

        // function from interface to confirm sign
        public Action ConfirmSign { get; set; }
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