using CrossLibrary;
using CrossLibrary.Network;
using Newtonsoft.Json;
using Server.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services.Network
{
    internal class TcpHandler : ProtocolHandler<Store>
    {
        private Dictionary<Command, Action<Transaction>> handlers;

        public TcpHandler(string ip, int port, Store store) : base(ip, port, store)
        {
            this.store = store;
            handlers = new()
            {
                { Command.SendFileMessageToContact, FileMessageHandle },
                { Command.SendFileMessageToGroup, SendFileMessageToGroupHandle },
            };
        }

        protected override ProtocolService CreateProtocolService(string ip, int port, Action<string> handle) => new TcpService(ip, port, handle);

        protected override void Handle(string str)
        {
            try
            {
                Transaction req = JsonConvert.DeserializeObject<Transaction>(str);
                handlers[req.Command](req);
            }
            catch
            {
                //Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Sorry can't get file message"));
            }
        }

        private void FileMessageHandle(Transaction transaction)
        {
            int to = store.pendingClients.Dequeue();
            if (store.pending.ContainsKey(to))
            {
                store.pending[to].AddLast(transaction);
            }
            else
            {
                LinkedList<Transaction> messages = new();
                messages.AddLast(transaction);
                store.pending.Add(to, messages);
            }
        }

        private void SendFileMessageToGroupHandle(Transaction req)
        {
            throw new NotImplementedException();
        }
    }
}