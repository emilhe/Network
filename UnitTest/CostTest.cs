using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost;
using NUnit.Framework;
using Optimization;
using Utils;

namespace UnitTest
{
     [TestFixture]
    class CostTest
    {

         [Test]
         public void HelloWorld()
         {
             //// ReBirth population.
             //var n = 50;
             //var strategy = new GeneticNodeOptimizationStrategy(new ParallelCostCalculator());
             //var population = new IChromosome[n];
             //for (int i = 0; i < population.Length; i++) population[i] = strategy.SpawnChromosome();
             //// Find optimum.
             //var optimizer = new GeneticOptimizer<NodeChromosome>(strategy);
             //var optimum = optimizer.Optimize(population);
             //optimum.ToJsonFile(@"C:\proto\genetic.txt");
             //Assert.AreEqual("Hello World!", optimum.ToString());
         }

    }
}
    