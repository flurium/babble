using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.Services
{
  public struct Message
  {
    public bool IsIncoming { get; set; }
    public string String { get; set; }
  }

  public class CommunicationService
  {
    private Dictionary<Prop, LinkedList<Message>> contactMessages = new();
    private Prop currentProp;
    private Dictionary<Prop, LinkedList<Message>> groupMessages = new();
    private Dictionary<Command, Action<Response>> handlers = new Dictionary<Command, Action<Response>>();
    private Socket listeningSocket;
    private Task listenongTask;
    private int localPort;
    private string remoteIp = "127.0.0.1";
    private int remotePort = 5001;
    private Random rnd = new Random();
    private bool run = false;

    public CommunicationService()
    {
      // test
      Contacts.Add(new Prop { Id = 3, Name = "3" });
      contactMessages.Add(new Prop { Id = 3, Name = "3" }, new());
      Contacts.Add(new Prop { Id = 2, Name = "2" });
      contactMessages.Add(new Prop { Id = 2, Name = "2" }, new());

      Groups.Add(new Prop { Id = 1, Name = "dfasd" });
      groupMessages.Add(new Prop { Id = 1, Name = "dfasd" }, new());

      // Init handlers
      handlers.Add(Command.SignIn, SignInHandle);
      handlers.Add(Command.SignUp, SignUpHandle);
      handlers.Add(Command.SendInvite, SendInviteHandle);
      handlers.Add(Command.GetInvite, GetInviteHandle);
      handlers.Add(Command.GetContact, GetContactHandle);
    }

    // ObservableCollections must not be recreated
    public ObservableCollection<Prop> Contacts { get; } = new();

    public ObservableCollection<Message> CurrentMessages { get; } = new();

    public ObservableCollection<Prop> Groups { get; } = new();

    public ObservableCollection<Prop> Invites { get; } = new();

    public Prop User { get; private set; }

    public void SendMessageToContact(string str)
    {
      Message message = new Message { String = str, IsIncoming = false };
      contactMessages[currentProp].AddLast(message);
      CurrentMessages.Add(message);
    }

    public void SendMessageToGroup(string str)
    {
      Message message = new Message { String = str, IsIncoming = false };
      groupMessages[currentProp].AddLast(message);
      CurrentMessages.Add(message);
    }

    public void SetCurrentContact(Prop contact)
    {
      currentProp = contact;
      // refresh CurrentMessages
      CurrentMessages.Clear();
      foreach (Message message in contactMessages[currentProp])
      {
        CurrentMessages.Add(message);
      }
    }

    public void SetCurrentGroup(Prop group)
    {
      currentProp = group;
      // refresh CurrentMessages
      CurrentMessages.Clear();
      foreach (Message message in groupMessages[currentProp])
      {
        CurrentMessages.Add(message);
      }
    }

    public void Disconnect()
    {
      // Send disconnect request
    }

    public void SignIn(string name, string password)
    {
      // TODO: remove to constructor
      do
      {
        localPort = rnd.Next(2000, 49000);
      } while (localPort == 5001); // 5001 - server port
      //

      try
      {
        Request sing = new Request();
        sing.Command = Command.SignIn;
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

    private void CloseConnection()
    {
      if (listeningSocket != null)
      {
        listeningSocket.Shutdown(SocketShutdown.Both);
        listeningSocket.Close();
        listeningSocket = null;
      }
    }

    private void GetContactHandle(Response res)
    { }

    private void GetInviteHandle(Response res)
    { }

    // Treatment
    private void Handle(string resStr)
    {
      Response res = JsonConvert.DeserializeObject<Response>(resStr);
      if (res.Status == Status.OK)
      {
        try
        {
          handlers[res.Command](res);
        }
        catch (Exception ex)
        {
          // Show error
        }
      }
      else if (res.Status == Status.Bad)
      {
        // Show exeption message
        // res.Data = message
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

          Handle(builder.ToString());

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

    // ip and port same all time
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

    private void SendData(Request req)
    {
      try
      {
        // TODO: optimize
        IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        string requestStr = JsonConvert.SerializeObject(req);
        byte[] data = Encoding.Unicode.GetBytes(requestStr);
        listeningSocket.SendTo(data, remotePoint);
      }
      catch (Exception ex)
      {
        // show error
      }
    }

    private void SendInviteHandle(Response res)
    { }

    private void SignInHandle(Response res)
    {
      User = res.Data.User;
      foreach (var group in res.Data.Groups) Groups.Add(group);
      foreach (var invite in res.Data.Invites) Groups.Add(invite);
      foreach (var contact in res.Data.Contacts) Groups.Add(contact);
    }

    private void SignUpHandle(Response res) => User = res.Data;
  }
}