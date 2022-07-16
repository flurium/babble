using Client.Models;
using Client.Services;
using CrossLibrary;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using static CrossLibrary.Globals;

namespace Client.Services
{
    public partial class CommunicationService
    {
        /// <summary>
        /// Base state abstraction. Inside of CommunicationService class,
        /// because need access to private fields.
        /// Need access to: currentProp, contactMessages, groupMessages, contacts, groups, pendingSendFile
        /// </summary>
        public abstract class CommunicationState
        {
            protected CommunicationService cs;

            public void SetCommunicationService(CommunicationService cs)
            {
                this.cs = cs;
            }

            // Base functions

            /// <summary>
            /// Base rename function, must be called from children.
            /// data must have NewName property.
            /// </summary>
            /// <param name="data">Data in request. Must have NewName property.</param>
            /// <param name="collection">Collection that must be updated.</param>
            /// <param name="command"></param>
            protected void Rename(dynamic data, ref ObservableCollection<Prop> collection, Command command)
            {
                Request req = new() { Command = command, Data = data };

                cs.SendData(req);

                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].Id == cs.currentProp.Id)
                    {
                        cs.currentProp = new Prop { Id = cs.currentProp.Id, Name = data.NewName };
                        collection[i] = cs.currentProp;
                        break;
                    }
                }
            }

            protected void RefreshMessages(ref Dictionary<int, LinkedList<Message>> dictionary)
            {
                cs.CurrentMessages.Clear();
                foreach (Message message in dictionary[cs.currentProp.Id])
                {
                    cs.CurrentMessages.Add(message);
                }
            }

            protected void SendMessage(string messageStr, ref Dictionary<int, LinkedList<Message>> dictionary, Command command)
            {
                Message message = new() { Text = messageStr, IsIncoming = false };
                dictionary[cs.currentProp.Id].AddLast(message);
                cs.CurrentMessages.Add(message);

                Request req = new() { Command = command, Data = new { To = cs.currentProp.Id, From = cs.User.Id, Message = message.Text } };
                cs.SendData(req);
            }

            protected void SendFileMessage(string messageStr, List<string> filePaths, ref Dictionary<int, LinkedList<Message>> dictionary, Command command)
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
                dictionary[cs.currentProp.Id].AddLast(message);
                cs.CurrentMessages.Add(message);

                // File request which will be sended to another client
                Request fileReq = new() { Command = command, Data = new { From = cs.User.Id, Message = message.Text, Files = files } };
                string fileReqStr = JsonConvert.SerializeObject(fileReq);
                byte[] fileReqData = CommunicationEncoding.GetBytes(fileReqStr);

                // send data size
                Request req = new() { Command = Command.GetFileMessageSize, Data = new { To = cs.currentProp.Id, Size = fileReqData.LongLength } };
                cs.SendData(req);

                cs.pendingSendFile = fileReqData;
            }

            // abstracts (will be overrided)

            public abstract void Rename(string newName);

            /// <summary>
            /// Leave group or remove contact
            /// </summary>
            public abstract void Leave(int id);

            public abstract void SendMessage(string messageStr);

            public abstract void SendFileMessage(string messageStr, List<string> filePaths);

            public abstract void RefreshMessages();
        }
    }
}