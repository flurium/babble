using Client.Pages;
using Client.Services.Communication;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static SignInPage signIn;
        public static SignUpPage signUp;
        public static UserPage userChat;
        private readonly CommunicationService cs = new();

        public MainWindow()
        {
            InitializeComponent();

            userChat = new UserPage(cs, SetTitle);
            signIn = new SignInPage(cs, SetTitle);
            signUp = new SignUpPage(cs, SetTitle);

            MainFrame.Content = signIn;
        }

        private void SetTitle(string title)
        {
            Title = string.Format("BabbleUp {0}", title);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cs.Disconnect();
        }
    }
}