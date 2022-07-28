using CrossLibrary;
using CrossLibrary.Network;
using Newtonsoft.Json;
using Server.Services.Communication;

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
                { Command.SendFileMessageToGroup, FileMessageHandle },
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
            var tos = store.pendingClients.Dequeue();
            Guid guid = Guid.NewGuid();
            store.pendingTransactions.Add(guid, new() { Count = tos.Count, Transaction = transaction });

            foreach (var to in tos)
            {
                if (store.pending.ContainsKey(to))
                {
                    store.pending[to].AddLast(guid);
                }
                else
                {
                    LinkedList<Guid> guids = new();
                    guids.AddLast(guid);
                    store.pending.Add(to, guids);
                }
            }
        }
    }
}