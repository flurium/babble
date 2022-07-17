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
        private readonly CommunicationService cs;

        public SignInPage(CommunicationService cs)
        {
            InitializeComponent();
            this.cs = cs;
        }

        private void ReverseIsEnabled()
        {
            SignInBtn.IsEnabled = !SignInBtn.IsEnabled;
            GoToSignUpBtn.IsEnabled = !GoToSignUpBtn.IsEnabled;
            NameInput.IsEnabled = !NameInput.IsEnabled;
            PasswordInput.IsEnabled = !PasswordInput.IsEnabled;
        }

        private void ConfirmSignIn()
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(MainWindow.userChat);
                NavigationService.RemoveBackEntry();
                ReverseIsEnabled();
                NameInput.Text = "";
                PasswordInput.Password = "";
            });
        }

        private void DenySignIn(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ReverseIsEnabled();
                MessageBox.Show(message);
            });
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
                ReverseIsEnabled();

                cs.DenySign = DenySignIn;
                cs.ConfirmSign = ConfirmSignIn;

                cs.SignIn(name, password);
            }
        }

        private void GoToSignUp(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainWindow.signUp);
            NavigationService.RemoveBackEntry();
        }
    }
}