using Client.Models;
using Client.Services.Communication;
using Client.Services.Network.Base;
using Client.Services.Network.Tcp;
using CrossLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Client.Services
{
    public class TcpHandler : ProtocolHandler
    {
        private Dictionary<Command, Action<Transaction>> handlers;
        private readonly string downloadFolder = string.Format("{0}\\Downloads\\babble", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        public TcpHandler(int port, Store store) : base(port, store)
        {
            handlers = new()
            {
                { Command.SendFileMessageToContact, SendFileMessageToContactHandle },
                { Command.SendFileMessageToGroup, SendFileMessageToGroupHandle },
            };
            if (!Directory.Exists(downloadFolder)) Directory.CreateDirectory(downloadFolder);
        }

        protected override ProtocolService CreateProtocolService(int port, Action<string> handle) => new TcpService(port, handle);

        protected override void Handle(string str)
        {
            try
            {
                Transaction req = JsonConvert.DeserializeObject<Transaction>(str);
                handlers[req.Command](req);
            }
            catch
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Sorry can't get file message"));
            }
        }

        private void SendFileMessageHandle(Transaction req, ref Dictionary<int, LinkedList<Message>> dictionary)
        {
            Message message = new()
            {
                IsIncoming = true,
                Text = req.Data.Message,
                Files = new()
            };
            int from = req.Data.From;

            var files = req.Data.Files;
            foreach (var file in files)
            {
                string name = file.Name;
                string extention = file.Extention;
                int count = 1;

                string downloadPath = string.Format("{0}\\{1}{2}", downloadFolder, name, extention);
                while (File.Exists(downloadPath))
                {
                    downloadPath = string.Format("{0}\\{1}({2}){3}", downloadFolder, name, count, extention);
                    count++;
                }

                byte[] data = file.Bytes;
                File.WriteAllBytes(downloadPath, data);

                message.Files.Add(new MessageFile { IsImage = file.IsImage, Path = downloadPath });
            }

            foreach (var propMessage in dictionary)
            {
                if (propMessage.Key == from)
                {
                    propMessage.Value.AddLast(message);
                    if (store.currentProp.Id == from)
                    {
                        Application.Current.Dispatcher.Invoke(() => store.currentMessages.Add(message));
                    }
                    return;
                }
            }
        }

        private void SendFileMessageToContactHandle(Transaction req)
        {
            SendFileMessageHandle(req, ref store.contactMessages);
        }

        private void SendFileMessageToGroupHandle(Transaction req)
        {
            SendFileMessageHandle(req, ref store.groupMessages);
        }
    }
}