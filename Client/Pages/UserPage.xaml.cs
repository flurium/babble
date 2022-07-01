using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.Services;
using CrossLibrary;

namespace Client
{
  /// <summary>
  /// Логика взаимодействия для UserPage.xaml
  /// </summary>
  public partial class UserPage : Page
  {
    private CommunicationService cs = new CommunicationService();

    public UserPage()
    {
      InitializeComponent();
      DataContext = cs;


      var lbi = new TextBlock();
      lbi.Text = " Hi Liber,afhagsfkhgaskgfkahsgfhagsfhgaagsjhfgajhsgfjhagsjfhgajhsfgjahgsfjhagsjfhgasjfhgajhsgfjhahfbznbcmnzbxmcbzmnxbcznbxcmabhsfahsjfhgashfgkasfkjaksjfkajsfkjaskfjgaksfghasgfhgahsfgjhgasfjhagsjf";

      MessageList.Items.Add(lbi);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      NavigationService.GoBack();
      NavigationService.RemoveBackEntry();
    }

    private void GoToContacts_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 0;

    private void GoToGroups_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 1;

    private void GoToInvites_Click(object sender, RoutedEventArgs e) => ListSection.SelectedIndex = 2;


  }
}