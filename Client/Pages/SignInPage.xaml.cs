using Client.Services;
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
    private CommunicationService cs;

    public SignInPage(CommunicationService cs)
    {
      InitializeComponent();
      this.cs = cs;
    }

    private void SignIn(object sender, RoutedEventArgs e)
    {
      string name = NameInput.Text.Trim();
      string password = PasswordInput.Password.Trim();

      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
      {
        MessageBox.Show("Name or password is empty");
      }
      else
      {
        cs.SignIn(name, password);

        // only if sign in
        // check
        NavigationService.Navigate(MainWindow.userChat);
        NavigationService.RemoveBackEntry();
      }
    }

    private void SignUp(object sender, RoutedEventArgs e)
    {
      NavigationService.Navigate(MainWindow.signUp);
      NavigationService.RemoveBackEntry();
    }
  }
}