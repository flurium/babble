using System.Net;
using System.Net.Sockets;
using System.Text;
using static CrossLibrary.Globals;

namespace CrossLibrary.Network
{
    public class TcpService : ProtocolService
    {
        private Task listenTask;
        private TcpClient tcpClient;
        private TcpListener tcpListener;
        private NetworkStream tcpStream;

        /// <param name="ip">Local ip</param>
        /// <param name="port">Local port</param>
        /// <param name="handle">Delegate to handle incomming message strings<</param>
        public TcpService(string ip, int port, Action<string> handle) : base(ip, port, handle)
        {
        }

        public override IPEndPoint RemoteIpEndPoint { get => (IPEndPoint)tcpClient.Client.RemoteEndPoint; }

        /// <summary>
        /// Connect to another client and send file message. Get nothing.
        /// </summary>
        public override void Send(byte[] data)
        {
            Send(data, RemoteIpEndPoint);
        }

        public override void Send(byte[] data, IPEndPoint ip)
        {
            TcpClient outcomeClient = new();
            NetworkStream? outcomeStream = null;
            try
            {
                outcomeClient.Connect(ip);
                outcomeStream = outcomeClient.GetStream();
                outcomeStream.Write(data, 0, data.Length);
            }
            finally
            {
                if (outcomeStream != null) outcomeStream.Close();
                outcomeClient.Close();
            }
        }

        public override void Start()
        {
            if (!run)
            {
                listenTask = new(Listen);
                listenTask.Start();
            }
        }

        public override void Stop()
        {
            run = false;
        }

        protected override void Listen()
        {
            try
            {
                tcpListener = new(IPAddress.Any, localPort);
                tcpListener.Start();
                run = true;

                while (run)
                {
                    try
                    {
                        tcpClient = tcpListener.AcceptTcpClient();
                        tcpStream = tcpClient.GetStream();

                        // get message only one time
                        string message = Receive();
                        handle(message);
                    }
                    finally
                    {
                        if (tcpStream != null) tcpStream.Close();
                        if (tcpClient != null) tcpClient.Close();
                    }
                }
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        protected override string Receive()
        {
            int bytes;
            byte[] buffer = new byte[bufferSize];
            StringBuilder builder = new();
            do
            {
                bytes = tcpStream.Read(buffer, 0, buffer.Length);
                builder.Append(CommunicationEncoding.GetString(buffer, 0, bytes));
            } while (tcpStream.DataAvailable);

            return builder.ToString();
        }
    }
}