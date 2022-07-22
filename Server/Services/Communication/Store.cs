using CrossLibrary;
using CrossLibrary.Network;
using Server.Services.Database;
using Server.Services.Network;
using System.Net;
using static CrossLibrary.Globals;

namespace Server.Services.Communication
{
    /// <summary>
    /// Singleton pattern
    /// </summary>
    internal sealed class Store
    {
        public Dictionary<int, IPEndPoint> clients = new();
        public DatabaseService db = new();

        /// <summary>
        /// Messages pending to be sent
        /// </summary>
        public Dictionary<int, LinkedList<Transaction>> pending = new();

        /// <summary>
        /// ЧМОСТРУКТУРА ЧМОСТРУКТУРА ЧМОСТРУКТУРА
        /// </summary>
        public Queue<int> pendingClients = new();

        public IProtocolService udpHandler;
        public IProtocolService tcpHandler;

        public Store()
        {
            udpHandler = new UdpHandler(ServerDestination.Ip, ServerDestination.Port, this);
            tcpHandler = new TcpHandler(ServerDestination.Ip, ServerDestination.Port, this);
        }
    }
}