using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace BusinessLogic.Cost
{
    public static class NodeGenesFactory
    {

        public static NodeGenes SpawnBeta(double alpha, double gamma, double beta)
        {
            var genes = CountryInfo.GetCountries().ToDictionary(item => item, item => new NodeGene { Alpha = alpha, Gamma = gamma });

            // The result is NOT defined in alpha = 0.
            //if (Math.Abs(alpha) < 1e-5) alpha = 1e-5;

            var lEU = CountryInfo.GetMeanLoadSum();
            var cfW = CountryInfo.WindOnshoreCf;
            var cfS = CountryInfo.SolarCf;
            // Calculated load weighted beta-scaled cf factors.
            var wSum = 0.0;
            var sSum = 0.0;
            foreach (var i in genes.Keys)
            {
                wSum += CountryInfo.GetMeanLoad(i) * Math.Pow(cfW[i], beta);
                sSum += CountryInfo.GetMeanLoad(i) * Math.Pow(cfS[i], beta);
            }
            // Now calculate alpha i.
            foreach (var i in genes.Keys)
            {
                // EMHER: Semi certain about the gamma equaltion. 
                genes[i].Alpha = (alpha < 1e-6) ? 0 : 1 / (1 + (1 / alpha - 1) * Math.Pow(cfS[i] / cfW[i], beta) * wSum / sSum);
                // EMHER: Quite certain about the gamma equaltion. 
                genes[i].Gamma = gamma * lEU * (alpha * Math.Pow(cfW[i], beta) / wSum + (1 - alpha) * Math.Pow(cfS[i], beta) / sSum);
            }
            var result = new NodeGenes(genes);
            // Make sanity check.
            var dAlpha = alpha - result.Alpha;
            var gGamma = gamma - result.Gamma;
            if (Math.Abs(dAlpha) > 1e-6) throw new ArgumentException("Alpha value wrong");
            if (Math.Abs(gGamma) > 1e-6) throw new ArgumentException("Gamma value wrong");

            return result;
        }

        public static NodeGenes SpawnCfMax(double alpha, double gamma, double k)
        {
            var genes = CountryInfo.GetCountries().ToDictionary(item => item, item => new NodeGene { Alpha = alpha, Gamma = gamma });
            var loadSum = genes.Select(item => CountryInfo.GetMeanLoad(item.Key)).Sum();

            // FIND WIND GAMMAS            
            var cfWs = genes.ToDictionary(item => item.Key, item => CountryInfo.GetOnshoreWindCf(item.Key));
            var gammaWs = genes.ToDictionary(item => item.Key, item => 1 / k);
            var sumW = loadSum * gamma;
            var minSumW = loadSum / k;
            foreach (var pair in cfWs.OrderByDescending(item => item.Value))
            {
                var load = CountryInfo.GetMeanLoad(pair.Key);
                var delta = sumW - minSumW;
                // Lots of "remaining power"; max out k.
                if (delta > load * (k - 1 / k))
                {
                    gammaWs[pair.Key] = k;
                    sumW -= k * load;
                }
                // Intermediate case...
                else if (sumW > 0)
                {
                    gammaWs[pair.Key] += delta / load;
                    break;
                }

                minSumW -= load * 1 / k;
            }

            // FIND SOLAR GAMMAS
            var cfSs = genes.ToDictionary(item => item.Key, item => CountryInfo.GetSolarCf(item.Key));
            var gammaSs = genes.ToDictionary(item => item.Key, item => 1 / (double)k);
            var sumS = loadSum * gamma;
            var minSumS = sumS / k;
            foreach (var pair in cfSs.OrderByDescending(item => item.Value))
            {
                var load = CountryInfo.GetMeanLoad(pair.Key);
                var delta = sumS - minSumS;
                // Lots of "remaining power"; max out k.
                if (delta > load * (k - 1 / (double)k))
                {
                    gammaSs[pair.Key] = k;
                    sumS -= k * load;
                }
                // Intermediate case...
                else if (sumS > 0)
                {
                    gammaSs[pair.Key] += delta / load;
                    break;
                }

                minSumS -= load * 1 / k;
            }

            foreach (var key in genes.Keys.ToArray())
            {
                genes[key].Gamma = gammaWs[key] * alpha + gammaSs[key] * (1 - alpha);
                genes[key].Alpha = gammaWs[key] * alpha / (alpha * gammaWs[key] + (1 - alpha) * gammaSs[key]);
            }

            var result = new NodeGenes(genes);
            // Make sanity check.
            var dAlpha = alpha - result.Alpha;
            var gGamma = gamma - result.Gamma;
            if (Math.Abs(dAlpha) > 1e-6) throw new ArgumentException("Alpha value wrong");
            if (Math.Abs(gGamma) > 1e-6) throw new ArgumentException("Gamma value wrong");

            return result;
        }

    }
}
