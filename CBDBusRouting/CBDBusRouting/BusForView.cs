using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDBusRouting
{
    public class BusForView
    {
        public int capacity;
        public int quantity;
        public bool access;

        public BusForView(int c, int q, bool a)
        {
            capacity = c;
            quantity = q;
            access = a;
        }

        public override string ToString()
        {
            string result = quantity + " buses of size " + capacity;
            if (access)
            {
                result += ", Accessible";
            }
            return result;
        }
    }
}
