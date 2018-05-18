using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public class ViewModel
    {
        public List<Group> allGroups;
        public List<Group> runGroups;

        public List<Location> allLocations;

        public List<BusForView> allBusSettings;

        public List<Bus> results;
        public CSVHandler csv;

        public ViewModel()
        {
            allLocations = new List<Location>();
            allBusSettings = new List<BusForView>();
            allGroups = new List<Group>();
            runGroups = new List<Group>();
            results = new List<Bus>();
            csv = new CSVHandler();

            // DON'T CHANGE ME BRO
            createHome("300 W Hawthorne Road", "Spokane", "WA", "Whitworth University (Home)");

            csv.readInLocationsAndDistances(allLocations);
            csv.readInGroups(allGroups, allLocations);
            csv.readInBuses(allBusSettings);

            //Location loc2 = new Location("1224 E Trent Ave", "Spokane", "WA", "Union Gospel Mission", allLocations[0].latitude, allLocations[0].longitude);

            //Group g = new Group("Whitworth University", "Pete Tucker", loc2, -1, 24, false, true);
            //Group g1 = new Group("Union Gospel Mission", "Anne Trefry", loc2, -1, 36, false, false);

            //allGroups.Add(g);
            //allGroups.Add(g1);
            //csv.outputGroupsDataCsv(allGroups);

            //BusForView bus1 = new BusForView(50, 2, false);
            //BusForView bus2 = new BusForView(45, 10, true);
            //allBusSettings.Add(bus1);
            //allBusSettings.Add(bus2);

            //csv.outputBusDataCsv(allBusSettings);
        }

        public void createHome(string address, string city, string state, string nickname)
        {
            allLocations.Clear();
            Location home = new Location(address, city, state, nickname);
            allLocations.Add(home);
        }

        public void createLocation(string address, string city, string state, string nickname, double homeLat, double homeLong)
        {
            Location newLoc = new Location(address, city, state, nickname, homeLat, homeLong);
            allLocations.Add(newLoc);
            newLoc.computeDistancesBetweenAllLocations(allLocations);
            //csv.createLocation(allLocations);
        }

        public void prepAndRunAlgorithm()
        {
            //List<Bus> tempResults = new List<Bus>();

            List<Bus> busInputArr = new List<Bus>();
            foreach (BusForView bfv in allBusSettings)
            {
                for (int q = 0; q < bfv.quantity; q++)
                {
                    busInputArr.Add(new Bus(bfv.capacity, bfv.access));
                }
            }

            Group[] nonAccGroupInputArr = allGroups.Where<Group>(g => g.access == false).ToArray();
            Group[] accGroupInputArr = allGroups.Where<Group>(g => g.access == true).ToArray();

            //List<Location> locationInputArr = new List<Location>();
            Location inputOrigin = allLocations[0]; //allLocations[0] should always be Whitworth, managed through csv etc

            results = Algorithm.runAlgorithm(busInputArr.ToArray(), nonAccGroupInputArr, accGroupInputArr, inputOrigin);

            //List<List<int>> fakePB = new List<List<int>>();
            //List<Cluster> fakeClusters = new List<Cluster>();
            //for (int bus = 0; bus < results.Count(); bus++)
            //{
            //    Console.Write("Bus " + bus + " Capacity " + results[bus].totalSeats + " seats rem: " + results[bus].seatsRemaining(fakePB, fakeClusters, -1, -1, -1) + ",");
            //    for (int group = 0; group < results[bus].groups.Count(); group++)
            //    {
            //        Console.Write(" Group of " + " size " + results[bus].groups[group].numStudents + /*" Location " + buses[bus].groups[group].destination.address + */",");
            //    }
            //    Console.Write("\n");
            //    for (int group = 0; group < results[bus].groups.Count(); group++)
            //    {
            //        Console.Write(results[bus].groups[group].destination.address + "\n");
            //    }

            //    Console.WriteLine("Time taken: " + results[bus].timeTaken());
            //    Console.Write("\n");
            //}
        }
    }
}
