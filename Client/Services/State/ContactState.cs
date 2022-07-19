using CrossLibrary;
using System.Collections.Generic;

namespace Client.Services
{
    public class ContactState : State
    {
        public override void Leave(int id)
        {
            Request req = new() { Command = Command.RemoveContact, Data = { To = id, From = store.user.Id } };
            store.udpHandler.Send(req.ToStrBytes());
        }

        public override void RefreshMessages() => RefreshMessages(ref store.contactMessages);

        public override void Rename(string newName)
        {
            var data = new { To = store.currentProp.Id, From = store.user.Id, NewName = newName };
            Rename(data, ref store.contacts, Command.RenameContact);
        }

        public override void SendFileMessage(string messageStr, List<string> filePaths)
        {
            SendFileMessage(messageStr, filePaths, ref store.contactMessages, Command.SendFileMessageToContact);
        }

        public override void SendMessage(string messageStr)
        {
            SendMessage(messageStr, ref store.contactMessages, Command.SendMessageToContact);
        }
    }
}