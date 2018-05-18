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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class GroupsPage : Page
    {
        ViewModel vm;
        Group g;
        bool edit;
        
        public GroupsPage()
        {
            InitializeComponent();
        }

        //for add
        public GroupsPage(ViewModel vm)
        {
            edit = false;
            InitializeComponent();
            this.vm = vm;
            locationDropdown.ItemsSource = vm.allLocations;
        }

        //for edit
        public GroupsPage(ViewModel vm, Group groupToEdit)
        {
            edit = true;
            InitializeComponent();
            this.vm = vm;
            g = groupToEdit;
            locationDropdown.ItemsSource = vm.allLocations;
            groupName.Text = groupToEdit.name;

            int locIndex = -1;
            for(int i = 0; i < vm.allLocations.Count(); i++)
            {
                if (vm.allLocations[i].address == groupToEdit.destination.address)
                {
                    locIndex = i;
                }
            }
            locationDropdown.SelectedIndex = locIndex;

            leader.Text = groupToEdit.leader;
            groupSize.Text = groupToEdit.numStudents.ToString();
            isAccess.IsChecked = groupToEdit.access;
        }

        // For edit group functionality, if implemented

        //public GroupsPage(ViewModel vm, int groupIndex)
        //{
        //    InitializeComponent();
        //    this.vm = vm;
        //    g = vm.allGroups[groupIndex];
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(groupName.Text) || locationDropdown.SelectedIndex <= -1 || String.IsNullOrEmpty(leader.Text) || String.IsNullOrEmpty(groupSize.Text))
            {
                MessageBox.Show("Invalid data entry. Check all of the fields to make sure you entered the correct data.");
            }
            else
            {
                if (!edit)
                {
                    bool needsAccess = isAccess.IsChecked.Value;
                    vm.allGroups.Add(new Group(groupName.Text, leader.Text, locationDropdown.SelectedItem as Location, -1, Convert.ToInt32(groupSize.Text), needsAccess, false));
                }
                else
                {
                    g.name = groupName.Text;
                    g.destination = locationDropdown.SelectedItem as Location;
                    g.leader = leader.Text;
                    g.numStudents = Convert.ToInt32(groupSize.Text);
                    g.access = isAccess.IsChecked.Value;
                    g.runFlag = false;
                }
                //Go back to the home screen and save it. 
                Home home = new Home(vm);
                NavigationService.Navigate(home);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
