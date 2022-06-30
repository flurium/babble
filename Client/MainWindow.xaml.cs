using Client.Pages;
using System.Windows;

namespace Client
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public static UserPage userChat = new UserPage();
    public static SignInPage signIn = new SignInPage();
    public static SignUpPage signUp = new SignUpPage();

    public MainWindow()
    {
      InitializeComponent();
      this.MinWidth = 400;
      MainFrame.Content = signIn;
    }
  }
}