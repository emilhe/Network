using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNetwork
{
    public class GridEvaluator
    {

        /// <summary>
        /// Evaluated all points on the grid. Expensive, but solid.
        /// </summary>
        /// <param name="func"> function to evaluate </param>
        /// <param name="dims"> grid dimensions </param>
        /// <returns> grid values </returns>
        public static T[,] EvalDense<T>(Func<int[], T> func, int[] dims)
        {
            // TODO: Generic dimension (currently only 2D)
            var grid = new T[dims[0], dims[1]]; 
            var temp = new int[dims.Length];

            for (temp[0] = 0; temp[0] < grid.GetLength(0); temp[0]++)
            {
                for (temp[1] = 0; temp[1] < grid.GetLength(1); temp[1]++)
                {
                    grid[temp[0], temp[1]] = func(temp);
                }
            }

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
            var guess = dims[0]/2;

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

    }
}
