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
    public SignUpPage()
    {
      InitializeComponent();
    }

    private void SignUpNewAcc(object sender, RoutedEventArgs e)
    {
      // sign up code
      NavigationService.Navigate(MainWindow.userChat);
    }

    private void SignIn(object sender, RoutedEventArgs e)
    {
      NavigationService.Content = MainWindow.signIn;
    }
  }
}