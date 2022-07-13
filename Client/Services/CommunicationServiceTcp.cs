using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Services
{
  public partial class CommunicationService
  {
    private bool runTcp = false;
    private int tcpBufferSize = 1024;
    private NetworkStream tcpStream;
    private TcpClient tcpClient;
    private TcpListener tcpListener;

    private void ListenTcp(string ip, int port)
    {
      try
      {
        tcpListener = new(IPAddress.Any, localPort);
        tcpListener.Start();
        runTcp = true;

        while (runTcp)
        {
          try
          {
            tcpClient = tcpListener.AcceptTcpClient();
            tcpStream = tcpClient.GetStream();

            // get message only one time
            string message = TcpGetMessage();
            TcpMessageHandle(message);
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
        runTcp = false;
        tcpListener.Stop();
      }
    }

    /// <summary>
    /// Connect to another client and send file message. Get nothing.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="ip">Ip address of client to send.</param>
    /// <param name="port">Port of client to send.</param>
    private void TcpSendMessage(string message, string ip, int port)
    {
      TcpClient outcomeTcpClient = new();
      NetworkStream? outcomeStream = null;
      try
      {
        outcomeTcpClient.Connect(ip, port);
        outcomeStream = outcomeTcpClient.GetStream();

        byte[] data = Encoding.Unicode.GetBytes(message);
        outcomeStream.Write(data, 0, data.Length);
      }
      finally
      {
        if (outcomeStream != null) outcomeStream.Close();
        outcomeTcpClient.Close();
      }
    }

    private string TcpGetMessage()
    {
      int bytes;
      byte[] buffer = new byte[tcpBufferSize];
      StringBuilder builder = new();
      do
      {
        bytes = tcpStream.Read(buffer, 0, buffer.Length);
        builder.Append(Encoding.Unicode.GetString(buffer, 0, bytes));
      } while (tcpStream.DataAvailable);

      return builder.ToString();
    }

    private void TcpMessageHandle(string str)
    {
    }
  }
}