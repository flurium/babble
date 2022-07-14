using Client.Services;
using CrossLibrary;
using System;
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
    private CommunicationService cs;

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
        cs.SetCurrentProp(contact);

        MessageWrite.Focus();
        ChatName.Text = contact.Name;
        MessageWrite.IsEnabled = true;
      }
    }

    private void ClearInputs()
    {
      ChatName.Text = "";
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
        cs.SetCurrentProp(group);

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
        cs.SendMessage(message);
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
  }
}