using CrossLibrary;
using CrossLibrary.Network;
using Server.Services.Database;
using Server.Services.Network;
using System.Collections;
using System.Net;
using static CrossLibrary.Globals;

namespace Server.Services.Communication
{
    internal struct PendingTransaction
    {
        public int Count;
        public Transaction Transaction;
    }

    /// <summary>
    /// Singleton pattern
    /// </summary>
    internal sealed class Store
    {
        public Dictionary<int, IPEndPoint> clients = new();
        public DatabaseService db = new();

        /// <summary>
        /// Messages pending to be sent. Key is id of user.
        /// LinkedList<GUID> is keys of pendingTransaction.
        /// </summary>
        public Dictionary<int, LinkedList<Guid>> pending = new();

        //public List<Transaction> pendingTransactions = new();

        /// <summary>
        /// ЧМОСТРУКТУРА ЧМОСТРУКТУРА ЧМОСТРУКТУРА
        /// </summary>
        public Queue<List<int>> pendingClients = new();

        public Dictionary<Guid, PendingTransaction> pendingTransactions = new();

        public IProtocolService udpHandler;
        public IProtocolService tcpHandler;

        public Store()
        {
            udpHandler = new UdpHandler(ServerDestination.Ip, ServerDestination.Port, this);
            tcpHandler = new TcpHandler(ServerDestination.Ip, ServerDestination.Port, this);
        }
    }
}