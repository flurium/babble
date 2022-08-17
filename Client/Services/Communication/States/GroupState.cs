﻿using CrossLibrary;
using System.Collections.Generic;

namespace Client.Services.Communication.States
{
    internal class GroupState : State
    {
        public override void Leave() => Leave(Command.LeaveGroup);

        public override void RefreshMessages() => RefreshMessages(ref store.groupMessages);

        public override void Rename(string newName)
        {
            var data = new { Id = store.currentProp.Id, NewName = newName };
            Rename(data, ref store.groups, Command.RenameGroup);
        }

        public override void SendFileMessage(string messageStr, List<string> filePaths)
        {
            SendFileMessage(messageStr, filePaths, ref store.groupMessages, Command.SendFileMessageToGroup, Command.GroupFileSize, store.currentProp.Id);
        }

        public override void SendMessage(string messageStr)
        {
            SendMessage(messageStr, ref store.groupMessages, Command.SendMessageToGroup);
        }
    }
}