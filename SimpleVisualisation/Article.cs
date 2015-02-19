using System.Windows.Input;
using BusinessLogic.Cost;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using Utils;

namespace Main
{
    public class Article
    {

        public static void DoOptimizations()
        {
            // Default optimizations.
            for (var k = 2; k < 4; k++)
            {
                Optimization.Cukoo(k, 500, "default", new NodeChromosome(NodeGenesFactory.SpawnCfMax(1, 1, k)));
            }

            // Offshore fraction optimizations.
            for (var k = 2; k < 4; k++)
            {
                GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.25);
                Optimization.Cukoo(k, 500, "offshore25pct", new NodeChromosome(NodeGenesFactory.SpawnCfMax(1, 1, k)));
                GenePool.OffshoreFractions = CountryInfo.OffshoreFrations(0.50);
                Optimization.Cukoo(k, 500, "offshore50pct", new NodeChromosome(NodeGenesFactory.SpawnCfMax(1, 1, k)));
                GenePool.OffshoreFractions = null;
            }

            // Reduced solar cost optimizations
            for (var k = 2; k < 4; k++)
            {
                Optimization.Cukoo(k, 500, "solar25pct", new NodeChromosome(NodeGenesFactory.SpawnCfMax(1, 1, k)),
                    new ParallelNodeCostCalculator {SolarCostModel = new ScaledSolarCostModel(0.25)});
                Optimization.Cukoo(k, 500, "solar50pct", new NodeChromosome(NodeGenesFactory.SpawnCfMax(1, 1, k)),
                    new ParallelNodeCostCalculator {SolarCostModel = new ScaledSolarCostModel(0.50)});
            }
        }

        public void ExportFigureData()
        {

        }

        public void ExportData()
        {
            
        }

    }
}