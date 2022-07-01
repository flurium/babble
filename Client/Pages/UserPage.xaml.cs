using Client.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Client
{
  /// <summary>
  /// Логика взаимодействия для UserPage.xaml
  /// </summary>
  public partial class UserPage : Page
  {
    CommunicationService cs;

    public UserPage(CommunicationService cs)
    {
      InitializeComponent();
      this.cs = cs;
      DataContext = cs;
      
      AddMessage("Nunc pulvinar imperdiet neque, ac interdum lacus interdum ut. Ut id blandit metus, id pulvinar mauris. Nulla sapien velit, euismod eu imperdiet vitae, imperdiet eu augue.");
      AddMessage("Phasellus vitae quam arcu. Sed ac nunc metus. Nulla tellus mi, ornare vitae metus in, accumsan fringilla ex. Proin felis ligula, euismod non tellus sed, rhoncus hendrerit erat.");
    }

    private void AddMessage(string message)
    {
      Dispatcher.BeginInvoke(() =>
      {
        MessageList.Items.Add(message);
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
  }
}