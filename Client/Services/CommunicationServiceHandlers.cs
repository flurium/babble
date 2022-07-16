using Client.Models;
using CrossLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace Client.Services
{
    /// <summary>
    /// Processing responses from the server
    /// </summary>
    public partial class CommunicationService
    {
        private void AddToCollection(Prop prop, ref ObservableCollection<Prop> collection) => collection.Add(prop);

        private Prop GetProp(Response res)
        {
            return new()
            {
                Id = res.Data.Id,
                Name = res.Data.Name
            };
        }

        private void NewChatHandle(Response res, ref Dictionary<int, LinkedList<Message>> dictionary, ref ObservableCollection<Prop> collection)
        {
            if (res.Status == Status.OK)
            {
                Prop chat = GetProp(res);

                Delegate addDelegate = AddToCollection;
                object[] addParams = new object[] { chat, collection };

                // add contact to ui
                dictionary.Add(chat.Id, new());
                Application.Current.Dispatcher.Invoke(addDelegate, addParams);
            }
            else
            {
                // show error
                string message = (string)res.Data;
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
            }
        }



        /// <summary>
        /// Handler for incoming contacts from the server.
        /// Create new entries in "Dictionary contactMessages" and "ObservableCollection<Prop> Contacts"
        /// </summary>
        /// <param name="res"></param>
        private void GetContactHandle(Response res) => NewChatHandle(res, ref contactMessages, ref contacts);


        /// <summary>
        /// Group сreation handler. Adding a group to
        /// "Dictionary groupMessages" and "ObservableCollection<Prop> Groups"
        /// </summary>
        /// <param name="res"></param>
        private void CreateGroupHandle(Response res) => NewChatHandle(res, ref groupMessages, ref groups);

        /// <summary>
        /// Group exit handler.
        /// Removing a group from "Dictionary groupMessages" and "ObservableCollection<Prop> Groups"
        /// </summary>
        /// <param name="res"></param>
        private void EnterGroupHandle(Response res) => NewChatHandle(res, ref groupMessages, ref groups);


        /// <summary>
        /// Deleting a contact handler.
        /// From the "Dictionary contactMessages" and "ObservableCollection<Prop> Contacts".
        /// </summary>
        /// <remarks>Accepts response from the contact id in the Data.Id</remarks>
        /// <param name="res"></param>
        private void RemoveContactHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Prop contact = new()
                {
                    Id = res.Data.Id
                };

                foreach (var cont in contacts)
                {
                    if (cont.Id == contact.Id)
                    {
                        contacts.Remove(cont);
                        contactMessages.Remove(cont.Id);
                        break;
                    }
                }

                Application.Current.Dispatcher.Invoke(() => contacts.Add(contact));
            }
            else
            {
                string message = (string)res.Data;
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message));
            }
        }

        /// <summary>
        /// Accepting an invitation from the server.
        /// Placement of the invitation in  "ObservableCollection<Prop> Invites"
        /// </summary>
        /// <param name="res"></param>
        private void GetInviteHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Prop invite = GetProp(res);
                Application.Current.Dispatcher.Invoke(() => Invites.Add(invite));
            }
            else
            {
                // show error
            }
        }

        private void SendInviteHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                string message = res.Data.Message;
                MessageBox.Show(message);
            }
            else
            {
                string message = res.Data;
                MessageBox.Show(message);
            }
        }

        private void GetMessageHandle(Response res, ref Dictionary<int, LinkedList<Message>> dictionary)
        {
            int id = res.Data.Id;
            string text = res.Data.Message;
            Message message = new() { Text = text, IsIncoming = true };

            foreach (var propMessage in dictionary)
            {
                if (propMessage.Key == id)
                {
                    propMessage.Value.AddLast(message);
                    if (currentProp.Id == id)
                    {
                        Application.Current.Dispatcher.Invoke(() => CurrentMessages.Add(message));
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Accepting a message from a contact from the north.
        /// Inserting a message into "Dictionary contactMessages" and " ObservableCollection<Message> CurrentMessages"
        /// </summary>
        /// <param name="res"></param>
        private void GetMessageFromContactHandle(Response res) => GetMessageHandle(res, ref contactMessages);


        /// <summary>
        /// Accepting a message from a group from the north.
        /// Inserting a message into "Dictionary groupMessages" and " ObservableCollection<Message> CurrentMessages"
        /// </summary>
        /// <param name="res"></param>
        private void GetMessageFromGroupHandle(Response res) => GetMessageHandle(res, ref groupMessages);


        private void RenameHandle(Response res, ref ObservableCollection<Prop> collection)
        {
            if (res.Status == Status.OK)
            {
                int id = res.Data.Id;
                string newName = res.Data.Name;

                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].Id == id)
                    {
                        Prop newContact = new() { Id = id, Name = newName };
                        collection[i] = newContact;
                        break;
                    }
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// Processing a message from the server about renaming a contact.
        /// Search for a contact in the ObservableCollection<Prop> Contacts and change the Сontacts.Name
        /// </summary>
        /// <param name="res"></param>
        private void RenameContactHandle(Response res) => RenameHandle(res, ref contacts);


        /// <summary>
        /// Processing a message from the server about renaming a group.
        /// Search for a group in the ObservableCollection<Prop> Groups and change the Groups.Name
        /// </summary>
        /// <param name="res"></param>
        private void RenameGroupHandle(Response res) => RenameHandle(res, ref groups);


        /// <summary>
        /// Login processing from the server.
        /// Obtaining data about contacts, groups and invites, entering into the appropriate arrays.
        /// </summary>
        /// <param name="res"></param>
        private void SignInHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Prop user = new()
                {
                    Id = res.Data.User.Id,
                    Name = res.Data.User.Name
                };
                User = user;

                var resGroups = res.Data.Groups;
                foreach (var group in resGroups)
                {
                    Prop groupProp = new() { Id = group.Id, Name = group.Name };
                    groupMessages.Add(groupProp.Id, new());
                    Application.Current.Dispatcher.Invoke(() => groups.Add(groupProp));
                }

                var invites = res.Data.Invites;
                foreach (var invite in invites)
                {
                    Prop inviteProp = new() { Id = invite.Id, Name = invite.Name };
                    Application.Current.Dispatcher.Invoke(() => Invites.Add(inviteProp));
                }

                var resContacts = res.Data.Contacts;
                foreach (var contact in resContacts)
                {
                    Prop contactProp = new() { Id = contact.Id, Name = contact.Name };
                    contactMessages.Add(contactProp.Id, new());
                    Application.Current.Dispatcher.Invoke(() => contacts.Add(contactProp));
                }

                ConfirmSign();
            }
            else
            {
                string message = res.Data;
                DenySign(message);
            }
        }

        /// <summary>
        /// Processing a registration request from the server.
        /// Creating a new user.
        /// </summary>
        /// <param name="res"></param>
        private void SignUpHandle(Response res)
        {
            if (res.Status == Status.OK)
            {
                Prop user = new()
                {
                    Id = res.Data.Id,
                    Name = res.Data.Name
                };
                User = user;
                ConfirmSign();
            }
            else
            {
                string message = res.Data;
                DenySign(message);
            }
        }
    }
}