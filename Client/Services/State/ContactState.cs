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

            public override void RefreshMessages() => RefreshMessages(ref cs.contactMessages);

            public override void Rename(string newName)
            {
                var data = new { To = cs.currentProp.Id, From = cs.User.Id, NewName = newName };
                Rename(data, ref cs.contacts, Command.RenameContact);
            }

            public override void SendFileMessage(string messageStr, List<string> filePaths)
            {
                SendFileMessage(messageStr, filePaths, ref cs.contactMessages, Command.SendFileMessageToContact);
            }

            public override void SendMessage(string messageStr)
            {
                SendMessage(messageStr, ref cs.contactMessages, Command.SendMessageToContact);
            }
        }
    }
}