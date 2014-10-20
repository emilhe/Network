using System;
using System.Diagnostics;
using Utils;

namespace BusinessLogic.Utils
{
    public class GridEvaluator
    {

        #region Genetic functionality

        /// <summary>
        /// Evaluated all points on the grid. Expensive, but solid.
        /// </summary>
        /// <param name="func"> function to evaluate </param>
        /// <param name="dims"> grid dimensions </param>
        /// <returns> grid values </returns>
        public static T[,] EvalDense<T>(Func<int[], T> func, int[] dims)
        {
            var grid = new T[dims[0], dims[1]]; 

            grid.MultiLoop(indices => grid[indices[0], indices[1]] = func(indices));

            return grid;
        }

        /// <summary>
        /// Assumes two regions are present; by strategic evaluation the two regions
        /// are identified with less evaluations than required by a full scan.
        /// </summary>
        /// <param name="func"> function to evaluate </param>
        /// <param name="dims"> grid dimensions </param>
        /// <returns> grid values </returns>
        public static bool[,] EvalSparse(Func<int[], bool> func, int[] dims)
        {
            // TODO: Generic dimension (currently only 2D)            
            var grid = new bool[dims[0], dims[1]];
            var temp = new int[dims.Length];
            //var guess = dims[0]/2;
            var guess = 0;

            for (temp[1] = 0; temp[1] < dims[1]; temp[1]++)
            {
                temp[0] = guess;
                var response = func(temp);
                // Determine where the boundary is.
                if (response)
                {
                    temp[0]--;
                    while (temp[0] >= 0 && func(temp)) temp[0]--;
                    temp[0]++;
                }
                else
                {
                    temp[0]++;
                    while (temp[0] < dims[0] && !func(temp)) temp[0]++;
                }
                guess = temp[0];
                // Flip the bits on in positive side.
                while (temp[0] < dims[0])
                {
                    grid[temp[0], temp[1]] = true;
                    temp[0]++;
                }
            }

            return grid;
        }

        #endregion

    }

    public class GridScanParameters
    {
        public bool StartFromMin { get; set; }
        public double MixingFrom { get; set; }
        public double MixingTo { get; set; }
        public int MixingSteps { get; set; }
        public double MixingStep
        {
            get { return (MixingTo - MixingFrom) / MixingSteps; }
        }

        public double[] Rows
        {
            get
            {
                var result = new double[MixingSteps];
                for (int i = 0; i < MixingSteps; i++)
                {
                    result[i] = MixingFrom + MixingStep * i;
                }
                return result;
            }
        }
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
