using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork.ExportStrategies
{
    public class SelfishExportStrategy : IExportStrategy
    {
        public void Bind(List<Node> nodes, double[] mismatches, double tolerance = 0)
        {
            throw new NotImplementedException();
        }

        public double TraverseStorageLevels(int tick)
        {
            throw new NotImplementedException();
        }
    }
}
