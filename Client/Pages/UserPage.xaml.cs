using Client.Services;
using CrossLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using static Client.Services.CommunicationService;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    ///
    public partial class UserPage : Page
    {
        private readonly CommunicationService cs;
        private List<string>? selectedFiles;

        public UserPage(CommunicationService cs)
        {
            InitializeComponent();
            this.cs = cs;
            DataContext = cs;
        }

        private void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsList.SelectedIndex != -1)
            {
                Prop contact = (Prop)ContactsList.SelectedItem;

                cs.SetState(new ContactState());
                cs.CurrentProp = contact;

                OpenSettingsBtn.IsEnabled = true;
                MessageWrite.Focus();
                ChatName.Content = contact.Name;
                MessageWrite.IsEnabled = true;
            }
        }

        private void ClearInputs()
        {
            ChatName.Content = "";
            InviteContact.Text = "";
            GroupInput.Text = "";
            MessageWrite.Text = "";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
            NavigationService.RemoveBackEntry();
            cs.Disconnect();
            ClearInputs();
        }

        private void GoToContacts_Click(object sender, RoutedEventArgs e)
        {
            ListSection.SelectedIndex = 0;
            GroupsList.SelectedIndex = -1;
        }

        private void GoToGroups_Click(object sender, RoutedEventArgs e)
        {
            ListSection.SelectedIndex = 1;
            ContactsList.SelectedIndex = -1;
        }

        private void GoToInvites_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 2;

        private void GroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupsList.SelectedIndex != -1)
            {
                Prop group = (Prop)GroupsList.SelectedItem;

                cs.SetState(new GroupState());
                cs.CurrentProp = group;

                OpenSettingsBtn.IsEnabled = true;
                MessageWrite.Focus();
                ChatName.Content = group.Name;
                MessageWrite.IsEnabled = true;
            }
        }

        private void MessageList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer scrollViewer &&
              Math.Abs(e.ExtentHeightChange) > 0.0)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        private void MessageSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void RenameBtn_Click(object sender, RoutedEventArgs e)
        {
            string newName = ReNameInput.Text.Trim();
            if (newName != "") cs.Rename(newName); ChatName.Content = newName;
        }

        private void SendMessage()
        {
            string message = MessageWrite.Text.Trim();
            if (message != "")
            {
                cs.SendMessage(message, selectedFiles);
                MessageWrite.Text = "";
                MessageWrite.Focus();
            }
        }

        private void SendInviteBtn_Click(object sender, RoutedEventArgs e)
        {
            string inviteContact = InviteContact.Text.Trim();
            if (inviteContact != "")
            {
                cs.SendInvite(inviteContact);
                InviteContact.Text = "";
            }
        }

        private void EnterGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            string group = GroupInput.Text.Trim();
            if (group != "")
            {
                cs.EnterGroup(group);
                GroupInput.Text = "";
            }
        }

        private void AcceptInviteBtn_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            cs.AcceptInvite(id);
        }

        private void CreateGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            string group = GroupInput.Text.Trim();
            if (group != "")
            {
                cs.CreateGroup(group);
            }
        }

        private void FileBth_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Title = "Choose files"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilesText.Text = "";
                SelectedFilesText.Visibility = Visibility.Visible;
                UnselectFilesBtn.Visibility = Visibility.Visible;

                selectedFiles = new List<string>(openFileDialog.FileNames);
                foreach (string file in selectedFiles)
                {
                    SelectedFilesText.Text += string.Format("{0} ", file.Substring(file.LastIndexOf('\\') + 1));
                }
            }
        }

        private void ShowInFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            //(Button)sender;
        }

        private void UnselectFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedFiles = null;
            SelectedFilesText.Text = "";
            SelectedFilesText.Visibility = Visibility.Collapsed;
            UnselectFilesBtn.Visibility = Visibility.Collapsed;
        }

        private void ChatName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChatOption.Visibility = Visibility.Visible;
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            ChatOption.Visibility = Visibility.Hidden;
        }

        private void DeleteChat_Click(object sender, RoutedEventArgs e)
        {
        }

        private void OpenSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            ListSection.SelectedIndex = 3;
        }
    }
}