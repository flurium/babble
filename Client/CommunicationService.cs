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
using HashLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{

    public struct Request
    {
        public string Command { get; set; }
        public dynamic Data { get; set; }
    }

    public class CommunicationService
    {
        private int remotePort=5001;
        private int localPort;
        private string remoteIp = "127.0.0.1";
        private bool run = false;
        private Socket listeningSocket;
        Random rnd = new Random();
        private Task listenongTask;



        public void SignIn(string name, string password)
        {
            do
            {
                localPort = rnd.Next(2000, 49000);
            } while (localPort == 5001); // 5001 - server port
            
            try
            { 
            Request sing = new Request();
            string data = string.Format("{'Name':{0},'Password':{1}", name, password);
            sing.Command = "signin";
            sing.Data = JObject.Parse(data);
            SendData(sing, remoteIp, remotePort);
            run = true;
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Task listenongTask = new Task(Listen);
            }
              catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


















        private void SendData(Request message, string ip, int port)
        {
            try
            {
                // where to send
                IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
                string requestStr = JsonConvert.SerializeObject(message);
                byte[] data = Encoding.Unicode.GetBytes(requestStr);
                listeningSocket.SendTo(data, remotePoint);
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