using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Client.Pages
{
  /// <summary>
  /// Interaction logic for SignInPage.xaml
  /// </summary>
  public partial class SignInPage : Page
  {
    public SignInPage()
    {
      InitializeComponent();
    }

    private void SignUp(object sender, RoutedEventArgs e)
    {
      NavigationService.Content = MainWindow.signUp;
    }

    private void SignIn_Click(object sender, RoutedEventArgs e)
    {
      NavigationService.Content = MainWindow.userChat;
    }
  }
}