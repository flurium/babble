using Client.Services.Communication;
using Client.Services.Communication.States;
using CrossLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO;
using System.Diagnostics;

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
        private Action<string> setTitle;

        internal UserPage(CommunicationService cs, Action<string> setTitle)
        {
            InitializeComponent();
            this.cs = cs;
            this.setTitle = setTitle;
            DataContext = cs;
        }

        private void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsList.SelectedIndex != -1)
            {
                Prop contact = (Prop)ContactsList.SelectedItem;

                cs.SetState(new ContactState());
                cs.CurrentProp = contact;

                RenameBtn.IsEnabled = true;
                LeaveBtn.IsEnabled = true;
                MessageWrite.Focus();
                ChatName.Text = contact.Name;
                MessageWrite.IsEnabled = true;
            }
        }

        private void ClearInputs()
        {
            ChatName.Text = "";
            selectedFiles?.Clear();
            MessageWrite.Text = "";
            RenameBtn.IsEnabled = false;
            LeaveBtn.IsEnabled = false;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainWindow.signIn);
            NavigationService.RemoveBackEntry();
            cs.Disconnect();
            ClearInputs();
            setTitle("");
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

        private void GoToInvites_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainWindow.confirm);
            NavigationService.RemoveBackEntry();
        }

        private void GroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupsList.SelectedIndex != -1)
            {
                Prop group = (Prop)GroupsList.SelectedItem;

                cs.SetState(new GroupState());
                cs.CurrentProp = group;

                RenameBtn.IsEnabled = true;
                LeaveBtn.IsEnabled = true;
                MessageWrite.Focus();
                ChatName.Text = group.Name;
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
            ChatName.IsReadOnly = !ChatName.IsReadOnly;

            if (ChatName.IsReadOnly)
            {
                RenameBtn.Content = "Rename";
                ChatName.FontSize = 12;

                // confirm rename
                string newName = ChatName.Text.Trim();
                if (newName != "") cs.Rename(newName);
            }
            else
            {
                RenameBtn.Content = "Confirm";
                ChatName.FontSize = 14;
                ChatName.Focus();
                ChatName.CaretIndex = ChatName.Text.Length;
            }
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
            string downloadFolder = string.Format("{0}\\Downloads\\babble", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            if (Directory.Exists(downloadFolder))
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = downloadFolder,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
        }

        private void UnselectFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedFiles = null;
            SelectedFilesText.Text = "";
            SelectedFilesText.Visibility = Visibility.Collapsed;
            UnselectFilesBtn.Visibility = Visibility.Collapsed;
        }

        private void LeaveBtn_Click(object sender, RoutedEventArgs e)
        {
            cs.Leave();
        }
    }
}