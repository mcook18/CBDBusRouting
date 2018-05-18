using System;
using System.Collections.Generic;
using System.IO;
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
        public ViewModel vm;
        public Home()
        {
            InitializeComponent();
            vm = new ViewModel();

            refreshListBoxes();
        }

        // after adding or editing a group
        public Home(ViewModel vm)
        {
            InitializeComponent();
            this.vm = vm;
            refreshListBoxes();
        }

        // constructor used to show either location or bus tab immediately - determined by comingFromLocation boolean
        public Home(ViewModel vm, bool comingFromLocation)
        {
            InitializeComponent();
            this.vm = vm;
            refreshListBoxes();
            if (comingFromLocation)
            {
                Tabs.SelectedIndex = 1;
            }
            else
            {
                Tabs.SelectedIndex = 2;
            }
            refreshListBoxes();
        }

        private void refreshListBoxes()
        {
            CompleteList.ItemsSource = vm.allGroups.Where<Group>(g => g.runFlag == false);
            RunList.ItemsSource = vm.allGroups.Where<Group>(g => g.runFlag == true);
            LocationList.ItemsSource = vm.allLocations.Where<Location>(loc => loc.nickname != "Whitworth University (Home)");
            BusList.ItemsSource = vm.allBusSettings;
            LocationList.Items.Refresh();
            BusList.Items.Refresh();
        }

        private void save()
        {
            // Locations are saved in the csv as they are created
            vm.csv.outputGroupsDataCsv(vm.allGroups);
            vm.csv.outputBusDataCsv(vm.allBusSettings);
            vm.csv.outputLocationDataCsv(vm.allLocations);
        }

        private void Button_Click_G(object sender, RoutedEventArgs e)
        {   
            //go to groups page from home
            this.NavigationService.Navigate(new GroupsPage(vm));

        }

        private void Button_Click_B(object sender, RoutedEventArgs e)
        {
            BusesPage busesPage = new BusesPage(vm);
            this.NavigationService.Navigate(busesPage);
        }

        private void Button_Click_L(object sender, RoutedEventArgs e)
        {
            Locations locations = new Locations(vm);
            this.NavigationService.Navigate(locations);
        }

        private void Button_Click_Gimme(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The algorithm will take a few seconds to run, please be patient.");
            // Get results here
            vm.prepAndRunAlgorithm();

            if (vm.results.Count() != 0)
            {
                string resultsFilename = "Results_" + DateTime.Now.ToFileTime() + ".csv";
                vm.csv.outputResultsCsv(vm.results, resultsFilename);
                MessageBox.Show("The algorithm has completed.  The output file is named " + resultsFilename);
            }
            else
            {
                MessageBox.Show("The algorithm couldn't find a solution for the current setup.\n" +
                    "Consider the following when trying to find a valid setup:\n" +
                    "-Are there enough buses to fit all the groups?\n" +
                    "-Are there enough accessible buses to accommodate the groups with accessibility needs?\n" +
                    "-Are all the groups that you want to include in the right-side box in the Groups page?\n" +
                    "-Is there a location that is more than 40 minutes away? If so, consider excluding it from this program, just remember to give them a bus on Community Building Day!\n" +
                    "-If you had a setup that worked before, did you forget to save your changes after running it?");
            }
        }

        private void Button_Click_EditB(object sender, RoutedEventArgs e)
        {
            if (BusList.SelectedIndex != -1)
            {
                // Edit buses here
                this.NavigationService.Navigate(new BusesPage(vm, BusList.SelectedItem as BusForView));
            }
            else
            {
                MessageBox.Show("There is no bus selected to edit.");
            }
        }

        private void Button_Click_BusDelete(object sender, RoutedEventArgs e)
        {
            if (BusList.SelectedIndex != -1)
            {
                // Delete buses here
                BusForView toDelete = BusList.SelectedItem as BusForView;
                vm.allBusSettings.Remove(toDelete);
                refreshListBoxes();
            }
            else
            {
                MessageBox.Show("There is no bus selected to delete.");
            }
        }

        private void Button_Click_LSave(object sender, RoutedEventArgs e)
        {
            // Save Locations here
            save();
        }

        private void Button_Click_GSave(object sender, RoutedEventArgs e)
        {
            // Save groups here
            save();
        }

        private void Button_Click_LocDelete(object sender, RoutedEventArgs e)
        {
            if (LocationList.SelectedIndex != -1)
            {
                // Delete locations here
                Location toDelete = LocationList.SelectedItem as Location;
                vm.allLocations.Remove(toDelete);
                foreach (Location loc in vm.allLocations)
                {
                    loc.distanceDict.Remove(toDelete);
                }

                foreach (Group g in vm.allGroups)
                {
                    if (g.destination == toDelete)
                    {
                        g.destination.address = null;
                        g.destination.city = null;
                        g.destination.state = null;
                        g.runFlag = false;
                    }
                }
                refreshListBoxes();
            }
            else
            {
                MessageBox.Show("There is no location selected to delete.");
            }
        }

        private void Button_Click_GEdit(object sender, RoutedEventArgs e)
        {
            //go to groups page from home
            if (CompleteList.SelectedIndex != -1)
            {
                this.NavigationService.Navigate(new GroupsPage(vm, CompleteList.SelectedItem as Group));
            }
            else
            {
                MessageBox.Show("There is no group selected in the 'All the Groups' list to edit.");
            }
        }

        private void Button_Click_GDelete(object sender, RoutedEventArgs e)
        {
            if (CompleteList.SelectedIndex != -1)
            {
                Group toDelete = CompleteList.SelectedItem as Group;
                vm.allGroups.Remove(toDelete);
                refreshListBoxes();
            }
            else
            {
                MessageBox.Show("There is no selected group in the 'All the Groups' list to delete.");
            }
        }

        private void MoveRunGroupToAllGroup_Click(object sender, RoutedEventArgs e)
        {
            if (RunList.SelectedIndex != -1)
            {
                Group temp = RunList.SelectedItem as Group;
                temp.flipRunFlag();
                refreshListBoxes();
            }
            else
            {
                MessageBox.Show("There is no selected group in the 'Run List of Group'.");
            }

        }

        private void MoveAllGroupToRunGroup_Click(object sender, RoutedEventArgs e)
        {
            if (CompleteList.SelectedIndex != -1)
            {
                Group temp = CompleteList.SelectedItem as Group;
                if (temp.destination.address == null)
                {
                    MessageBox.Show("This group's location was deleted. Please edit the group's location.");
                }
                else
                {
                    temp.flipRunFlag();
                    refreshListBoxes();
                }
            }
            else
            {
                MessageBox.Show("There is no selected group in the 'All the Groups' list.");
            }
        }
    }
}
