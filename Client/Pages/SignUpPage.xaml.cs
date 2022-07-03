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
      string name = NameInput.Text.Trim();
      string password = PasswordInput.Password.Trim();
      string passwordConfirm = PasswordConfirmInput.Password.Trim();

      if (password != passwordConfirm)
      {
        MessageBox.Show("Password confirm is failed");
        PasswordConfirmInput.Password = "";
      }
      else if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
      {
        MessageBox.Show("Name or password is empty");
      }
      else
      {
        cs.SignUp(name, password);

        // sign up code
        NavigationService.Navigate(MainWindow.userChat);
        NavigationService.RemoveBackEntry();
      }
    }
  }
}