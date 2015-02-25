using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationTest
{
    /// <summary>
    /// Source = http://www.geocities.ws/eadorio/mvf.pdf & http://arxiv.org/pdf/1003.1594.pdf
    /// </summary>
    class FunctionFactory
    {

        public static FunctionDefinition Shubert()
        {
            return new FunctionDefinition
            {
                Name = "Shubert",
                Dimensionality = 2,
                Evaluate = tuple =>
                {
                    var x = tuple.GetValue(0);
                    var y = tuple.GetValue(1);
                    var sum1 = 0.0;
                    var sum2 = 0.0;
                    for (int i = 0; i < 5; i++)
                    {
                        sum1 += (i + 1)*Math.Cos((i + 2)*x + (i + 1));
                        sum2 += (i + 1)*Math.Cos((i + 2)*y + (i + 1));
                    }
                    var value = sum1 + sum2;
                    return value;
                },
                Min = new[] { -10.0, -10.0 },
                Max = new[] { +10.0, +10.0 },
                Optima = new[] { -186.7309 }
            };
        }

        public static FunctionDefinition Easom()
        {
            return new FunctionDefinition
            {
                Name = "Easom",
                Dimensionality = 2,
                Evaluate = tuple =>
                {
                    var x = tuple.GetValue(0);
                    var y = tuple.GetValue(1);
                    var value = -Math.Cos(x) * Math.Cos(y) *
                                Math.Exp(-(x - Math.PI) * (x - Math.PI) - (y - Math.PI) * (y - Math.PI));
                    return value;
                },
                Min = new double[] { -100, -100 },
                Max = new double[] { 100, 100 },
                Optima = new[] { -1.0 }
            };
        }

        public static FunctionDefinition Michaelwicz(int d = 2)
        {
            double optimum;
            switch (d)
            {
                case 2:
                    optimum = -1.8013;
                    break;
                case 5:
                    optimum = -4.687658;
                    break;
                case 10:
                    optimum = -9.66015;
                    break;
                default:
                    throw new ArgumentException("Optimum unknown.");
            }
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = 0;
                max[i] = 5;
            }
            return new FunctionDefinition
            {
                Name = "Michaelwicz",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var m = 10;
                    var value = 0.0;
                    for (int i = 0; i < d; i++)
                    {
                        var xi = tuple.GetValue(i);
                        value -= Math.Sin(xi)*Math.Pow(Math.Sin((i + 1)*xi*xi/Math.PI), 2*m);
                    }
                    return value;
                },
                Min = min,
                Max = max,
                Optima = new[] {optimum}
            };
        }

        public static FunctionDefinition JongsFirst(int d = 2)
        {
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = -5.12;
                max[i] = +5.12;
            }
            return new FunctionDefinition
            {
                Name = "Jongs First",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var sum = 0.0;
                    for (int i = 0; i < tuple.Dimension; i++)
                    {
                        sum += tuple.GetValue(i) * tuple.GetValue(i);
                    }
                    return sum;
                },
                Min = min,
                Max = max,
                Optima = new[] { 0.0 }
            };
        }

        public static FunctionDefinition Griewank(int d = 2)
        {
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = -600;
                max[i] = +600;
            }
            return new FunctionDefinition
            {
                Name = "Griewank",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var sum = 0.0;
                    var prod = 1.0;
                    for (int i = 0; i < d; i++)
                    {
                        sum += tuple.GetValue(i) * tuple.GetValue(i);
                        prod *= Math.Cos(tuple.GetValue(i) / Math.Sqrt(i + 1.0));
                    }
                    var value = 1.0 / 4000.0 * sum - prod + 1.0;
                    return value;
                },
                Min = min,
                Max = max,
                Optima = new[] { 0.0 }
            };
        }

        public static FunctionDefinition Ackley(int d = 2)
        {
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = -32.768;
                max[i] = +32.768;
            }
            return new FunctionDefinition
            {
                Name = "Ackley",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var sum1 = 0.0;
                    var sum2 = 0.0;
                    for (int i = 0; i < d; i++)
                    {
                        sum1 += tuple.GetValue(i) * tuple.GetValue(i);
                        sum2 += Math.Cos(2 * Math.PI * tuple.GetValue(i));
                    }
                    var term1 = -20 * Math.Exp(-0.2 * Math.Sqrt(sum1 / d));
                    var value = term1 - Math.Exp(sum2 / d) + (20 + Math.E);
                    return value;
                },
                Min = min,
                Max = max,
                Optima = new[] { 0.0 }
            };
        }

        public static FunctionDefinition Rosenbrock(int d = 2)
        {
            // Note: These limits are a guess!
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = -3;
                max[i] = +3;
            }
            return new FunctionDefinition
            {
                Name = "Rosenbrock",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var value = 0.0;
                    for (int i = 0; i < d - 1; i++)
                    {
                        var xi = tuple.GetValue(i);
                        value += Math.Pow(1 - xi, 2);
                        value += 100 * Math.Pow(tuple.GetValue(i + 1) - xi * xi, 2);
                    }
                    return value;
                },
                Min = min,
                Max = max,
                Optima = new[] { 0.0 }
            };
        }

        public static FunctionDefinition Schwefel(int d = 2)
        {
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = -500;
                max[i] = +500;
            }
            return new FunctionDefinition
            {
                Name = "Schwefel",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var sum = 0.0;
                    for (int i = 0; i < d; i++)
                    {
                        var xi = tuple.GetValue(i);
                        sum += xi * Math.Sin(Math.Sqrt(Math.Abs(xi)));
                    }
                    var value = 418.9829 * d - sum;
                    return value;
                },
                Min = min,
                Max = max,
                Optima = new[] { 0.0 }
            };
        }

        public static FunctionDefinition Rastrigin(int d = 2)
        {
            var min = new double[d];
            var max = new double[d];
            for (int i = 0; i < d; i++)
            {
                min[i] = -5.12;
                max[i] = +5.12;
            }
            return new FunctionDefinition
            {
                Name = "Rastrigin",
                Dimensionality = d,
                Evaluate = tuple =>
                {
                    var sum = 0.0;
                    for (int i = 0; i < d; i++)
                    {
                        var xi = tuple.GetValue(i);
                        sum += xi * xi - 10 * Math.Cos(2 * Math.PI * xi);
                    }
                    var value = 10 * d + sum;
                    return value;
                },
                Min = min,
                Max = max,
                Optima = new[] { 0.0 }
            };
        }

    }

}
