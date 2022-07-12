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

  public partial class CommunicationService
  {
    private const string remoteIp = "127.0.0.1";
    private const int remotePort = 5001;
    private CommunicationState state;

    // key = contact id
    private readonly Dictionary<int, LinkedList<Message>> contactMessages = new();

    // key = group id
    private readonly Dictionary<int, LinkedList<Message>> groupMessages = new();

    private readonly Dictionary<Command, Action<Response>> handlers = new();
    private readonly int localPort;
    private Prop currentProp;
    private Socket? listeningSocket;
    private Task listeningTask;
    private bool run = false;

    public CommunicationService()
    {
      // open port
      Random rnd = new();
      do
      {
        localPort = rnd.Next(3000, 49000);
      } while (localPort == 5001); // 5001 = server port

      // Init handlers
      handlers.Add(Command.SignIn, SignInHandle);
      handlers.Add(Command.SignUp, SignUpHandle);
      handlers.Add(Command.SendInvite, SendInviteHandle);
      handlers.Add(Command.GetInvite, GetInviteHandle);
      handlers.Add(Command.GetContact, GetContactHandle);
      handlers.Add(Command.GetMessageFromContact, GetMessageFromContactHandle);
      handlers.Add(Command.GetMessageFromGroup, GetMessageFromGroupHandle);
      handlers.Add(Command.CreateGroup, CreateGroupHandle);
      handlers.Add(Command.EnterGroup, EnterGroupHandle);
      handlers.Add(Command.RemoveContact, RemoveContactHandle);
      handlers.Add(Command.RenameContact, RenameContactHandle);
      handlers.Add(Command.RenameGroup, RenameGroupHandle);
    }

    // function from interface to confirm sign
    public Action ConfirmSign { get; set; }

    // ObservableCollections must not be recreated
    public ObservableCollection<Prop> Contacts { get; } = new();

    public ObservableCollection<Message> CurrentMessages { get; } = new();
    public Action<string> DenySign { get; set; }
    public ObservableCollection<Prop> Groups { get; } = new();

    public ObservableCollection<Prop> Invites { get; } = new();

    public Prop User { get; private set; }

    public void SetState(CommunicationState state)
    {
      this.state = state;
      state.SetCommunicationService(this);
    }

    public void AcceptInvite(int id)
    {
      Request req = new() { Command = Command.AcceptInvite, Data = new { Id = id } };
      SendData(req);

      foreach (var invite in Invites)
      {
        if (invite.Id == id)
        {
          Invites.Remove(invite);
          return;
        }
      }
    }

    public void CreateGroup(string groupName)
    {
      Request req = new() { Command = Command.CreateGroup, Data = new { Group = groupName, User = User.Id } };
      SendData(req);
    }

    public void Disconnect()
    {
      Request req = new() { Command = Command.Disconnect, Data = new { Id = User.Id } };
      SendData(req);

      contactMessages.Clear();
      groupMessages.Clear();
      Contacts.Clear();
      CurrentMessages.Clear();
      Groups.Clear();
      Invites.Clear();

      CloseConnection();
    }

    public void Leave(int id) => state.Leave(id);

    public void EnterGroup(string groupName)
    {
      Request req = new() { Command = Command.EnterGroup, Data = new { Group = groupName, User = User.Id } };
      SendData(req);
    }

    public void Rename(string newName) => state.Rename(newName);

    public void SendInvite(string name)
    {
      Request req = new() { Command = Command.SendInvite, Data = new { To = name, From = User.Id } };
      SendData(req);
    }

    public void SendMessage(string messageStr) => state.SendMessage(messageStr);

    public void SetCurrentProp(Prop prop)
    {
      currentProp = prop;
      state.RefreshMessages();
    }

    public void SignIn(string name, string password)
    {
      try
      {
        Request req = new() { Command = Command.SignIn, Data = new { Name = name, Password = password } };

        if (!run) OpenConnection();

        SendData(req, remoteIp, remotePort);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    public void SignUp(string name, string password)
    {
      try
      {
        Request req = new() { Command = Command.SignUp, Data = new { Name = name, Password = Hasher.Hash(password) } };

        if (!run) OpenConnection();

        SendData(req, remoteIp, remotePort);
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
        run = false;
        listeningSocket.Shutdown(SocketShutdown.Both);
        listeningSocket.Close();
        listeningSocket = null;
      }
    }


    // Treatment
    private void Handle(string resStr)
    {
      Response res = JsonConvert.DeserializeObject<Response>(resStr);
      try
      {
        handlers[res.Command](res);
      }
      catch (Exception ex)
      {
        // Show error
      }
    }

    /// <summary>
    /// Loop that read incomming responses
    /// </summary>
    private void Listen()
    {
      if (listeningSocket != null)
      {
        try
        {
          while (run)
          {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[1024];

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
    }

    private void OpenConnection()
    {
      run = true;
      listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      IPEndPoint localIP = new IPEndPoint(IPAddress.Parse(remoteIp), localPort);
      listeningSocket.Bind(localIP);
      listeningTask = new(Listen);
      listeningTask.Start();
    }

    // ip and port same all time
    private void SendData(Request message, string ip, int port)
    {
      if (listeningSocket != null)
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
    }

    private void SendData(Request req)
    {
      if (listeningSocket != null)
      {
        try
        {
          // TODO: optimize
          IPEndPoint remotePoint = new(IPAddress.Parse(remoteIp), remotePort);
          string requestStr = JsonConvert.SerializeObject(req);
          byte[] data = Encoding.Unicode.GetBytes(requestStr);
          listeningSocket.SendTo(data, remotePoint);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

   
  }
}