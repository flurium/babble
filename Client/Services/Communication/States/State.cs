﻿using Client.Models;
using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using static CrossLibrary.Globals;

namespace Client.Services.Communication.States
{
    /// <summary>
    /// Base state abstraction. Inside of CommunicationService class,
    /// because need access to private fields.
    /// Need access to: currentProp, contactMessages, groupMessages, contacts, groups, pendingSendFile
    /// </summary>
    internal abstract class State
    {
        protected Store store;

        public void SetCommunicationService(Store store)
        {
            this.store = store;
        }

        // Base functions
        protected void Send(Transaction transaction)
        {
            store.udpHandler.Send(transaction.ToStrBytes(), store.destination);
        }

        /// <summary>
        /// Base rename function, must be called from children.
        /// data must have NewName property.
        /// </summary>
        /// <param name="data">Data in request. Must have NewName property.</param>
        /// <param name="collection">Collection that must be updated.</param>
        /// <param name="command"></param>
        protected void Rename(dynamic data, ref ObservableCollection<Prop> collection, Command command)
        {
            Transaction req = new() { Command = command, Data = data };

            Send(req);

            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Id == store.currentProp.Id)
                {
                    store.currentProp = new Prop { Id = store.currentProp.Id, Name = data.NewName };
                    collection[i] = store.currentProp;
                    break;
                }
            }
        }

        protected void RefreshMessages(ref Dictionary<int, LinkedList<Message>> dictionary)
        {
            store.currentMessages.Clear();
            foreach (Message message in dictionary[store.currentProp.Id])
            {
                store.currentMessages.Add(message);
            }
        }

        protected void SendMessage(string messageStr, ref Dictionary<int, LinkedList<Message>> dictionary, Command command)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.ToLocalTime().ToShortTimeString();

            Message message = new() { Text = messageStr, IsIncoming = false, Time = timeStr };
            dictionary[store.currentProp.Id].AddLast(message);
            store.currentMessages.Add(message);

            Transaction req = new() { Command = command, Data = new { To = store.currentProp.Id, From = store.user.Id, Message = message.Text, Time = time } };
            Send(req);
        }

        protected void SendFileMessage(string messageStr, List<string> filePaths, ref Dictionary<int, LinkedList<Message>> dictionary, Command fileCommand, Command sizeCommand, int from)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.ToLocalTime().ToShortTimeString();

            Message message = new() { IsIncoming = false, Text = messageStr, Files = new(), Time = timeStr };

            LinkedList<object> files = new();
            foreach (string filePath in filePaths)
            {
                string extention = Path.GetExtension(filePath).ToLower();
                bool isImage = MessageFile.ImageExtentions.Contains(extention);
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                byte[] data = File.ReadAllBytes(filePath);
                files.AddLast(new
                {
                    IsImage = isImage,
                    Bytes = data,
                    Name = fileName,
                    Extention = extention
                });

                message.Files.Add(new MessageFile { IsImage = isImage, Path = filePath, Name = $"{fileName}{extention}" });
            }

            // add to ui
            dictionary[store.currentProp.Id].AddLast(message);
            store.currentMessages.Add(message);

            // File request which will be sended to another client
            Transaction fileReq = new() { Command = fileCommand, Data = new { From = from, User = store.user.Name, Message = message.Text, Files = files, Time = time } };
            string fileReqStr = JsonConvert.SerializeObject(fileReq);
            byte[] fileReqData = CommunicationEncoding.GetBytes(fileReqStr);

            // send data size
            Transaction req = new() { Command = sizeCommand, Data = new { To = store.currentProp.Id, Size = fileReqData.LongLength, From = store.user.Id, Time = time } };
            Send(req);

            store.pendingFiles.Enqueue(fileReqData);
        }

        protected void Leave(Command command)
        {
            Send(new() { Command = command, Data = new { To = store.currentProp.Id, From = store.user.Id } });
        }

        // abstracts (will be overrided)

        public abstract void Rename(string newName);

        /// <summary>
        /// Leave group or remove contact
        /// </summary>
        public abstract void Leave();

        public abstract void SendMessage(string messageStr);

        public abstract void SendFileMessage(string messageStr, List<string> filePaths);

        public abstract void RefreshMessages();
    }
}