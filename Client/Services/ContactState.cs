using CrossLibrary;
using System.Collections.Generic;

namespace Client.Services
{
  public partial class CommunicationService
  {
    public class ContactState : CommunicationState
    {
      public override void Leave(int id)
      {
        Request req = new() { Command = Command.RemoveContact, Data = { To = id, From = cs.User.Id } };
        cs.SendData(req);
      }

      public override void RefreshMessages()
      {
        cs.CurrentMessages.Clear();
        foreach (Message message in cs.contactMessages[cs.currentProp.Id])
        {
          cs.CurrentMessages.Add(message);
        }
      }

      public override void Rename(string newName)
      {
        Request req = new() { Command = Command.RenameContact, Data = new { To = cs.currentProp.Id, From = cs.User.Id, NewName = newName } };
        cs.SendData(req);

        for (int i = 0; i < cs.Contacts.Count; i++)
        {
          if (cs.Contacts[i].Id == cs.currentProp.Id)
          {
            cs.currentProp = new Prop { Id = cs.currentProp.Id, Name = newName };
            cs.Contacts[i] = cs.currentProp;
            break;
          }
        }
      }

      public override void SendFileMessage(string messageStr, List<string> filePaths)
      {
        throw new System.NotImplementedException();
      }

      public override void SendMessage(string messageStr)
      {
        Message message = new() { String = messageStr, IsIncoming = false };
        cs.contactMessages[cs.currentProp.Id].AddLast(message);
        cs.CurrentMessages.Add(message);

        Request req = new() { Command = Command.SendMessageToContact, Data = new { To = cs.currentProp.Id, From = cs.User.Id, Message = message.String } };
        cs.SendData(req);
      }
    }
  }
}