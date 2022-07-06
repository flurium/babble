﻿using CrossLibrary;

namespace Client.Services
{
  public partial class CommunicationService
  {
    public class GroupState : State
    {
      public override void Leave(int id)
      {
        Request req = new() { Command = Command.LeaveGroup, Data = new { Group = id, User = cs.User.Id } };
        cs.SendData(req);
      }

      public override void RefreshMessages()
      {
        cs.CurrentMessages.Clear();
        foreach (Message message in cs.groupMessages[cs.currentProp.Id])
        {
          cs.CurrentMessages.Add(message);
        }
      }

      public override void Rename(string newName)
      {
        Request req = new() { Command = Command.RenameGroup, Data = new { Id = cs.currentProp.Id, NewName = newName } };
        cs.SendData(req);
      }

      public override void SendMessage(string messageStr)
      {
        Message message = new() { String = messageStr, IsIncoming = false };
        cs.groupMessages[cs.currentProp.Id].AddLast(message);
        cs.CurrentMessages.Add(message);

        Request req = new() { Command = Command.SendMessageToGroup, Data = new { To = cs.currentProp.Id, From = cs.User.Id, Message = message.String } };
        cs.SendData(req);
      }
    }
  }
}