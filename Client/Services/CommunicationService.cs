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
    private Dictionary<Command, Action<Response>> handlers = new();
    private Socket? listeningSocket;
    private Task listeningTask;
    private int localPort;
    private string remoteIp = "127.0.0.1";
    private int remotePort = 5001;
    private Random rnd = new Random();
    private bool run = false;

    // unnessery
    private Dictionary<Command, Action<Request>> senders = new();

    // function from interface to confirm sign
    public Action ConfirmSign { get; set; }

    public Action<string> DenySign { get; set; }

    public CommunicationService()
    {
      // open port
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
    }

    // ObservableCollections must not be recreated
    public ObservableCollection<Prop> Contacts { get; } = new();

    public ObservableCollection<Message> CurrentMessages { get; } = new();

    public ObservableCollection<Prop> Groups { get; } = new();

    public ObservableCollection<Prop> Invites { get; } = new();

    public Prop User { get; private set; }

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

    private void OpenConnection()
    {
      run = true;
      listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      IPEndPoint localIP = new IPEndPoint(IPAddress.Parse(remoteIp), localPort);
      listeningSocket.Bind(localIP);
      listeningTask = new(Listen);
      listeningTask.Start();
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

    private void GetContactHandle(Response res)
    { }

    private void GetInviteHandle(Response res)
    { }

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

    private void SendInviteHandle(Response res)
    {
      /*
       Response sing = new Response();
       sing.Command = Command.SignIn;
       sing.Data = new
       {
           Name = name,
           Password = password
       };
      */
    }

    private void SignInHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop user = new();
        user.Id = res.Data.User.Id;
        user.Name = res.Data.User.Name;
        User = user;

        var groups = res.Data.Groups;
        foreach (var group in groups)
        {
          Prop groupProp = new() { Id = group.Id, Name = group.Name };
          groupMessages.Add(groupProp, new());

          Application.Current.Dispatcher.Invoke(() => Groups.Add(groupProp));
        }

        var invites = res.Data.Invites;
        foreach (var invite in invites)
        {
          Prop inviteProp = new() { Id = invite.Id, Name = invite.Name };
          Application.Current.Dispatcher.Invoke(() => Invites.Add(inviteProp));
        }

        var contacts = res.Data.Contacts;
        foreach (var contact in contacts)
        {
          Prop contactProp = new() { Id = contact.Id, Name = contact.Name };
          contactMessages.Add(contactProp, new());
          Application.Current.Dispatcher.Invoke(() => Contacts.Add(contactProp));
        }

        ConfirmSign();
      }
      else
      {
        string message = res.Data;
        DenySign(message);
      }
    }

    private void SignUpHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop user = new();
        user.Id = res.Data.Id;
        user.Name = res.Data.Name;
        User = user;
        ConfirmSign();
      }
      else
      {
        string message = res.Data;
        DenySign(message);
      }
    }

    public void SendMessageToContact(string messageStr)
    {
      Message message = new() { String = messageStr, IsIncoming = false };
      contactMessages[currentProp].AddLast(message);
      CurrentMessages.Add(message);

      Request req = new() { Command = Command.SendMessageToContact, Data = new { To = currentProp.Id, From = User.Id, Message = message.String } };
      SendData(req);
    }

    public void SendMessageToGroup(string messageStr)
    {
      Message message = new() { String = messageStr, IsIncoming = false };
      groupMessages[currentProp].AddLast(message);
      CurrentMessages.Add(message);

      Request req = new() { Command = Command.SendMessageToGroup, Data = new { To = currentProp.Id, From = User.Id, Message = message.String } };
      SendData(req);
    }

    public void RenameContact(string newName)
    {
      Request req = new() { Command = Command.RenameContact, Data = new { To = currentProp.Id, From = User.Id, NewName = newName } };
      SendData(req);
    }

    public void RenameGroup(string newName)
    {
      Request req = new() { Command = Command.RenameGroup, Data = new { Id = currentProp.Id, NewName = newName } };
      SendData(req);
    }

    public void LeaveGroup(int groupId)
    {
      Request req = new() { Command = Command.LeaveGroup, Data = new { Group = groupId, User = User.Id } };
      SendData(req);
    }

    public void RemoveContact(int to)
    {
      Request req = new() { Command = Command.RemoveContact, Data = { To = to, From = User.Id } };
      SendData(req);
    }

    public void AddGroup(string name)
    {
      Request req = new() { Command = Command.AddGroup, Data = new { Name = name, User = User.Id } };
      SendData(req);
    }

    public void SendInvite(string name)
    {
      Request req = new() { Command = Command.SendInvite, Data = new { To = name, From = User.Id } };
      SendData(req);
    }

    public void AcceptInvite(int id)
    {
      Request req = new() { Command = Command.AcceptInvite, Data = new { Id = id } };
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

    private void GetMessageFromContactHandle(Response res)
    {
      int id = res.Data.Id;
      string messageStr = res.Data.Message;
      Message message = new() { String = messageStr, IsIncoming = true };

      foreach (var contactMessage in contactMessages)
      {
        if (contactMessage.Key.Id == id)
        {
          contactMessage.Value.AddLast(message);
          if (currentProp.Id == id)
          {
            Application.Current.Dispatcher.Invoke(() => CurrentMessages.Add(message));
          }
          return;
        }
      }
    }
  }
}