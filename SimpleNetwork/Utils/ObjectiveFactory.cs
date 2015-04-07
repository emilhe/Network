using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using Gurobi;

namespace BusinessLogic.Utils
{
    static class ObjectiveFactory
    {

        /// <summary>
        /// Flow minimization objective.
        /// </summary>
        public static GRBQuadExpr SquaredFlow(EdgeCollection edges, MyModel wrap)
        {
            var n = edges.NodeCount;
            GRBQuadExpr objective = 0.0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (!edges.Connected(i, j)) continue;
                    objective.AddTerm(edges.GetEdgeCost(i, j), wrap.Edges[i + j * n], wrap.Edges[i + j * n]);
                }
            }

            return objective;
        }

        /// <summary>
        /// Linear balancing objective.
        /// </summary>
        public static GRBLinExpr LinearBalancing(MyModel wrap)
        {
            GRBLinExpr objective = 0.0;
            //foreach (var expr in wrap.NodeExprs) objective.Add(expr.GrbLinExpr);
            foreach (var dummy in wrap.NodeExprsDummies) objective.Add(dummy);

            return objective;
        }

        /// <summary>
        /// Quadratic balancing objective.
        /// </summary>
        public static GRBQuadExpr QuadraticBalancing(MyModel wrap, double[] weights)
        {
            GRBQuadExpr objective = 0.0;
            for (int i = 0; i < wrap.NodeExprsDummies.Length; i++) objective.AddTerm(weights[i], wrap.NodeExprsDummies[i], wrap.NodeExprsDummies[i]);

            return objective;
        }

    }
}
