using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost;
using NUnit.Framework;

namespace UnitTest
{
    public class GenePoolTest
    {

        [Test]
        public void SpawnNormalizationTest()
        {
            // Arrange.
            var n = 1000;
            var genes = new NodeChromosome[n];
            // Act.
            for (int i = 0; i < n; i++) genes[i] = GenePool.SpawnChromosome();
            var alphas = genes.SelectMany(item => item.Genes.Select(pair => pair.Value.Alpha)).ToArray();
            var gammas = genes.SelectMany(item => item.Genes.Select(pair => pair.Value.Gamma)).ToArray();
            var delta = 1e-3;
            // Assert.
            Assert.That(alphas.Min() >= GenePool.AlphaMin - delta);
            Assert.That(alphas.Max() <= GenePool.AlphaMax + delta);
            Assert.That(gammas.Min() >= GenePool.GammaMin - delta);
            Assert.That(gammas.Max() <= GenePool.GammaMax + delta);
        }

        [Test]
        public void MutationNormalizationTest()
        {
            // Arrange.
            var n = 100;
            var genes = new NodeChromosome[n];
            // Act.
            for (int i = 0; i < n; i++)
            {
                genes[i] = GenePool.SpawnChromosome();
                foreach (var key in genes[i].Genes.Keys)
                {
                    // Perform 10 mutations.
                    for (int j = 0; j < 10; j++)
                    {
                        GenePool.TryMutate(genes[i], key);                        
                    }
                }
            }
            var alphas = genes.SelectMany(item => item.Genes.Select(pair => pair.Value.Alpha)).ToArray();
            var gammas = genes.SelectMany(item => item.Genes.Select(pair => pair.Value.Gamma)).ToArray();
            var delta = 1e-3;
            // Assert.
            Assert.That(alphas.Min() >= GenePool.AlphaMin - delta);
            Assert.That(alphas.Max() <= GenePool.AlphaMax + delta);
            Assert.That(gammas.Min() >= GenePool.GammaMin - delta);
            Assert.That(gammas.Max() <= GenePool.GammaMax + delta);
        }
    }
}
