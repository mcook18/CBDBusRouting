using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    class DataClassesView
    {
        class Group
        {
            public Location destination;
            public int numStudents;

            public Group(Location dest, int n)
            {
                destination = dest;
                numStudents = n;
            }
        }

        class Location
        {
            //might not be exhaustive
            public string address;

            public Location(string addr)
            {
                address = addr;
            }
        }

        class Bus
        {
            public int numSeats;
            public List<Group> groups;

            public Bus(int n)
            {
                numSeats = n;
                groups = new List<Group>();
            }

            public Bus(int n, List<Group> g)
            {
                numSeats = n;
                groups = g;
            }
        }
    }

    
}
