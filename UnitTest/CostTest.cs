using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost;
using NUnit.Framework;

namespace UnitTest
{
     [TestFixture]
    class CostTest
    {

         [Test]
         public void NoExportTest()
         {
             // Assign.
             var watch = new Stopwatch();
             watch.Start();
             var costCalc = new CostCalculator();
             var genes = new Chromosome(30, 0.5, 1);
             var setup = watch.ElapsedMilliseconds;
             watch.Restart();
             // Act.
             var cost = costCalc.DetailedSystemCostWithoutLinks(genes);
             // Assert.
             var eval = watch.ElapsedMilliseconds;
             //Assert.AreEqual(0,cost);
         }

    }
}
    