using System.Windows.Controls;
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

namespace Client.Pages
{
    /// <summary>
    /// Логика взаимодействия для ConfirmPage.xaml
    /// </summary>
    public partial class ConfirmPage : Page
    {
        private readonly CommunicationService cs;

        internal ConfirmPage(CommunicationService cs, Action<string> setTitle)
        {
            InitializeComponent();
            this.cs = cs;
            DataContext = cs;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainWindow.userChat);
            NavigationService.RemoveBackEntry();
        }

        private void AcceptInvite_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            cs.AcceptInvite(id);
        }

        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            string group = GroupInput.Text.Trim();
            if (group != "")
            {
                cs.CreateGroup(group);
                GroupInput.Text = "";
            }
        }

        private void SendInvite_Click(object sender, RoutedEventArgs e)
        {
            string inviteContact = InviteInput.Text.Trim();
            if (inviteContact != "")
            {
                cs.SendInvite(inviteContact);
                InviteInput.Text = "";
            }
        }

        private void RejectInvite_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            cs.RejectInvite(id);
        }

        private void EnterGroup_Click(object sender, RoutedEventArgs e)
        {
            string group = GroupInput.Text.Trim();
            if (group != "")
            {
                cs.EnterGroup(group);
                GroupInput.Text = "";
            }
        }
    }
}