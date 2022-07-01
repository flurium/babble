using Client.Pages;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Client
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  /// 

  public partial class MainWindow : Window
  {

    public static UserPage userChat = new UserPage();
    public static SignInPage signIn = new SignInPage();
    public static SignUpPage signUp = new SignUpPage();


    public MainWindow()
    {
      InitializeComponent();
      MainFrame.Content = signIn;
    }
  }
}