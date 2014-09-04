using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.TimeSeries
{
    class BasicTimeSeries
    {

        public BasicTimeSeries()
        {
            Properties = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Properties { get; set; }

        public string Name
        {
            get { return Properties["Name"]; }
            set { Properties["Name"] = value; }
        }

    }
}
