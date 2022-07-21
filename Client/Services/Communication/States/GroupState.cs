using CrossLibrary;
using System.Collections.Generic;

namespace Client.Services.Communication.States
{
    public class GroupState : State
    {
        public override void Leave(int id)
        {
            Transaction req = new() { Command = Command.LeaveGroup, Data = new { Group = id, User = store.user.Id } };
            Send(req);
        }

        public override void RefreshMessages() => RefreshMessages(ref store.groupMessages);

        public override void Rename(string newName)
        {
            var data = new { Id = store.currentProp.Id, NewName = newName };
            Rename(data, ref store.groups, Command.RenameGroup);
        }

        public override void SendFileMessage(string messageStr, List<string> filePaths)
        {
            SendFileMessage(messageStr, filePaths, ref store.groupMessages, Command.SendFileMessageToGroup);
        }

        public override void SendMessage(string messageStr)
        {
            SendMessage(messageStr, ref store.groupMessages, Command.SendMessageToGroup);
        }
    }
}