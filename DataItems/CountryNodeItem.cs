using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace DataItems
{

    [ProtoContract]
    public class CountryNodeItem
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public string Abbrevation { get; set; }

        [ProtoMember(3)]
        public List<double> TimeSeries { get; set; }

        public CountryNodeItem()
        {
            // For protobuf.            
        }
    }
}
