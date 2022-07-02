using Client.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Client.Pages
{
  /// <summary>
  /// Interaction logic for SignUpPage.xaml
  /// </summary>
  public partial class SignUpPage : Page
  {
    private CommunicationService cs;

    public SignUpPage(CommunicationService cs)
    {
      InitializeComponent();
      this.cs = cs;
    }

    private void SignIn(object sender, RoutedEventArgs e)
    {
      NavigationService.Navigate(MainWindow.signIn);
      NavigationService.RemoveBackEntry();
    }

    private void SignUpNewAcc(object sender, RoutedEventArgs e)
    {
      // sign up code
      NavigationService.Navigate(MainWindow.userChat);
      NavigationService.RemoveBackEntry();
    }
  }
}