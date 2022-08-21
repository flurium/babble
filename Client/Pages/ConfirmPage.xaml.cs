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
        }

        private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void GroupMake_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
        private void Invite_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}