using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Optimization;
using Optimization.OldOptimization;
using Utils;

namespace OptimizationTest
{

    internal class Program
    {

        private static void Main(string[] args)
        {
            ////var func = FunctionFactory.Shubert();
            ////var tuple = new Tuple(2);
            ////tuple.SetValue(0, 5.791794);
            ////tuple.SetValue(1, 5.791794);
            ////var value = func.Evaluate(tuple);
            ////Console.WriteLine("The value is {0}", value);
            ////Console.ReadLine();

            //var n = 100; // For asym search, a value of \approx 20 is best.
            //var m = 10;
            //var functionList = new[]
            //{
            //    //FunctionFactory.Easom(),
            //    //FunctionFactory.Shubert(),
            //    FunctionFactory.JongsFirst(30),
            //    //FunctionFactory.Michaelwicz(10),
            //    FunctionFactory.Ackley(30),
            //    //FunctionFactory.Rosenbrock(10),
            //    //FunctionFactory.Schwefel(),
            //    //FunctionFactory.Rastrigin(),
            //    //FunctionFactory.Griewank(),
            //};

            //var total = 0.0;
            //foreach (var def in functionList)
            //{
            //    var evals = TestFunction(def, n, m);
            //    var success = evals.Where(item => item < 1000000).Count();
            //    Console.WriteLine("The success rate was {0}% for {1}.", success/((double) m)*100, def.Name);
            //    var avg = 0.0;
            //    if (success > 0)
            //    {
            //        avg = evals.Where(item => item < 1000000).Average();
            //        Console.WriteLine("Optimization took {0} function evaluations on average.",
            //            evals.Where(item => item < 1000000).Average());
            //    }
            //    total += avg;

            //}
            //Console.WriteLine();
            //Console.WriteLine("Total number of evaluations = {0}", total);

            //var old = FileUtils.FromJsonFile<Dictionary<string, List<Wrapper>>>(@"C:\proto\highDimTest.txt");
            //var upd = new Dictionary<string, Dictionary<string,double[]>>();

            //foreach (var key in old.Keys)
            //{
            //    var dim = new double[30];
            //    var succes = new double[30];
            //    var evalAvg = new double[30];
            //    var evalStd = new double[30];
            //    var idx =0;
            //    foreach (var wrapper in old[key])
            //    {
            //        dim[idx] = wrapper.Dimension;
            //        succes[idx] = wrapper.SuccessRate;
            //        evalAvg[idx] = wrapper.EvaluationAvg;
            //        evalStd[idx] = wrapper.EvaluationStd;
            //        idx++;
            //    }
            //    upd.Add(key, new Dictionary<string, double[]>()
            //    {
            //        {"Dim", dim},
            //        {"Succes", succes},
            //        {"EvalAvg", evalAvg},
            //        {"EvalStd", evalStd}
            //    });
            //}

            //upd.ToJsonFile(@"C:\proto\highDimTest2.txt");

            DoStuff();

            Console.ReadLine();
        }


        private static void DoStuff()
        {
            var m = 100;
            var defs = new Dictionary<string, Func<FunctionDefinition, int, double[]>>
            {
                {"MCS", TestFunctionModified},
                {"CS", TestFunction}
            };

            var result = new Dictionary<string, Dictionary<string, double[]>>();
            foreach (var def in defs)
            {
                var dim = new double[30];
                var succes = new double[30];
                var evalAvg = new double[30];
                var evalStd = new double[30];

                for (int i = 1; i < 31; i++)
                {
                    var evals = def.Value(FunctionFactory.Ackley(i), m);
                    var success = evals.Where(item => item < 1000000).Count();
                    dim[i-1] = i;
                    succes[i-1] = success / ((double)m) * 100;
                    if (success > 0)
                    {
                        evalAvg[i-1] = evals.Where(item => item < 1000000).Average();
                        evalStd[i-1] = evals.Where(item => item < 1000000).StdDev(item => item);
                    }
                    Console.WriteLine("{0} passed dimension {1}", def.Key, i);
                }

                result.Add(def.Key, new Dictionary<string, double[]>
                {
                    {"Dim", dim},
                    {"Succes", succes},
                    {"EvalAvg", evalAvg},
                    {"EvalStd", evalStd}
                });
            }

            result.ToJsonFile(@"C:\proto\highDimTest.txt");
        }

        public class Wrapper
        {
            public int Dimension { get; set; }
            public double SuccessRate { get; set; }
            public double EvaluationAvg { get; set; }
            public double EvaluationStd { get; set; }
        }

        private static double[] TestFunction(FunctionDefinition func, int m)
        {
            var n = 25; 
            var rnd = new Random();
            var calc = new CukooFunctionCostCalculator {Function = func.Evaluate};
            var strat = new CukooFunctionOptimizationStrategy
            {
                Min = func.Min,
                Max = func.Max,
                Optima = func.Optima,
                LevyFunc = Rnd => Rnd.NextLevy(1.5, 0)
            };
            var optimizer = new OriginalCukooOptimizer<Tuple>(strat, calc)
            {
                CacheOnDisk = false,
                PrintToConsole = false
            };
            var evals = new double[m];
            for (int i = 0; i < m; i++)
            {
                evals[i] = double.MaxValue;
                var optimum = optimizer.Optimize(RandomNumbers(n, func.Dimensionality, rnd, func.Min, func.Max));
                // Check is the optimum is correct.
                for (int j = 0; j < func.Optima.Length; j++)
                {
                    var delta = Math.Abs(optimum.Cost - func.Optima[j]);
                    if (!(delta < 1e-4)) continue;
                    // Correct optimum! Write the iteration number.
                    evals[i] = calc.Evaluations;
                }
                calc.Reset();
                strat.Reset();
            }

            return evals;
        }

        private static double[] TestFunctionModified(FunctionDefinition func, int m)
        {
            var n = 100; 
            var rnd = new Random();
            var calc = new CukooFunctionCostCalculator { Function = func.Evaluate };
            var strat = new CukooFunctionOptimizationStrategy
            {
                Min = func.Min,
                Max = func.Max,
                Optima = func.Optima,
                LevyFunc = Rnd => Rnd.NextLevy(0.5, 1)
            };
            var optimizer = new ModifiedCukooOptimizer<Tuple>(strat, calc)
            {
                CacheOnDisk = false,
                PrintToConsole = false
            };
            var evals = new double[m];
            for (int i = 0; i < m; i++)
            {
                evals[i] = double.MaxValue;
                var optimum = optimizer.Optimize(RandomNumbers(n, func.Dimensionality, rnd, func.Min, func.Max));
                // Check is the optimum is correct.
                for (int j = 0; j < func.Optima.Length; j++)
                {
                    var delta = Math.Abs(optimum.Cost - func.Optima[j]);
                    if (!(delta < 1e-4)) continue;
                    // Correct optimum! Write the iteration number.
                    evals[i] = calc.Evaluations;
                }
                calc.Reset();
                strat.Reset();
            }

            return evals;
        }


        private static Tuple[] RandomNumbers(int n, int d, Random rnd, double[] min, double[] max)
        {
            var solutions = new Tuple[n];
            for (int i = 0; i < n; i++)
            {
                solutions[i] = new Tuple(d);
                for (int j = 0; j < d; j++)
                {
                    solutions[i].SetValue(j, rnd.NextDouble()*((max[j]-min[j]))+min[j]);
                }
            }
            return solutions;
        }

    }

    internal class Tuple : ISolution
    {

        private readonly double[] m_values;
        private double m_cost;
        private bool m_invalidCost;

        public int Dimension
        {
            get { return m_values.Length; }
        }

        public double GetValue(int idx)
        {
            return m_values[idx];
        }

        public void SetValue(int idx, double value)
        {
            m_values[idx] = value;
            m_invalidCost = true;
        }

        public double Cost
        {
            get
            {
                if (InvalidCost) throw new ArgumentException("Invalid cost.");
                return m_cost;
            }
        }

        public bool InvalidCost
        {
            get { return m_invalidCost; }
        }

        public void UpdateCost(Func<ISolution, double> eval)
        {
            m_cost = eval(this);
            m_invalidCost = false;
        }

        public Tuple(int dimension)
        {
            m_invalidCost = true;
            m_values = new double[dimension];
        }

    }

    internal class FunctionDefinition
    {

        public string Name { get; set; }

        public double[] Min { get; set; }
        public double[] Max { get; set; }
        public double[] Optima { get; set; }

        public int Dimensionality { get; set; }

        public Func<Tuple, double> Evaluate { get; set; }

    }

    #region Cukoo

    internal class CukooFunctionOptimizationStrategy : ICukooOptimizationStrategy<Tuple>
    {

        public Random Rnd = new Random();

        public double StepScale = 1;
        private static readonly double Phi = (1 + Math.Sqrt(5))/2.0;
        private int[] m_rndOrder1;
        private int[] m_rndOrder2;

        public double[] Min { get; set; }
        public double[] Max { get; set; }
        public double[] Optima { get; set; }

        public Func<Random,double> LevyFunc { get; set; } 

        public bool TerminationCondition(Tuple[] nests, int evaluations)
        {
            // Update the random ordering on 
            m_rndOrder1 = new int[nests.Length].Linspace().Shuffle(Rnd).ToArray();
            m_rndOrder2 = new int[nests.Length].Linspace().Shuffle(Rnd).ToArray();

            // Check convergence.
            if (evaluations > 1000000) return true;
            return Math.Abs(nests[0].Cost - Optima[0]) < 5*1e-5;

            //if (Math.Abs(m_lastCost - nests[0].Cost) < 1e-5) m_stagnationCount++;
            //if (m_stagnationCount >= m_stagnationLimit) return true;
            //// Update cost.
            //m_lastCost = nests[0].Cost;
            //return false; //(evaluations > 25000);
        }

        public Tuple LevyFlight(Tuple nest, Tuple bestNest)
        {
            var levyStep = LevyFunc(Rnd);
            var result = new Tuple(nest.Dimension);
            for (int i = 0; i < result.Dimension; i++)
            {
                // Do levy flight.
                var value = nest.GetValue(i) + levyStep * StepScale * Rnd.NextDouble() * (bestNest.GetValue(i) - nest.GetValue(i));
                // Enforce boundaries.
                if (value < Min[i]) value = Min[i];
                if (value > Max[i]) value = Max[i];
                result.SetValue(i, value);
            }

            return result;
        }

        public Tuple CrossOver(Tuple badNest, Tuple goodNest)
        {
            var result = new Tuple(badNest.Dimension);
            for (int i = 0; i < result.Dimension; i++)
            {
                result.SetValue(i, goodNest.GetValue(i) + (badNest.GetValue(i) - goodNest.GetValue(i)) / Phi);
            }
            return result;
        }

        public Tuple DifferentialEvolution(Tuple[] nests, int i)
        {
            // Do nest abandoning step (DE inspired).
            var result = new Tuple(nests[i].Dimension);
            var nest1 = nests[m_rndOrder1[i]];
            var nest2 = nests[m_rndOrder2[i]];
            for (int j = 0; j < result.Dimension; j++)
            {
                // Do the DE step.
                var value = nests[i].GetValue(j) + (nest1.GetValue(j) - nest2.GetValue(j))*Rnd.NextDouble();
                // Enforce boundaries.
                if (value < Min[j]) value = Min[j];
                if (value > Max[j]) value = Max[j];
                result.SetValue(j, value);
            }

            return result;
        }

        public void Reset()
        {
        }

    }

    internal class CukooFunctionCostCalculator : ICostCalculator<Tuple>
    {

        public Func<Tuple, double> Function { get; set; }

        private int m_evaluations;

        public int Evaluations
        {
            get { return m_evaluations; }
        }

        public void UpdateCost(IList<Tuple> solutions)
        {
            m_evaluations += solutions.Count(item => item.InvalidCost);
            foreach (var solution in solutions) solution.UpdateCost(item => Function((Tuple) item));
        }

        public void Reset()
        {
            m_evaluations = 0;
        }

    }

    #endregion

}
