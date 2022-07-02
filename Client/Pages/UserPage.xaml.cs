using Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Client
{
  /// <summary>
  /// Логика взаимодействия для UserPage.xaml
  /// </summary>
  /// 
  public struct Message
  {
    public string String { get; set; }
    public bool IsIncoming { get; set; }
  }

  public partial class UserPage : Page
  {
    private CommunicationService cs;

    public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

    public UserPage(CommunicationService cs)
    {
      InitializeComponent();
      this.cs = cs;
      ContactsList.DataContext = cs;
      GroupsList.DataContext = cs;
      InvitesList.DataContext = cs;
      MessageList.DataContext = this;
      
      AddMessage("Phasellus vitae quam arcu. Sed ac nunc metus", true);
      AddMessage("Phasellus vitae quam arcu. Sed ac nunc metus. Nulla tellus mi, ornare vitae metus in, accumsan fringilla ex. Proin felis ligula, euismod non tellus sed, rhoncus hendrerit erat.", true);
    }

    // if isIncomming == false message will be at right side
    // else message will be at left side
    private void AddMessage(string message, bool isIncoming = false)
    {
      Dispatcher.BeginInvoke(() =>
      {
        Messages.Add(new Message { String = message, IsIncoming = isIncoming });
      }, null);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      NavigationService.GoBack();
      NavigationService.RemoveBackEntry();
      cs.Disconnect();
    }

    private void GoToContacts_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 0;

    private void GoToGroups_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 1;

    private void GoToInvites_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 2;

    private void MessageSend_Click(object sender, RoutedEventArgs e)
    {
      if (MessageWrite.Text.Trim() != "")
      {
        AddMessage(MessageWrite.Text.Trim());
        MessageWrite.Text = "";
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
  }
}