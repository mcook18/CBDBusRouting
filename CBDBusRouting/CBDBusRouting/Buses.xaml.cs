using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class BusesPage : Page
    {
        ViewModel vm;
        BusForView bfv;
        bool edit;

        public BusesPage(ViewModel vm)
        {
            InitializeComponent();
            this.vm = vm;
        }

        public BusesPage(ViewModel vm, BusForView busToEdit)
        {
            edit = true;
            InitializeComponent();
            this.vm = vm;

            bfv = busToEdit;

            busSize.Text = busToEdit.capacity.ToString();
            numBuses.Text = busToEdit.quantity.ToString();
            isAccess.IsChecked = busToEdit.access;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(busSize.Text) || String.IsNullOrEmpty(numBuses.Text))
            {
                MessageBox.Show("Invalid data entry. Check all of the fields to make sure you entered the correct data.");
            }
            else
            {
                if (!edit)
                {
                    bool needsAccess = isAccess.IsChecked.Value;
                    vm.allBusSettings.Add(new BusForView(Convert.ToInt32(busSize.Text), Convert.ToInt32(numBuses.Text), needsAccess));
                }
                else
                {
                    bfv.capacity = Convert.ToInt32(busSize.Text);
                    bfv.quantity = Convert.ToInt32(numBuses.Text);
                    bfv.access = isAccess.IsChecked.Value;
                }
            }

            Home home = new Home(vm, false);
            this.NavigationService.Navigate(home);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
