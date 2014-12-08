using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Nodes;

namespace BusinessLogic.Utils
{
    public class Utils
    {

        public static int HoursInYear = 8766;
        //public static int HoursInYear = 8760;

        public static EdgeSet StraightLine(List<CountryNode> nodes)
        {
            var edges = new EdgeSet(nodes.Count);
            // For now, connect the nodes in a straight line.
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                edges.Connect(i, i + 1);
                edges.Connect(i+1, i);
            }
            return edges;
        }

        public static double FindBeta(double K, double delta)
        {
            var beta = 0.0;
            while (true)
            {
                var genes = new NodeGenes(0.8, 1, beta);
                var gammas = genes.Select(item => item.Value.Gamma).ToArray();
                if (gammas.Min() < 1 / K) break;
                if (gammas.Max() > K) break;
                beta += delta;
            }
            return beta - delta;
        }

    }
}
