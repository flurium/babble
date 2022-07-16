using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    internal class TcpService : ProtocolService
    {
        private TcpClient tcpClient;
        private TcpListener tcpListener;
        private NetworkStream tcpStream;

        /// <param name="port">Local port</param>
        /// <param name="handle">Delegate to handle incomming message strings<</param>
        public TcpService(int port, Action<string> handle) : base(port, handle) { }


        /// <summary>
        /// Connect to another client and send file message. Get nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ip">Ip address of client to send.</param>
        /// <param name="port">Port of client to send.</param>
        public void Send(string message, string ip, int port)
        {
            TcpClient outcomeClient = new();
            NetworkStream? outcomeStream = null;
            try
            {
                outcomeClient.Connect(ip, port);
                outcomeStream = outcomeClient.GetStream();

                byte[] data = Encoding.Unicode.GetBytes(message);
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
        protected override string Receive()
        {
            int bytes;
            byte[] buffer = new byte[BufferSize];
            StringBuilder builder = new();
            do
            {
                bytes = tcpStream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.Unicode.GetString(buffer, 0, bytes));
            } while (tcpStream.DataAvailable);

            return builder.ToString();
        }

        protected override void Listen()
        {
            try
            {
                tcpListener = new(IPAddress.Any, port);
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
    }
}