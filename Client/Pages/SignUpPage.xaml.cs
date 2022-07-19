using Client.Services.Communication;
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
        private readonly CommunicationService cs;

        public SignUpPage(CommunicationService cs)
        {
            InitializeComponent();
            this.cs = cs;
        }

        private void ReverseIsEnabled()
        {
            SignUpBtn.IsEnabled = !SignUpBtn.IsEnabled;
            GoToSignInBtn.IsEnabled = !GoToSignInBtn.IsEnabled;
            NameInput.IsEnabled = !NameInput.IsEnabled;
            PasswordInput.IsEnabled = !PasswordInput.IsEnabled;
            PasswordConfirmInput.IsEnabled = !PasswordConfirmInput.IsEnabled;
        }

        private void GoToSignIn(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainWindow.signIn);
            NavigationService.RemoveBackEntry();
        }

        private void ConfirmSignUp()
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(MainWindow.userChat);
                NavigationService.RemoveBackEntry();
                ReverseIsEnabled();
                NameInput.Text = "";
                PasswordInput.Password = "";
                PasswordConfirmInput.Password = "";
            });
        }

        private void DenySignUp(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ReverseIsEnabled();
                MessageBox.Show(message);
            });
        }

        private void SignUp(object sender, RoutedEventArgs e)
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
                ReverseIsEnabled();

                cs.SignUp(name, password, ConfirmSignUp, DenySignUp);
            }
        }
    }
}