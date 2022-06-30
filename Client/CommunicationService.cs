using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    public class CommunicationService
    {
        int remotePort;
        int localPort;
        string remoteIp;
        bool run = false;
        Socket listeningSocket;

        Random rnd = new Random();
        Task listenongTask;

        private void Authentication(string name, string password) 
        {
            bool passСheck = HashService.Verify(password, "ser");
            if (passСheck)
            {

            }
        }


















        private void SendData(string message, string ip, int port)
        {
            try
            {
                // where to send
                IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
                byte[] data = Encoding.Unicode.GetBytes(message); // message in bytes
                listeningSocket.SendTo(data, remotePoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Start()
        {
            run = true;
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listenongTask = new Task(Listen);
                listenongTask.Start();
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// ///////////
        /// </summary>
        private void Listen()
        {
            try
            {
                IPEndPoint localIP = new IPEndPoint(IPAddress.Parse(remoteIp), localPort);
                listeningSocket.Bind(localIP);

                while (run)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];

                    // adress fromm where get
                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        bytes = listeningSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (listeningSocket.Available > 0);

                    IPEndPoint remoteFullIp = (IPEndPoint)remoteIp;


                    // output
                    AddToOutput(builder.ToString());
                    //return builder.ToString();
                }
            }
            catch (SocketException socketEx)
            {
                if (socketEx.ErrorCode != 10004)
                    MessageBox.Show(socketEx.Message + socketEx.ErrorCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.GetType().ToString());
            }
            finally
            {
               CloseConnection();
            }
        }



        private string AddToOutput(string message)
        {
            return message;
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