using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static CrossLibrary.Globals;

namespace CrossLibrary.Network
{
    internal class UdpService : ProtocolService
    {
        private Socket socket;
        private EndPoint remoteEndPoint;

        public UdpService(string ip, int port, Action<string> handle) : base(ip, port, handle)
        { }

        public override void Send(byte[] data)
        {
            socket.SendTo(data, remoteEndPoint);
        }

        public override void Send(byte[] data, IPEndPoint ipEndPoint)
        {
            socket.SendTo(data, ipEndPoint);
        }

        public override void Start()
        {
            if (!run)
            {
                run = true;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint localEndPoint = new(IPAddress.Parse(ip), port);
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
                    handle(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            StringBuilder builder = new();
            do
            {
                bytes = socket.ReceiveFrom(data, ref remoteEndPoint);
                builder.Append(CommunicationEncoding.GetString(data, 0, bytes));
            } while (socket.Available > 0);

            return builder.ToString();
        }
    }
}