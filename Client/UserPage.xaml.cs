using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            var lbi = new TextBlock();
            lbi.Background = Brushes.DarkSlateBlue;
            lbi.Foreground = Brushes.AliceBlue;
            lbi.Text = "Liber";
            lbi.Height = 30;
            ContactsList.Items.Add(lbi);


             lbi = new TextBlock();
             lbi.Text = " Hi Liber,afhagsfkhgaskgfkahsgfhagsfhgaagsjhfgajhsgfjhagsjfhgajhsfgjahgsfjhagsjfhgasjfhgajhsgfjhahfbznbcmnzbxmcbzmnxbcznbxcmabhsfahsjfhgashfgkasfkjaksjfkajsfkjaskfjgaksfghasgfhgahsfgjhgasfjhagsjf";
            
            MessageList.Items.Add(lbi);


        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
