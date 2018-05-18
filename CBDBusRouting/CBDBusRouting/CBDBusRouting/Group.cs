using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public class Group
    {
        //NEEDS TO INCLUDE THE OTHER PROPERTIES THAT WILL BE USED IN THE VIEW. JUST INITIALIZE AND LEAVE THEM BE
        public string name;
        public string leader;
        public Location destination;
        public int cluster;             //initial cluster that is assigned. If that cluster is split then it gets the value of the new cluster
        public int numStudents;
        public bool access;
        public bool runFlag;

        public Group(Location dest, int c, int num, bool a)
        {
            destination = dest;
            cluster = c;
            numStudents = num;
            access = a;
            runFlag = true;
        }

        public Group(string name, string leader, Location dest, int c, int num, bool a, bool toBeRun)
        {
            this.name = name;
            this.leader = leader;
            destination = dest;
            cluster = c;
            numStudents = num;
            access = a;
            runFlag = toBeRun;
        }

        public void flipRunFlag()
        {
            runFlag = !runFlag;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
