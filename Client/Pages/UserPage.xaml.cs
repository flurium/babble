using Client.Services;
using CrossLibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Client
{
  /// <summary>
  /// Логика взаимодействия для UserPage.xaml
  /// </summary>
  ///
  public struct Message
  {
    public bool IsIncoming { get; set; }
    public string String { get; set; }
  }

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
        cs.SetCurrentContact(contact);
        MessageWrite.Focus();
        ChatName.Text = contact.Name;
      }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      NavigationService.GoBack();
      NavigationService.RemoveBackEntry();
      cs.Disconnect();
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
        cs.SetCurrentGroup(group);
        MessageWrite.Focus();
        ChatName.Text = group.Name;
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
      RenameBtn.Content = ChatName.IsReadOnly ? "Rename" : "Confirm";
      ChatName.Focus();
      ChatName.CaretIndex = ChatName.Text.Length;
      ChatName.FontSize = ChatName.FontSize == 14 ? 12 : 14;
    }

    private void SendMessage()
    {
      string message = MessageWrite.Text.Trim();
      if (message != "")
      {
        if (ListSection.SelectedIndex == 0 && ContactsList.SelectedIndex != -1)
        {
          cs.SendMessageToContact(message);
          MessageWrite.Text = "";
          MessageWrite.Focus();
        }
        else if (ListSection.SelectedIndex == 1 && GroupsList.SelectedIndex != -1)
        {
          cs.SendMessageToGroup(message);
          MessageWrite.Text = "";
          MessageWrite.Focus();
        }
      }
    }

    private void SendInviteBtn_Click(object sender, RoutedEventArgs e)
    {
      string inviteContact = InviteContact.Text.Trim();
      if (inviteContact != "")
      {
        cs.SendInvite(inviteContact);
      }
    }

    private void EnterGroupBtn_Click(object sender, RoutedEventArgs e)
    {
      string group = EnterGroup.Text.Trim();
      if (group != "")
      {
        //cs.EnterGroup(group);
      }
    }

    private void AcceptInviteBtn_Click(object sender, RoutedEventArgs e)
    {
      int id = (int)((Button)sender).Tag;

      cs.AcceptInvite(id);
    }
  }
}