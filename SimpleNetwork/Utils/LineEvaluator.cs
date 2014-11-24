using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Nodes;

namespace BusinessLogic.Utils
{
    public class LineEvaluator
    {

        #region Specific functionality

        public static bool[] EvalSimulation(LineScanParameters gridParams, Simulation simulation, int years = 1)
        {
            var watch = new Stopwatch();
            // Eval grid.
            return EvalDense(delegate(int idx)
            {
                var pen = gridParams.PenetrationFrom + gridParams.PenetrationStep * idx;
                foreach (var node in simulation.Model.Nodes)
                {
                    ((CountryNode) node).Model.Gamma = pen;
                }
                // Do simulation.
                watch.Restart();
                simulation.Simulate(8765 * years, LogLevelEnum.None);
                Console.WriteLine("Penetation " + pen + ": " +
                                  watch.ElapsedMilliseconds + ", " + (simulation.Output.Success ? "SUCCESS" : "FAIL"));
                return simulation.Output.Success;
            }, gridParams.PenetrationSteps);
        }

        #endregion

        #region Genetic functionality

        /// <summary>
        /// Evaluated all points on the line.
        /// </summary>
        /// <param name="func"> function to evaluate </param>
        /// <param name="dim"> line dimension </param>
        /// <returns> line values </returns>
        public static T[] EvalDense<T>(Func<int, T> func, int dim)
        {
            var line = new T[dim];
            int temp;

            for (temp = 0; temp < line.Length; temp++) line[temp] = func(temp);

            return line;
        }

        #endregion

    }

    public class LineScanParameters
    {

        public double[] Cols
        {
            get
            {
                var result = new double[PenetrationSteps];
                for (int i = 0; i < PenetrationSteps; i++)
                {
                    result[i] = PenetrationFrom + PenetrationStep * i;
                }
                return result;
            }
        }

        public double PenetrationTo { get; set; }
        public double PenetrationFrom { get; set; }
        public int PenetrationSteps { get; set; }
        public double PenetrationStep
        {
            get { return (PenetrationTo - PenetrationFrom) / PenetrationSteps; }
        }

    }
}
