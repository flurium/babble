using CrossLibrary;
using System.Collections.Generic;

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

            public override void RefreshMessages() => RefreshMessages(ref cs.groupMessages);

            public override void Rename(string newName)
            {
                var data = new { Id = cs.currentProp.Id, NewName = newName };
                Rename(data, ref cs.groups, Command.RenameGroup);
            }

            public override void SendFileMessage(string messageStr, List<string> filePaths)
            {
                SendFileMessage(messageStr, filePaths, ref cs.groupMessages, Command.SendFileMessageToGroup);
            }

            public override void SendMessage(string messageStr)
            {
                SendMessage(messageStr, ref cs.groupMessages, Command.SendMessageToGroup);
            }
        }
    }
}