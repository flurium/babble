using Client.Pages;
using Client.Services;
using System.Windows;

namespace Client
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  ///

  public partial class MainWindow : Window
  {
    public static SignInPage signIn;
    public static SignUpPage signUp;
    public static UserPage userChat;
    private CommunicationService cs = new CommunicationService();

    public MainWindow()
    {
      InitializeComponent();

      userChat = new UserPage(cs);
      signIn = new SignInPage(cs);
      signUp = new SignUpPage(cs);

      MainFrame.Content = signIn;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      cs.Disconnect();
    }
  }
}