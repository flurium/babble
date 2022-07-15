using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static CrossLibrary.Globals;

namespace Client.Network
{
  public class UdpService
  {
    private int localPort;
    private int remotePort;
    private string remoteAddress;
    private Action<string> handle;
    private Task listenTask;
    private Socket socket;
    private bool run = false;
    private EndPoint remoteIp;

    public UdpService(string remoteAddress, int remotePort, int localPort, Action<string> handle)
    {
      this.remoteAddress = remoteAddress;
      this.remotePort = remotePort;
      this.localPort = localPort;
      this.handle = handle;
    }

    public long BufferSize { get; set; } = 1024;

    public void Start()
    {
      if (!run)
      {
        run = true;
        socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint localIp = new(IPAddress.Parse("127.0.0.1"), localPort);
        socket.Bind(localIp);

        listenTask = new(Listen);
        listenTask.Start();
      }
    }

    public void Stop()
    {
      run = false;
      socket.Shutdown(SocketShutdown.Both);
      socket.Close();
    }

    /// <summary>
    /// Loop that read incomming responses
    /// </summary>
    private void Listen()
    {
      try
      {
        while (run)
        {
          remoteIp = new IPEndPoint(IPAddress.Any, localPort);
          string message = GetMessage();

          //var fullIp = (IPEndPoint)ip;

          handle(message);
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
        Stop();
      }
    }

    public void Send(Request req)
    {
      try
      {
        string reqStr = JsonConvert.SerializeObject(req);
        byte[] data = CommunicationEncoding.GetBytes(reqStr);
        Send(data);
      }
      catch (Exception ex)
      {
      }
    }

    public void Send(byte[] data)
    {
      try
      {
        IPEndPoint remoteIP = new(IPAddress.Parse(remoteAddress), remotePort);
        socket.SendTo(data, remoteIP);
      }
      catch (Exception ex)
      {
      }
    }

    private string GetMessage()
    {
      int bytes;
      byte[] buffer = new byte[BufferSize];
      StringBuilder builder = new();
      do
      {
        bytes = socket.ReceiveFrom(buffer, ref remoteIp);

        builder.Append(CommunicationEncoding.GetString(buffer, 0, bytes));
      } while (socket.Available > 0);

      return builder.ToString();
    }
  }
}