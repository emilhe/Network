using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;

namespace HomeWork
{
    class Program
    {

        private const int n = 5;

        static void Main(string[] args)
        {
            var opt = Setup();

            ExerciseI(opt);
            ExerciseIII(opt);

            Console.ReadLine();
        }

        #region Exercise 1

        private static void ExerciseI(FlowOptimizer opt)
        {
            SolveI(opt, -2);
            PrintSystemInfo(opt);
            SolveI(opt, -4);
            PrintSystemInfo(opt);
            SolveI(opt, -6);
            PrintSystemInfo(opt);
            SolveI(opt, -8);
            PrintSystemInfo(opt);
            //Solve(opt, -10);
            //PrintSystemInfo(opt);

        }

        private static void SolveI(FlowOptimizer opt, double delta1)
        {
            var nodes = new[] { delta1, 2, 1, 3, 2 };
            opt.SetNodes(nodes, new double[] { 0, 0, 0, 0, 0 }, new[] { double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue });
            opt.Solve();
        }

        #endregion

        #region Exercise 3

        private static void ExerciseIII(FlowOptimizer opt)
        {
            SolveIII(opt, new[] {-16/5.0, 4/5.0, -1/5.0, 9/5.0, 4/5.0});
            PrintSystemInfo(opt);
            SolveIII(opt, new[] {-24/5.0, 6/5.0, 1/5.0, 11/5.0, 6/5.0});
            PrintSystemInfo(opt);
            SolveIII(opt, new[] {-32/5.0, 8/5.0, 3/5.0, 13/5.0, 8/5.0});
            PrintSystemInfo(opt);
            SolveIII(opt, new double[] { -8, 2, 1, 3, 2});
            PrintSystemInfo(opt);
            SolveIII(opt, new[] {-48/5.0, 12/5.0, 7/5.0, 17/5.0, 12/5.0});
            PrintSystemInfo(opt);
        }

        private static void SolveIII(FlowOptimizer opt, double[] injection)
        {
            opt.SetNodes(injection, new[] { 0.0, 0, 0, 0, 0 }, new[] { 0.0, 0, 0, 0, 0 });
            opt.Solve();
        }

        #endregion

        private static FlowOptimizer Setup()
        {
            var opt = new FlowOptimizer(n);
            var edges = new EdgeSet(n);
            edges.Connect(0, 1);
            edges.Connect(0, 4);
            edges.Connect(1, 2);
            edges.Connect(1, 3);
            edges.Connect(2, 4);
            edges.Connect(3, 4);
            opt.SetEdges(edges);
            return opt;
        }

        private static void PrintSystemInfo(FlowOptimizer opt)
        {
            for (int i = 0; i < n; i++)
            {
                if (opt.NodeOptimum[i]< 1e-2) continue;
                Console.WriteLine("Curtailment of {0} is {1}.", i+1, opt.NodeOptimum[i]);
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (opt.Flows[i,j] < 1e-2) continue;
                    Console.WriteLine("Flow of {0} between {1} and {2}.", opt.Flows[i, j], i+1,j+1);
                }
            }

            Console.WriteLine();
        }

    }
}
