using Accord.MachineLearning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public static class Algorithm
    {
        //will hold the indexes of clusters, in the order of the groups List
        public static int[] kmeansArr;  //num of groups
        //List<Group> groups = new List<Group>(40);     //changed it to array to accomodate the partition func. Thought this was fine because we only access indexes of 'groups'
        public static Group[] groups;                 //needs to be the right number of groups
        public static Group[] aGroups;                  //needs to be number of accessible groups
        public static List<Cluster> clusters = new List<Cluster>();  //number of clusters
        public static List<List<int>> potentialBuses = new List<List<int>>();
        public static List<Bus> buses; //needs to be the right number of buses
        public static List<Location> locations;
        public static bool validSolution = true;
        public static string invalidSolString = "";


        //sets the values in kmeansArr[]
        public static void runKMeans(ref Group[] gs)
        {
            int numGroups = gs.Count();
            // Declaring and intializing array for K-Means
            double[][] observations = new double[numGroups][];

            for (int i = 0; i < observations.Length; i++)
            {
                observations[i] = new double[2];
                observations[i][0] = gs[i].destination.coords[0];
                observations[i][1] = gs[i].destination.coords[1];
            }

            int numClusters = (gs.Count() / 6) + 1; //THIS WHAT WE WANT?
            KMeans km = new KMeans(numClusters);

            KMeansClusterCollection clust = km.Learn(observations);

            kmeansArr = clust.Decide(observations);
        }

        //create the actual Clusters based on kmeansArr[]
        public static void kMeansToClusters()
        {
            //the index of kmeansArr will correspond to the index of groups[], so just assign them across
            for (int i = 0; i < kmeansArr.Length; i++)
            {
                groups[i].cluster = kmeansArr[i];
                clusters[kmeansArr[i]].members.Add(groups[i]);  //add group to corresponding cluster
            }
        }

        //if a cluster is larger than 10, it is very computationally intensive to find the partitions for it. So we want to split up clusters with members > 10
        //this solution is pretty elegant, and it makes sure that even newly generated clusters and previously split clusters are < 10
        public static void findLargeClusters()
        {
            for (int i = 0; i < clusters.Count; i++)
            {
                while (clusters[i].members.Count > 10)   //even if it splits once, will check it again to see if it is still > 10
                {
                    //split cluster
                    splitCluster(i);
                }
            }
        }

        private static void splitCluster(int index)
        {
            //kmeans with k = 2 for clusters[index].members
            //if kmeansArr == 1, add group to temp list for new cluster, delete from clusters[index]
            //create new Cluster with the temp list, append to end of clusters (the for loop in findLargeClusters will adapt to it and check it later for >15
            int numGroups = clusters[index].members.Count;
            double[][] observations = new double[numGroups][];

            for (int i = 0; i < observations.Length; i++)
            {
                observations[i] = new double[2];
                observations[i][0] = clusters[index].members[i].destination.coords[0];
                observations[i][1] = clusters[index].members[i].destination.coords[1];
            }

            KMeans km = new KMeans(2);

            KMeansClusterCollection clust = km.Learn(observations);

            int[] clustArr = clust.Decide(observations);

            //if a group is in the second of the two clusters, we will put it in a new List and delete it from the old one
            List<Group> forNewCluster = new List<Group>();
            for (int i = clustArr.Length - 1; i >= 0; i--)
            {
                if (clustArr[i] == 1)
                {
                    forNewCluster.Add(clusters[index].members[i]);
                    clusters[index].members.RemoveAt(i);
                }
            }

            Cluster newCluster = new Cluster(forNewCluster);

            //update the cluster attributes in each group for the new cluster
            clusters.Add(newCluster);
            foreach (Group g in clusters[clusters.Count() - 1].members)
            {
                g.cluster = clusters.Count() - 1;
            }
        }

        //since we are clustering the groups and only assigning buses within those clusters, some buses will be left with only one group when they may have fit in a bus that
        //is currently being used in a different cluster. So for single-group buses, we check the surrounding buses and see if we can move that group while staying within the
        //time and capacity limits. If there are multiple candidates, it chooses the bus which would take the least travel time
        public static void moveOnes()
        {
            double shortestTime = 100;
            int newBus = -1;
            //foreach bus, if numGroups == 1, then look for buses NOT in its current cluster that it fits in and time is ok. Record bus with lowest total journey time
            for (int i = 0; i < buses.Count; i++)
            {
                shortestTime = 100;
                newBus = -1;
                //if we have a single-group bus
                if (buses[i].groups.Count == 1)
                {
                    //look for a candidate bus
                    for (int b = 0; b < buses.Count; b++)
                    {
                        //if the group we're moving is accessible, then we need the potential bus to be accessible too
                        if (buses[i].groups[0].access == true)
                        {
                            if (buses[b].access == false)
                            {
                                continue;   //move on to next bus
                            }
                        }
                        //not putting group into same bus AND new potential bus has 1 or 2 groups AND not putting bus into same cluster (earlier condition short circuits)
                        if (b != i && (buses[b].groups.Count == 1 || buses[b].groups.Count == 2) && buses[i].groups[0].cluster != buses[b].groups[0].cluster)
                        {
                            //if fits in bus
                            if (buses[i].groups[0].numStudents < buses[b].seatsRemaining(potentialBuses, clusters, -1, -1, -1))
                            {
                                //if one group on new bus. we split these cases up because totalTime is overloaded with a different number of arguments
                                if (buses[b].groups.Count == 1)
                                {
                                    double journeyTime = totalTime(buses[b].groups[0], buses[i].groups[0]);
                                    if (journeyTime <= 40 && journeyTime < shortestTime)
                                    {
                                        shortestTime = journeyTime;
                                        newBus = b;
                                    }
                                }
                                //if two groups on new bus
                                else
                                {
                                    double journeyTime = totalTime(buses[b].groups[0], buses[b].groups[1], buses[i].groups[0]);
                                    if (journeyTime <= 40 && journeyTime < shortestTime)
                                    {
                                        shortestTime = journeyTime;
                                        newBus = b;
                                    }
                                }
                            }
                        }
                    }
                    //if we can move it to a bus with a shorter time, then do it
                    if (newBus != -1)
                    {
                        //put on bus AKA add to new bus member list, delete from old bus member list
                        //buses[i].groups[0].cluster = buses[b].groups[0].cluster;  LINE NOT NECESSARY, WE ARE DONE WITH CLUSTERS NOW?
                        buses[newBus].groups.Add(buses[i].groups[0]);
                        buses[i].groups.RemoveAt(0);    //MAKE SURE THESE TWO LINES WORK PROPERLY RE: REFERENTIAL EQUALITY
                    }
                }
            }
        }

        //journey time for one group on a bus. Will just be the time it takes to get to that one location
        public static double totalTime(Group B)
        {
            //should always be whitworth
            Location A = locations[0];
            //only combo (AB)
            return A.distanceDict[B.destination];
        }

        //There are two ways to get to two destinations from an origin. Return the fastest one.
        public static double totalTime(Group B, Group C)
        {
            //should always be whitworth
            Location A = locations[0];
            double minTime = 100;
            //first combo (ABC)
            double ABC;
            ABC = A.distanceDict[B.destination] + B.destination.distanceDict[C.destination];
            if (ABC < minTime) minTime = ABC;
            //second combo (ACB)
            double ACB;
            ACB = A.distanceDict[C.destination] + C.destination.distanceDict[B.destination];
            if (ACB < minTime) minTime = ACB;

            return minTime;
        }

        //There are six ways to get to three destinations from an origin. Return the fastest one.
        public static double totalTime(Group B, Group C, Group D)
        {
            //should always be whitworth
            Location A = locations[0];
            double minTime = 100;
            //ABCD
            double ABCD;
            ABCD = A.distanceDict[B.destination] + B.destination.distanceDict[C.destination] + C.destination.distanceDict[D.destination];
            if (ABCD < minTime) minTime = ABCD;
            //ABDC
            double ABDC;
            ABDC = A.distanceDict[B.destination] + B.destination.distanceDict[D.destination] + D.destination.distanceDict[C.destination];
            if (ABDC < minTime) minTime = ABDC;
            //ACBD
            double ACBD;
            ACBD = A.distanceDict[C.destination] + C.destination.distanceDict[B.destination] + B.destination.distanceDict[D.destination];
            if (ACBD < minTime) minTime = ACBD;
            //ACDB
            double ACDB;
            ACDB = A.distanceDict[C.destination] + C.destination.distanceDict[D.destination] + D.destination.distanceDict[B.destination];
            if (ACDB < minTime) minTime = ACDB;
            //ADBC
            double ADBC;
            ADBC = A.distanceDict[D.destination] + D.destination.distanceDict[B.destination] + B.destination.distanceDict[C.destination];
            if (ADBC < minTime) minTime = ADBC;
            //ADCB
            double ADCB;
            ADCB = A.distanceDict[D.destination] + D.destination.distanceDict[C.destination] + C.destination.distanceDict[B.destination];
            if (ADCB < minTime) minTime = ADCB;

            return minTime;
        }

        //looks through the combinations that have been created too see which ones: 1. Have buses that all take <= 40 minutes... 2. Fit on buses
        public static void fillBuses()
        {
            putAccessibleGroupsOnBuses();

            //output accessible buses and the groups they have
            //for (int i = 0; i < buses.Count(); i++)
            //{
            //    if (buses[i].access == true)
            //    {
            //        Console.WriteLine("Accessible bus of size: " + buses[i].totalSeats);
            //        for (int g = 0; g < buses[i].groups.Count(); g++)
            //        {
            //            Console.WriteLine("Group going to " + buses[i].groups[g].destination.address + " Size " + buses[i].groups[g].numStudents);
            //        }
            //        Console.WriteLine("Time taken " + buses[i].timeTaken());
            //        Console.Write("\n");
            //    }
            //}
            //Console.ReadLine();


            //sort clusters descending
            //https://stackoverflow.com/questions/3062513/how-can-i-sort-generic-list-desc-and-asc
            clusters.Sort((a, b) => -1 * a.CompareTo(b));    //should call the CompareTo in the Cluster class (based in IComparable)

            //sort combos within each cluster: fewest groups in combo, then by most 3's ( make a custom < function for Group[][] )
            foreach (Cluster c in clusters)
            {
                c.combos = c.combos.OrderBy<Group[][], int>(combo => combo.Count()).ThenByDescending<Group[][], int>(combo => combo.Where(thebus => thebus.Count() == 3).Count()).ToList();
            }

            //sort buses by capacity low to high
            buses.Sort();

            double timeTaken = 100;
            //for each cluster, move through the 'combos', and if it fits onto some unused buses within time limit, then assign them to those buses!
            for (int cl = 0; cl < clusters.Count; cl++)
            {
                //for each combination in the cluster. A new combination will be tried if the previous one was found to break any of the requirements
                for (int cb = 0; cb < clusters[cl].combos.Count(); cb++)
                {
                    //does it fit the time limit? if not, continue
                    bool fitsTimeLimit = true;
                    //TIME LIMIT CHECKING
                    //foreach clump (basically a bus) of groups in a combo
                    for (int clump = 0; clump < clusters[cl].combos[cb].Count(); clump++)
                    {
                        timeTaken = 100;
                        //since totalTime is overloaded
                        switch (clusters[cl].combos[cb][clump].Count())
                        {
                            case 1:
                                timeTaken = totalTime(clusters[cl].combos[cb][clump][0]);
                                break;
                            case 2:
                                timeTaken = totalTime(clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1]);
                                break;
                            case 3:
                                timeTaken = totalTime(clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1], clusters[cl].combos[cb][clump][2]);
                                break;
                            default:    //will this ever happen?
                                //Console.WriteLine("somehow a clump had <1 or >3 groups");
                                break;
                        }
                        //if it breaks the condition, change fitsTimeLimit
                        if (timeTaken > 40)
                        {
                            //for (int i = 0; i < clusters[cl].combos[cb][clump].Count(); i++)
                            //{
                            //    Console.WriteLine(clusters[cl].combos[cb][clump][i].destination.address);
                            //}
                            //Console.WriteLine("timeTaken: " + timeTaken + " fail  clumpsize: " + clusters[cl].combos[cb][clump].Count());
                            fitsTimeLimit = false;
                            break;
                        }
                        else
                        {
                            //for (int i = 0; i < clusters[cl].combos[cb][clump].Count(); i++)
                            //{
                            //    Console.WriteLine(clusters[cl].combos[cb][clump][i].destination.address);
                            //}
                            //Console.WriteLine("timeTaken: " + timeTaken + " success  clumpsize: " + clusters[cl].combos[cb][clump].Count());
                        }
                    }
                    //after every 'clump' has been tested. If all of them passed the time limit then fitsTimeLimit will still be true
                    if (fitsTimeLimit == false)
                    {
                        continue;
                    }
                    //Console.WriteLine("All clumps in the combo made it through!");

                    //then does it fit in some buses that we have? if not, continue(??)
                    //go through buses. Assign the groups to the buses if they will all fit.
                    //note: currently the clumps of groups within each combo are NOT sorted.
                    //  ^i dont think this will make a huge difference. 
                    potentialBuses.Clear();
                    //potentialBuses represents ONE combination. First list is clump, second is groups. It is used so we don't assign groups to buses before ALL the clumps in a combo have been checked
                    //it is structured the same way as ONE combination from clusters[cl].combos.  So a group's assigned bus is represented by potentialBuses[clump][group]

                    //since we need to restart with potentialBuses every time we reach a new combo, we must recreate the size of it as well for that new combo. Initialize with -1, meaning no bus assigned for that group
                    for (int clumpp = 0; clumpp < clusters[cl].combos[cb].Count(); clumpp++)
                    {
                        potentialBuses.Add(new List<int>());                                //add a 'clump'
                        for (int g = 0; g < clusters[cl].combos[cb][clumpp].Count(); g++)    //add a 'group'
                        {
                            potentialBuses[clumpp].Add(-1);
                        }
                    }

                    //FIT ON BUS CHECK
                    //foreach clump of groups in a combo
                    for (int clump = 0; clump < clusters[cl].combos[cb].Count(); clump++)
                    {
                        //go thru buses to see if it will fit on one of dem. If it fits, then save the index of the bus for the case that they ALL fit
                        //"If it fits, group sits" -Drew
                        //clumpSum represents the total size of the groups in a clump (or bus pairing)
                        int clumpSum = 0;
                        for (int g = 0; g < clusters[cl].combos[cb][clump].Count(); g++)
                        {
                            //Console.WriteLine(clusters[cl].combos[cb][clump][g].destination.address);
                            clumpSum += clusters[cl].combos[cb][clump][g].numStudents;
                        }

                        //HAVE TO FIND TIMETAKEN FOR THE CLUMP AGAIN
                        timeTaken = 100;
                        //since totalTime is overloaded
                        //switch (clusters[cl].combos[cb][clump].Count())
                        //{
                        //    case 1:
                        //        timeTaken = totalTime(clusters[cl].combos[cb][clump][0]);
                        //        break;
                        //    case 2:
                        //        timeTaken = totalTime(clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1]);
                        //        break;
                        //    case 3:
                        //        timeTaken = totalTime(clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1], clusters[cl].combos[cb][clump][2]);
                        //        break;
                        //    default:    //will this ever happen?
                        //        Console.WriteLine("somehow a clump had <1 or >3 groups");
                        //        break;
                        //}

                        double timeOnThisPossibleBus = timeTaken; //travel time of clump and A groups already on bus
                        //see if this clump will fit on a bus.
                        for (int bus = 0; bus < buses.Count(); bus++)
                        {

                            //if the bus' groups attribute does not contain groups AND the bus does not have a potential group assigned to it in potentialBuses AND adding the clump to the bus wouldn't put it over 3 (because of A groups)
                            if (buses[bus].filled == false && !busTaken(bus) && (clusters[cl].combos[cb][clump].Count() + buses[bus].groups.Count()) <= 3)
                            {
                                //calculate timeOnThisPossibleBus, since the bus is available and doesn't have 3 A groups
                                switch (buses[bus].groups.Count())
                                {
                                    case 1:     //already one A group. So time would be with that A group and either 1 or 2 groups in the clump (if-else)
                                        if (clusters[cl].combos[cb][clump].Count() == 1)
                                        {
                                            timeOnThisPossibleBus = totalTime(buses[bus].groups[0], clusters[cl].combos[cb][clump][0]);
                                        }
                                        //should be if there are two groups in clump
                                        else
                                        {
                                            timeOnThisPossibleBus = totalTime(buses[bus].groups[0], clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1]);
                                        }
                                        break;
                                    case 2:     //already two A groups. So time would be with those two A groups and the one group in the clump
                                        timeOnThisPossibleBus = totalTime(buses[bus].groups[0], buses[bus].groups[1], clusters[cl].combos[cb][clump][0]);
                                        break;
                                    //won't be a 3 case because we would have failed the if condition earlier that the clump would make the bus <= 3 groups
                                    default:    //zero A groups on bus, keep timeOnThisPossibleBus at timeTaken (which is just for the clump)
                                        switch (clusters[cl].combos[cb][clump].Count())
                                        {
                                            case 1:
                                                timeOnThisPossibleBus = totalTime(clusters[cl].combos[cb][clump][0]);
                                                break;
                                            case 2:
                                                timeOnThisPossibleBus = totalTime(clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1]);
                                                break;
                                            case 3:
                                                timeOnThisPossibleBus = totalTime(clusters[cl].combos[cb][clump][0], clusters[cl].combos[cb][clump][1], clusters[cl].combos[cb][clump][2]);
                                                break;
                                            default:    //will this ever happen?
                                                //Console.WriteLine("somehow a clump had <1 or >3 groups");
                                                break;
                                        }
                                        break;
                                }

                                //if the clump will fit on the bus AND be within the time limit, since the bus may have A groups on it
                                //Console.Write("bus: " + bus + " timeOnThisPossibleBus " + timeOnThisPossibleBus + " clumpSum: " + clumpSum + " seatsRemaining: " + buses[bus].seatsRemaining(potentialBuses, clusters, bus, cl, cb));
                                if (clumpSum <= buses[bus].seatsRemaining(potentialBuses, clusters, bus, cl, cb) && timeOnThisPossibleBus <= 40)
                                {
                                    //Console.Write(" success\n");
                                    for (int g = 0; g < clusters[cl].combos[cb][clump].Count(); g++)
                                    {
                                        //use potential buses
                                        potentialBuses[clump][g] = bus;     //assign the bus to the group in potentialBuses. Not actually added to Bus.groups yet
                                    }
                                    break;
                                }
                                else
                                {
                                    //Console.Write(" failure\n");
                                }
                            }
                        }
                    }
                    //if all the clumps in the combo have been assigned a bus
                    if (busTaken(-1) == false)
                    {
                        //add the groups to buses using potentialBuses. Set Bus.filled to true
                        for (int clump = 0; clump < potentialBuses.Count(); clump++)
                        {
                            for (int g = 0; g < potentialBuses[clump].Count(); g++)
                            {
                                buses[potentialBuses[clump][g]].groups.Add(clusters[cl].combos[cb][clump][g]);
                                buses[potentialBuses[clump][g]].filled = true;
                            }
                        }
                        break;  //we found a solution for this combo so go to the next cluster
                    }
                    //else try the next combo
                    else
                    {
                        potentialBuses.Clear();

                    }
                }
            }
        }

        public static void putAccessibleGroupsOnBuses()
        {
            double travelTime = 100;
            //double[] tTime = new double[aGroups.Count()]; NECESSARY?
            //Group[] aGroups = groups.Where<Group>(g => g.access == true).ToArray();

            aGroups = aGroups.OrderByDescending<Group, int>(g => g.numStudents).ToArray();
            if (aGroups.Count() > 0)
            {
                List<Bus> aBuses = buses.Where<Bus>(b => b.access == true).ToList();

                if (aBuses.Count() == 0)
                {
                    //VERY BAD. THERE ARE NO ACCESSIBLE BUSES FOR THE ACCESSIBLE GROUPS
                    validSolution = false;
                    return;
                    invalidSolString = "No accessible buses to accommodate groups with accessibility needs";
                }

                //we want to use the biggest aBus first since that will make it more likely that we put another group on it later
                aBuses.Sort();
                aBuses.Reverse();

                int[] assignments = new int[aGroups.Count()];   //will hold the bus in aBuses that an aGroup has been assigned to
                for (int i = 0; i < assignments.Count(); i++) { assignments[i] = -1; }

                //put biggest aGroup on a bus first

                //putting biggest group on first (biggest) bus
                //if it fits on the bus
                if (aGroups[0].numStudents <= aBuses[0].seatsRemaining(potentialBuses, clusters, -1, -1, -1))
                {
                    aBuses[0].groups.Add(aGroups[0]);      //keeps bus.filled at false because we need to be able to consider it later
                    assignments[0] = 0;
                }
                else
                {
                    //this is bad. because we aren't able to fit the smallest aGroup on the biggest aBus. Ultimately the program should just not assign the aGroups to buses so validSolution will be false

                }

                //for(int i = 0; i < tTime.Count(); i++) { tTime[i] = 100; } NECESSARY?

                //do our best to fill each bus. first time thru this will put smallest group on first (biggest) bus
                List<Group> groupsInLimit = new List<Group>();
                for (int bus = 0; bus < aBuses.Count(); bus++)
                {
                    if (aBuses[bus].groups.Count() == 3)
                    {
                        continue;
                    }
                    //find out which buses will be in the time limit if added to this bus
                    for (int g = 0; g < aGroups.Count(); g++)
                    {
                        //if the group doesn't have a bus yet
                        if (assignments[g] == -1)
                        {
                            //calculate the travel time for if this group was added to this bus. can be 0 or 1 or 2 groups on it already
                            switch (aBuses[bus].groups.Count())
                            {
                                case 1:
                                    travelTime = totalTime(aGroups[g], aBuses[bus].groups[0]);
                                    break;
                                case 2:
                                    travelTime = totalTime(aGroups[g], aBuses[bus].groups[0], aBuses[bus].groups[1]);
                                    break;
                                default:    //aka case 0;
                                    travelTime = totalTime(aGroups[g]);
                                    break;
                            }
                            //if fits time limit AND would fit on the bus
                            if (travelTime <= 40 && aGroups[g].numStudents <= aBuses[bus].seatsRemaining(potentialBuses, clusters, -1, -1, -1))
                            {
                                //add to list of buses that fit time limit
                                groupsInLimit.Add(aGroups[g]);
                            }
                        }
                    }
                    if (groupsInLimit.Count() == 0)
                    {
                        continue; //to next bus since none of the remaining groups will fit
                    }
                    //now we can use groupsInLimit. Find the smallest group in this time and add that to the bus
                    //put the smallest remaining group in groupsInLimit on the bus

                    //since we know the smallestAGroup in groupsInLimit will fit fine, add it to the Bus.
                    aBuses[bus].groups.Add(groupsInLimit[0]);      //keeps bus.filled at false because we need to be able to consider it later
                    for (int gr = 0; gr < aGroups.Count(); gr++)
                    {
                        if (aGroups[gr] == groupsInLimit[0])
                        {
                            assignments[gr] = bus;
                            groupsInLimit.Clear();
                            break;
                        }
                    }
                }
            }
        }

        //has a group been assigned to a bus in potentialBuses?
        public static bool busTaken(int element)
        {
            for (int clump = 0; clump < potentialBuses.Count(); clump++)
            {
                for (int groop = 0; groop < potentialBuses[clump].Count(); groop++)
                {
                    if (potentialBuses[clump][groop] == element)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void initializeClusters()
        {
            for (int i = 0; i < 7; i++)
            {
                clusters.Add(new Cluster());
            }
        }

        //incomplete for now
        public static List<Bus> runAlgorithm(Bus[] b, Group[] g, Group[] accessg, Location origin)
        {
            Accord.Math.Random.Generator.Seed = 0;

            kmeansArr = new int[g.Count()];
            groups = new Group[g.Count()];
            aGroups = new Group[accessg.Count()];
            buses = new List<Bus>(b.Count());
            locations = new List<Location>();

            initializeClusters();
            groups = g;
            aGroups = accessg;
            buses = b.ToList();
            locations.Add(origin);

            runKMeans(ref groups);
            kMeansToClusters();
            findLargeClusters();
            foreach (Cluster c in clusters)
            {
                c.createCombinations();
            }
            fillBuses();
            moveOnes();

            int groupCounter = 0;
            for (int bus = 0; bus < buses.Count(); bus++)
            {
                groupCounter += buses[bus].groups.Count();
            }
            if (groupCounter < groups.Count() + aGroups.Count())
            {
                validSolution = false;
            }

            if (validSolution)
            {
                return buses;
            }
            else
            {
                List<Bus> emptyList = new List<Bus>();
                return emptyList;
            }
        }
    }
}
