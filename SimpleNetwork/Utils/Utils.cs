using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetwork.ExportStrategies;
using SimpleNetwork.ExportStrategies.DistributionStrategies;

namespace SimpleNetwork.Utils
{
    public class Utils
    {

        public static EdgeSet StraightLine(List<Node> nodes)
        {
            var edges = new EdgeSet(nodes.Count);
            // For now, connect the nodes in a straight line.
            for (int i = 0; i < nodes.Count - 1; i++) edges.AddEdge(i, i + 1);
            return edges;
        }

    }
}
