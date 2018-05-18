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
    /// Interaction logic for Locations.xaml
    /// </summary>
    public partial class Locations : Page
    {
        ViewModel vm;

        public Locations(ViewModel vm)
        {
            InitializeComponent();
            this.vm = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(nickname.Text) || String.IsNullOrEmpty(address.Text) || String.IsNullOrEmpty(city.Text))
            {
                MessageBox.Show("Invalid data entry. Check all of the fields to make sure you entered the correct data.");
            }
            else
            {
                string enteredAddress = address.Text;
                string enteredCity = city.Text;

                foreach(Location loc in vm.allLocations)
                {
                    if(loc.address == enteredAddress && loc.city == enteredCity)
                    {
                        MessageBox.Show("The location already exists.");
                        return;
                    }
                }
                vm.createLocation(enteredAddress, enteredCity, "WA", nickname.Text, vm.allLocations[0].latitude, vm.allLocations[1].longitude);
            }

            Home home = new Home(vm, true);
            this.NavigationService.Navigate(home);

        }
    }
}
