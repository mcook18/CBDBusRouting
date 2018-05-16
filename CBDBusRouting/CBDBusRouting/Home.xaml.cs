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
            BusesPage busesPage = new BusesPage();
            this.NavigationService.Navigate(busesPage);
        }

        private void Button_Click_L(object sender, RoutedEventArgs e)
        {
            Locations locations = new Locations();
            this.NavigationService.Navigate(locations);
        }

        private void Button_Click_R(object sender, RoutedEventArgs e)
        {
            // Refresh locations here
        }

        private void Button_Click_Gimme(object sender, RoutedEventArgs e)
        {
            // Get results here
        }

        private void Button_Click_EditB(object sender, RoutedEventArgs e)
        {
            // Edit buses here
        }

        private void Button_Click_BusDelete(object sender, RoutedEventArgs e)
        {
            // Delete buses here
        }

        private void Button_Click_LSave(object sender, RoutedEventArgs e)
        {
            // Save Locations here
        }

        private void Button_Click_GSave(object sender, RoutedEventArgs e)
        {
            // Save groups here
        }

        private void Button_Click_LocEdit(object sender, RoutedEventArgs e)
        {
            // Edit locations here
        }

        private void Button_Click_LocDelete(object sender, RoutedEventArgs e)
        {
            // Delete locations here
        }

        private void Button_Click_GEdit(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_GDelete(object sender, RoutedEventArgs e)
        {

        }
     }
}
