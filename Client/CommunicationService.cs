using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Client
{

    public class Request
    {
        public string Command { get; set; }
        public dynamic Data { get; set; }
    }

    public class Response
    {
        public string Status { get; set; }
        public string Command { get; set; }
        public dynamic Data { get; set; }
    }

    public struct Parameter
    {
        public string Name { get; set; }
        public int Id { get; set; }
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
                sing.Command = "signin";
                sing.Data = new
                {
                    Name = name,
                    Password = password
                };


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
                   // AddToOutput(builder.ToString());
                   
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


        private void Treatment(string message)
        {
            Response lable =JsonConvert.DeserializeObject<Response>(message);
            if (lable.Status== "ok")
            {
                if (lable.Command== "signin")
                {
                    List <Parameter> group=new List<Parameter>();
                   foreach (var item in lable.Data.Groups)
                    {
                        group.Add(new Parameter { Name = item.Name, Id = item.Id });
                    }
                    List<Parameter> contact = new List<Parameter>();
                    foreach (var item in lable.Data.Contacts)
                    {
                        contact.Add(new Parameter { Name = item.Name, Id = item.Id });
                    }
                }
                else if(lable.Command== "")
                {

                }
                else
                {

                }
            }
            
        }


        /*
        private void AddToOutput(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                //messageTextBlock.Text += Environment.NewLine;
                outputTextBox.Text += string.Format("{0}\n", message);
            }, null);
        }
      */


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