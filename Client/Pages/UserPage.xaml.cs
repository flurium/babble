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

      var lbi = new TextBlock();
      lbi.Text = " Hi Liber,afhagsfkhgaskgfkahsgfhagsfhgaagsjhfgajhsgfjhagsjfhgajhsfgjahgsfjhagsjfhgasjfhgajhsgfjhahfbznbcmnzbxmcbzmnxbcznbxcmabhsfahsjfhgashfgkasfkjaksjfkajsfkjaskfjgaksfghasgfhgahsfgjhgasfjhagsjf";

      MessageList.Items.Add(lbi);
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