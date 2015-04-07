using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Nodes;
using Utils;
using Utils.Statistics;

namespace BusinessLogic.Utils
{
    public class Stuff
    {

        public static int HoursInYear = 8766;
        //public static int HoursInYear = 8760;

        public static EdgeCollection StraightLine(CountryNode[] nodes)
        {
            var builder = new EdgeBuilder(nodes.Select(item => item.Name).ToArray());
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                builder.Connect(i, i + 1);
            }
            return builder.ToEdges();
        }

        // EMHER: The value 0.95 is to "optimal mix".
        public static double FindBeta(double K, double delta, double alpha = 0.95)
        {
            var beta = 0.0;
            while (true)
            {
                var genes = NodeGenesFactory.SpawnBeta(alpha, 1, beta);
                var gammas = genes.Select(item => item.Value.Gamma).ToArray();
                if (gammas.Min() < 1/K)
                {
                    break;
                }
                if (gammas.Max() > K)
                {
                    break;
                }
                beta += delta;
            }
            return Math.Max(0,beta-delta);
        }

        // Delta weights - so far ONLY using one year
        public static double[] DeltaWeights(CountryNode[] nodes, NodeGenes genes)
        {
            var weights = new double[nodes.Length];

            for (int j = 0; j < nodes.Length; j++)
            {
                var node = nodes[j];
                node.Model.Gamma = genes[node.Name].Gamma;
                node.Model.Alpha = genes[node.Name].Alpha;
                node.Model.OffshoreFraction = genes[node.Name].OffshoreFraction;
                var backup = new double[HoursInYear];
                for (int i = 0; i < HoursInYear; i++)
                {
                    node.TickChanged(i);
                    backup[i] = node.GetDelta();
                }
                weights[j] = Math.Abs(MathUtils.Percentile(backup,1));
            }
            weights.Norm();

            return weights;
        }

    }
}
