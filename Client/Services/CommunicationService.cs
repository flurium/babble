﻿using CrossLibrary;
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

    private void GetContactHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop contact = new()
        {
          Id = res.Data.Id,
          Name = res.Data.Name
        };

        // add contact to ui
        contactMessages.Add(contact.Id, new());
        Application.Current.Dispatcher.Invoke(() => Contacts.Add(contact));
      }
      else
      {
        // show error
        string message = (string)res.Data;
        Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
      }
    }

    private void CreateGroupHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop group = new()
        {
          Id = res.Data.Id,
          Name = res.Data.Name
        };

        // add contact to ui
        groupMessages.Add(group.Id, new());
        Application.Current.Dispatcher.Invoke(() => Groups.Add(group));
      }
      else
      {
        string message = (string)res.Data;
        Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
        // show error
      }
    }

    private void EnterGroupHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop group = new()
        {
          Id = res.Data.Id,
          Name = res.Data.Name
        };

        // add contact to ui
        groupMessages.Add(group.Id, new());
        Application.Current.Dispatcher.Invoke(() => Groups.Add(group));
      }
      else
      {
        string message = (string)res.Data;
        Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
        // show error
      }
    }

    private void RemoveContactHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop contact = new()
        {
          Id = res.Data.Id
        };

        foreach (var cont in Contacts)
        {
          if (cont.Id == contact.Id)
          {
            Contacts.Remove(cont);
            contactMessages.Remove(cont.Id);
            break;
          }
        }

        Application.Current.Dispatcher.Invoke(() => Contacts.Add(contact));
      }
      else
      {
        string message = (string)res.Data;
        Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
      }
    }

    private void GetInviteHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        Prop invite = new()
        {
          Id = res.Data.Id,
          Name = res.Data.Name
        };

        Application.Current.Dispatcher.Invoke(() => Invites.Add(invite));
      }
      else
      {
        // show error
      }
    }

    private void GetMessageFromContactHandle(Response res)
    {
      int id = res.Data.Id;
      string messageStr = res.Data.Message;
      Message message = new() { String = messageStr, IsIncoming = true };

      foreach (var contactMessage in contactMessages)
      {
        if (contactMessage.Key == id)
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

    private void GetMessageFromGroupHandle(Response res)
    {
      int id = res.Data.Id;
      string messageStr = res.Data.Message;
      Message message = new() { String = messageStr, IsIncoming = true };

      foreach (var groupMessage in groupMessages)
      {
        if (groupMessage.Key == id)
        {
          groupMessage.Value.AddLast(message);
          if (currentProp.Id == id)
          {
            Application.Current.Dispatcher.Invoke(() => CurrentMessages.Add(message));
          }
          return;
        }
      }
    }

    private void RenameContactHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        int id = res.Data.Id;
        string newName = res.Data.Name;

        for (int i = 0; i < Contacts.Count; i++)
        {
          if (Contacts[i].Id == id)
          {
            Prop newContact = new() { Id = id, Name = newName };

            Contacts[i] = newContact;
            break;
          }
        }
      }
      else
      {
      }
    }

    private void RenameGroupHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
        int id = res.Data.Id;
        string newName = res.Data.Name;

        for (int i = 0; i < Groups.Count; i++)
        {
          if (Groups[i].Id == id)
          {
            Prop newGroup = new() { Id = id, Name = newName };

            Groups[i] = newGroup;
            break;
          }
        }
      }
      else
      {
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

    private void SendInviteHandle(Response res)
    {
      if (res.Status == Status.OK)
      {
      }
      else
      {
        string message = res.Data;
        MessageBox.Show(message);
      }
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
          groupMessages.Add(groupProp.Id, new());

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
          contactMessages.Add(contactProp.Id, new());
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
  }
}