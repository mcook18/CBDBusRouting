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

namespace CBDBusRouting
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void Button_Click_G(object sender, RoutedEventArgs e)
        {   
            //go to groups page from home
            GroupsPage groupspage = new GroupsPage();
            this.NavigationService.Navigate(groupspage);

        }

        private void Button_Click_B(object sender, RoutedEventArgs e)
        {
            buses buses = new buses();
            this.NavigationService.Navigate(buses);
        }

        private void Button_Click_L(object sender, RoutedEventArgs e)
        {
            Locations locations = new Locations();
            this.NavigationService.Navigate(locations);
        }

        private void Button_Click_R(object sender, RoutedEventArgs e)
        {

        }
    }
}
