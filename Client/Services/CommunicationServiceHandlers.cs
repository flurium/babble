using Client.Models;
using CrossLibrary;
using System.Windows;

namespace Client.Services
{
  public partial class CommunicationService
  {
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

    private void GetMessageFromContactHandle(Response res)
    {
      int id = res.Data.Id;
      string messageStr = res.Data.Message;
      Message message = new() { Text = messageStr, IsIncoming = true };

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
      Message message = new() { Text = messageStr, IsIncoming = true };

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