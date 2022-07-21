using CrossLibrary;
using Server.Services.Exceptions;
using Server.Services.Network.Base;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static CrossLibrary.Globals;

namespace Server.Services.Network.Udp
{
    internal class UdpService : ProtocolService
    {
        private Socket socket;
        private EndPoint remoteEndPoint;

        public EndPoint RemoteEndPoint { get => remoteEndPoint; }

        public UdpService(int port, Action<string, IPEndPoint> handle) : base(port, handle)
        { }

        public override void Send(Transaction transaction, IPEndPoint ip)
        {
            string json = transaction.ToJson();
            byte[] data = CommunicationEncoding.GetBytes(json);
            socket.SendTo(data, ip);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(json);
            Console.ForegroundColor = ConsoleColor.Cyan;
        }

        public override void Start()
        {
            if (!run)
            {
                run = true;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint localEndPoint = new(IPAddress.Parse(ServerDestination.Ip), ServerDestination.Port);
                socket.Bind(localEndPoint);
                Listen();
            }
        }

        public override void Stop()
        {
            if (run)
            {
                run = false;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        protected override void Listen()
        {
            try
            {
                while (run)
                {
                    remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    string message = Receive();
                    Console.WriteLine(message);

                    IPEndPoint fullEndPoint = (IPEndPoint)remoteEndPoint;

                    handle(message, fullEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.LogException(ex);
            }
            finally
            {
                Stop();
            }
        }

        protected override string Receive()
        {
            int bytes;
            byte[] data = new byte[bufferSize];
            StringBuilder builder = new StringBuilder();
            do
            {
                bytes = socket.ReceiveFrom(data, ref remoteEndPoint);
                builder.Append(CommunicationEncoding.GetString(data, 0, bytes));
            } while (socket.Available > 0);

            return builder.ToString();
        }
    }
}