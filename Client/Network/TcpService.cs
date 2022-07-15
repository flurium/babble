using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    internal class TcpService
    {
        private readonly Action<string> handle;
        private Task listenTask;
        private readonly int port;
        private bool run = false;
        private TcpClient tcpClient;
        private TcpListener tcpListener;
        private NetworkStream tcpStream;

        public TcpService(int port, Action<string> handle)
        {
            this.port = port;
            this.handle = handle;
        }

        public long BufferSize { get; set; } = 1024;

        /// <summary>
        /// Connect to another client and send file message. Get nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ip">Ip address of client to send.</param>
        /// <param name="port">Port of client to send.</param>
        public void SendMessage(string message, string ip, int port)
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

        public void Start()
        {
            listenTask = new(Listen);
            listenTask.Start();
        }

        public void Stop() => run = false;

        private string GetMessage()
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

        private void Listen()
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
                        string message = GetMessage();
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