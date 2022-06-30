using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public class CommunicationService
    {
        int localPort = 5001;
        string localIp = "127.0.0.1";
        bool run = false;
        Socket listeningSocket;
        Dictionary<IPEndPoint, DateTime> clients = new Dictionary<IPEndPoint, DateTime>();

        public void Run()
        {
            // запуск
            run = true;
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Task listenongTask = new Task(Listen);
        }

        private void Listen()
        {
            IPAddress ip;
            if (IPAddress.TryParse(localIp, out ip))
            {
                IPEndPoint localIp = new IPEndPoint(ip, localPort);
                listeningSocket.Bind(localIp);
                try
                {
                    while (run)
                    {
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        byte[] data = new byte[1024];
                        EndPoint clientIp = new IPEndPoint(IPAddress.Any, 0);

                        do
                        {
                            bytes = listeningSocket.ReceiveFrom(data, ref clientIp);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));

                        } while (listeningSocket.Available > 0);
                        IPEndPoint clientFullIp = (IPEndPoint)clientIp;
                        string message = builder.ToString();

                        Console.WriteLine(string.Format("{0}:{1} - {2}", clientFullIp.Address.ToString(), clientFullIp.Port, message));

                        clients.Add(clientFullIp, DateTime.Now);

                        SendData(builder.ToString(), clientFullIp.Port);

                    }
                }
                catch (SocketException socketEx)
                {
                    if (socketEx.ErrorCode != 10004)
                        Console.WriteLine(socketEx.Message + socketEx.ErrorCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        private void SendData(string message, int port)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);

            foreach (var client in clients)
            {
                if (client.Key.Port == port) // send only to one who asked
                {
                    listeningSocket.SendTo(data, client.Key);
                    clients[client.Key] = DateTime.Now; // update time
                }
            }
        }

        private void CloseConnection()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
        }
    }
}
