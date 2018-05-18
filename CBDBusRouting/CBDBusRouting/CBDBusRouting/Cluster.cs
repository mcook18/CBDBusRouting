using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public class Cluster : IComparable<Cluster>
    {
        public List<Group> members;
        public List<Group[][]> combos; //List of combinations (partitions) of the members object. Each combination is an array of pairings (think buses). Each pairing is 1-3 groups in their own array

        public Cluster()
        {
            members = new List<Group>();
            combos = new List<Group[][]>();
        }

        //call this constructor when splitting a cluster into 2 smaller ones, since you can pass in the new members
        public Cluster(List<Group> m)
        {
            members = new List<Group>();
            combos = new List<Group[][]>();
            members = m;
        }

        //So that Clusters can be sorted. It sorts them based on how many members they have since that is how we order them later
        public int CompareTo(Cluster other)
        {
            if (this.members.Count < other.members.Count)
            {
                return -1;
            }
            else if (this.members.Count == other.members.Count)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private static bool lessThanFour(Group[][] combo)
        {
            foreach (Group[] bus in combo)
            {
                if (bus.Count() >= 4)
                {
                    return false;
                }
            }
            return true;
        }

        public void createCombinations()
        {
            //call the Partitioning class.
            //Following line explanation:
            //GetAllPartitions returns an IEnumerable. Takes in a list of Groups, which is the members property of a Cluster.
            //The Where() clause makes it only return the clusters that have no pairings with more than three groups.
            IEnumerable<Group[][]> thePartitions = Partitioning.GetAllPartitions<Group>(members.ToArray()).Where(lessThanFour);    //YOOOOO THIS WORKS
            combos = thePartitions.ToList();    //since GetAllPartitions returns an array, we turn it back to a list (List<Group[][]>)
        }
    }
}
