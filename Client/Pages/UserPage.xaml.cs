using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Client.Pages
{
  /// <summary>
  /// Interaction logic for UserPage.xaml
  /// </summary>
  public partial class UserPage : Page
  {
    public UserPage()
    {
      InitializeComponent();
      var lbi = new TextBlock();
      lbi.Background = Brushes.DarkSlateBlue;
      lbi.Foreground = Brushes.AliceBlue;
      lbi.Text = "Liber";
      lbi.Height = 30;
      ContactsList.Items.Add(lbi);

      lbi = new TextBlock();
      lbi.Text = " Hi Liber,afhagsfkhgaskgfkahsgfhagsfhgaagsjhfgajhsgfjhagsjfhgajhsfgjahgsfjhagsjfhgasjfhgajhsgfjhahfbznbcmnzbxmcbzmnxbcznbxcmabhsfahsjfhgashfgkasfkjaksjfkajsfkjaskfjgaksfghasgfhgahsfgjhgasfjhagsjf";

      MessageList.Items.Add(lbi);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      NavigationService.GoBack();
    }
  }
}