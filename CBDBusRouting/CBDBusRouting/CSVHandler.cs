using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public class CSVHandler
    {
        public CSVHandler() {}

        public void readInLocationsAndDistances(List<Location> allLocations)
        {
            string locationDataFilename = "StoredDataDoNotTouch\\LocationData.csv";
            int numSavedLocations = File.ReadLines(locationDataFilename).Count() - 2;
            // 2D array from saving location travel times for later use in dictionary
            string[,] locationData = new string[numSavedLocations, numSavedLocations];
            using (StreamReader sr = new StreamReader(locationDataFilename))
            {
                // Skips the headers for each columns
                sr.ReadLine();

                string[] values;
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    values = line.Split(',');
                    if (values.Length >= 8)
                    {
                        if (values[0] != "Whitworth University (Home)")
                        {
                            // Creates a new location with latitude, longitude, x-coordinate (relative to home), y-coordinate (relative to home)
                            Location newLoc = new Location(values[1], values[2], values[3], values[0], Convert.ToDouble(values[4]), Convert.ToDouble(values[5]), Convert.ToDouble(values[6]), Convert.ToDouble(values[7]));
                            allLocations.Add(newLoc);
                        }

                        for (int i = 0; i < numSavedLocations; i++)
                        {
                            // gets the next travel time between current location and the next location
                            locationData[allLocations.Count - 1, i] = values[i + 8];
                        }
                    }
                }

                for (int locationIndex = 0; locationIndex < allLocations.Count; locationIndex++)
                {
                    for (int travelTimeIndex = 0; travelTimeIndex < numSavedLocations; travelTimeIndex++)
                    {
                        if(locationIndex != 0 || travelTimeIndex != 0)
                            allLocations[locationIndex].distanceDict.Add(allLocations[travelTimeIndex], Convert.ToDouble(locationData[locationIndex, travelTimeIndex]));
                    }
                }
            }
        }

        public void readInGroups(List<Group> allGroups, List<Location> allLocations)
        {
            string groupDataFilename = "StoredDataDoNotTouch\\GroupData.csv";
            int numSavedGroups = File.ReadLines(groupDataFilename).Count() - 2;

            using (StreamReader sr = new StreamReader(groupDataFilename))
            {
                // Skips the headers for each columns
                sr.ReadLine();

                string[] values;
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    values = line.Split(',');
                    if (values.Length >= 8)
                    {
                        string address = values[4];
                        string city = values[5];

                        Location newGroupLoc = new Location();

                        foreach(Location loc in allLocations)
                        {
                            if(loc.address == address && loc.city == city)
                            {
                                newGroupLoc = loc;
                                break;
                            }
                        }
                        allGroups.Add(new Group(values[0], values[2], newGroupLoc, -1, Convert.ToInt32(values[1]), Convert.ToBoolean(values[3]), Convert.ToBoolean(values[7])));
                    }
                }
            }
        }

        public void readInBuses(List<BusForView> allBuses)
        {
            string busDataFilename = "StoredDataDoNotTouch\\BusData.csv";
            int numSavedBuses = File.ReadLines(busDataFilename).Count() - 2;

            using (StreamReader sr = new StreamReader(busDataFilename))
            {
                // Skips the headers for each columns
                sr.ReadLine();

                string[] values;
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    values = line.Split(',');
                    if (values.Length == 3)
                    {
                        allBuses.Add(new BusForView(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]), Convert.ToBoolean(values[2])));
                    }
                }
            }
        }

        // Creates location object and outputs an updated LocationData.csv file
        public void outputLocationDataCsv(List<Location> allLocations)
        {
            string filename = "StoredDataDoNotTouch\\LocationData.csv";

            StringBuilder sb = new StringBuilder();
            // Standard row header format
            string rowHeaders = string.Format("Nickname,Address,City,State,Latitude,Longitude,X_Coordinate,Y_Coordinate");

            // Updates header row to include headers for all locations
            for (int i = 1; i <= allLocations.Last().distanceDict.Count; i++)
            {
                rowHeaders += ",Distance_To_Next_Location";
            }
            sb.AppendLine(rowHeaders);

            foreach (Location lctn in allLocations)
            {
                // Start of a new row
                string newRow = string.Format(lctn.nickname + "," + lctn.address + "," + lctn.city + "," + lctn.state + "," + lctn.latitude + "," + lctn.longitude + "," + lctn.coords[0] + "," + lctn.coords[1]);

                foreach (double travelTime in lctn.distanceDict.Values)
                {
                    // Adds corresponding travel time to new row
                    newRow += "," + travelTime;
                }
                sb.AppendLine(newRow);
            }

            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(sb.ToString());
            }
        }

        public void outputGroupsDataCsv(List<Group> allGroups)
        {
            string filename = "StoredDataDoNotTouch\\GroupData.csv";

            StringBuilder sb = new StringBuilder();
            // Standard row header format
            string rowHeaders = string.Format("Group_Name,Number_Of_Students,Faculty_Leader,Accessibility,Address,City,State,Run_Flag");

            sb.AppendLine(rowHeaders);

            foreach (Group grp in allGroups)
            {
                // Start of a new row
                string newRow = string.Format(grp.name + "," + grp.numStudents + "," + grp.leader + "," + grp.access + "," + grp.destination.address + "," + grp.destination.city + "," + grp.destination.state + "," + grp.runFlag);

                sb.AppendLine(newRow);
            }

            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(sb.ToString());
            }
        }

        public void outputBusDataCsv(List<BusForView> allBuses)
        {
            string filename = "StoredDataDoNotTouch\\BusData.csv";

            StringBuilder sb = new StringBuilder();
            // Standard row header format
            string rowHeaders = string.Format("Capacity,Quantity,Access");

            sb.AppendLine(rowHeaders);

            foreach (BusForView bus in allBuses)
            {
                // Start of a new row
                string newRow = string.Format(bus.capacity + "," + bus.quantity + "," + bus.access);

                sb.AppendLine(newRow);
            }

            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(sb.ToString());
            }
        }

        public void outputResultsCsv(List<Bus> results, string resultsFilename)
        {
            StringBuilder sb = new StringBuilder();

            int busCounter = 1;
            List<List<int>> fakePB = new List<List<int>>();
            List<Cluster> fakeClusters = new List<Cluster>();

            string rowHeaders = string.Format("Bus #,Bus Capacity,Destination1,Address1,Leader1,Size1,Accessibility Needs1,Destination2,Address2,Leader2,Size2,Accessibility Needs2,Destination3,Address3,Leader3,Size3,Accessibility Needs3,Bus Accessible,Seats Used,Seats Remaining,Travel Time(minutes)");
            sb.AppendLine(rowHeaders);

            foreach (Bus b in results)
            {
                if (b.groups.Count() > 0)
                {
                    // Start of a new row
                    string newRow = string.Format(busCounter + "," + b.totalSeats + ",");
                    foreach (Group g in b.groups)
                    {
                        newRow += string.Format(g.destination.nickname + "," + g.destination.address + " " + g.destination.city + " " + g.destination.state + "," + g.leader + "," + g.numStudents + ",");
                        if (g.access)
                        {
                            newRow += string.Format("Yes,");
                        }
                        else
                        {
                            newRow += string.Format("No,");
                        }
                    }
                    for(int i = 0; i < 3 - b.groups.Count(); i++)
                    {
                        newRow += string.Format(",,,,,");
                    }
                    if (b.access)
                    {
                        newRow += string.Format("Yes,");
                    }
                    else
                    {
                        newRow += string.Format("No,");
                    }
                    newRow += string.Format(b.seatsTaken(fakePB, fakeClusters, -1, -1, -1) + "," + b.seatsRemaining(fakePB, fakeClusters, -1, -1, -1) + "," + b.timeTaken());

                    sb.AppendLine(newRow);
                    busCounter++;
                }
            }

            using (StreamWriter sw = new StreamWriter(resultsFilename))
            {
                sw.WriteLine(sb.ToString());
            }
        }

        public void openResultsFile(string filename)
        {
            Process.Start(filename);
        }
    }
}
