using System.Collections.Generic;

namespace BusinessLogic.Utils
{
    public class Utils
    {

        public static int HoursInYear = 8766;
        //public static int HoursInYear = 8760;

        public static EdgeSet StraightLine(List<Node> nodes)
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

    }
}
