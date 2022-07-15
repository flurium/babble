using Client.Models;
using CrossLibrary;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using static CrossLibrary.Globals;

namespace Client.Services
{
  public partial class CommunicationService
  {
    public class GroupState : CommunicationState
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

        for (int i = 0; i < cs.Groups.Count; i++)
        {
          if (cs.Groups[i].Id == cs.currentProp.Id)
          {
            cs.Groups[i] = cs.currentProp;
            break;
          }
        }
      }

      public override void SendFileMessage(string messageStr, List<string> filePaths)
      {
        Message message = new() { IsIncoming = false, Text = messageStr, Files = new() };

        LinkedList<object> files = new();
        foreach (string filePath in filePaths)
        {
          bool isImage = MessageFile.ImageExtentions.Contains(Path.GetExtension(filePath).ToLower());

          byte[] data = File.ReadAllBytes(filePath);
          files.AddLast(new { IsImage = isImage, Bytes = data });

          message.Files.Add(new MessageFile { IsImage = isImage, Path = filePath });
        }

        // add to ui
        cs.groupMessages[cs.currentProp.Id].AddLast(message);
        cs.CurrentMessages.Add(message);

        // File request which will be sended to another client
        Request fileReq = new() { Command = Command.SendFileMessageToGroup, Data = new { From = cs.User.Id, Message = message.Text, Files = files } };
        string fileReqStr = JsonConvert.SerializeObject(fileReq);
        byte[] fileReqData = CommunicationEncoding.GetBytes(fileReqStr);

        // send data size
        Request req = new() { Command = Command.GetFileMessageSize, Data = new { To = cs.currentProp.Id, Size = fileReqData.LongLength } };
        cs.SendData(req);

        cs.pendingSendFile = fileReqData;
      }

      public override void SendMessage(string messageStr)
      {
        Message message = new() { Text = messageStr, IsIncoming = false };
        cs.groupMessages[cs.currentProp.Id].AddLast(message);
        cs.CurrentMessages.Add(message);

        Request req = new() { Command = Command.SendMessageToGroup, Data = new { To = cs.currentProp.Id, From = cs.User.Id, Message = message.Text } };
        cs.SendData(req);
      }
    }
  }
}