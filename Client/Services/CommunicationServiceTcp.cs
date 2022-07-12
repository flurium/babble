using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services
{
  public partial class CommunicationService
  {
    private TcpClient tcpClient = null;
    private bool runTcp = false;
    private int tcpBufferSize = 1024;

    private void ListenTcp(string ip, int port)
    {
      try
      {
        tcpClient = new TcpClient(ip, port);
        NetworkStream stream = tcpClient.GetStream();
        runTcp = true;

        // get just one time
        int bytes = 0;
        byte[] buffer = new byte[tcpBufferSize];
        StringBuilder builder = new();
        do
        {
          bytes = stream.Read(buffer, 0, buffer.Length);
          builder.Append(Encoding.Unicode.GetString(buffer, 0, bytes));
        } while (stream.DataAvailable);

        // process messsage
        TcpMessageHandle(builder.ToString());
      }
      finally
      {
        tcpClient.Close();
      }
    }

    private void TcpMessageHandle(string str)
    {
    }
  }
}