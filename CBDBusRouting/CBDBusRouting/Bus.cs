using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public class Bus : IComparable<Bus>
    {
        public int totalSeats;
        public List<Group> groups;
        public bool filled = false;     //the bus has been given members (not in the potentialBuses List)
        public bool access;

        //Used to sort buses based on total number of seats
        public int CompareTo(Bus other)
        {
            //using totalSeats because we sort the buses when they are empty
            if (this.totalSeats < other.totalSeats)
            {
                return -1;
            }
            else if (this.totalSeats == other.totalSeats)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        //total number of seats being used by the bus. Pass busindex, clusterindex, comboindex as -1 if the potentialBuses list should not be considered
        public double seatsTaken(List<List<int>> potBus, List<Cluster> cls, int busindex, int clusterindex, int comboindex)
        {
            //all groups actually in Bus.groups
            double sum = 0;
            foreach (Group g in this.groups)
            {
                sum += g.numStudents;
            }
            //if we want to include the groups that COULD be added to the bus, if those groups' combination is found to be a legal one
            if (busindex != -1 && clusterindex != -1 && comboindex != -1)
            {
                //we could have some groups that are in potentialBus but are NOT YET in the Bus itself. We must include these too
                for (int clump = 0; clump < potBus.Count(); clump++)
                {
                    for (int group = 0; group < potBus[clump].Count(); group++)
                    {
                        if (potBus[clump][group] == busindex)
                        {
                            sum += cls[clusterindex].combos[comboindex][clump][group].numStudents;
                        }
                        else
                        {
                            //Console.WriteLine("LoopFail");
                        }
                    }
                }
            }
            return sum;
        }
        //total seats minus the seats that have been taken
        public double seatsRemaining(List<List<int>> potBus, List<Cluster> cls, int busindex, int clusterindex, int comboindex)
        {
            return totalSeats - seatsTaken(potBus, cls, busindex, clusterindex, comboindex);
        }

        //so that we can see how long the current A groups on the bus will take, as well as for output later
        public double timeTaken()
        {
            double answer;
            switch (groups.Count())
            {
                case (1):
                    answer = Algorithm.totalTime(groups[0]);
                    break;
                case (2):
                    answer = Algorithm.totalTime(groups[0], groups[1]);
                    break;
                case (3):
                    answer = Algorithm.totalTime(groups[0], groups[1], groups[2]);
                    break;
                default:
                    answer = 0;
                    break;
            }
            return answer;
        }

        public Bus(int seats, bool a)
        {
            totalSeats = seats;
            access = a;
            groups = new List<Group>();
        }
    }

}
