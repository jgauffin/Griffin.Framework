using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTest
{
    public class Ping
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return "Ping from " + Name;
        }
    }
}
