using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;
using Utils;

namespace BusinessLogic.Cost
{
    public class 
        GenePool
    {

        // ALPHA/GAMMA LIMITS
        public static double K = 1;
        public static double AlphaMin = 0;
        public static double AlphaMax = 1;

        public static readonly double GammaMin = 1/K;
        public static readonly double GammaMax = K;

        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);

        #region Gene modification

        public static NodeGene RndGene()
        {
            return new NodeGene
            {
                Alpha = Rnd.NextDouble() * (AlphaMax - AlphaMin) + AlphaMin,
                Gamma = Rnd.NextDouble() * (GammaMax - GammaMin) + GammaMin
            };
        }

        #endregion

        #region Safe (respects gamma limits) gene modification.

        public static NodeChromosome SpawnParticle()
        {
            double rescaling;
            NodeChromosome guess;
            do
            {
                guess = new NodeChromosome(new NodeGenes(RndGene));
                rescaling = GammaRescaling(guess);
            } while (rescaling.Equals(double.NegativeInfinity));
            ScaleGamma(guess, rescaling);
            return guess;
        }

        public static NodeChromosome SpawnChromosome()
        {
            double rescaling;
            NodeChromosome guess;
            do
            {
                guess = new NodeChromosome(new NodeGenes(RndGene));
                rescaling = GammaRescaling(guess);
            } while (rescaling.Equals(double.NegativeInfinity));
            ScaleGamma(guess, rescaling);
            return guess;
        }

        public static void TryReSeed(NodeChromosome chromosome, string key)
        {
            ChangeGamma(chromosome, key, () => Rnd.NextDouble() * (GammaMax - GammaMin) + GammaMin);
            ChangeAlpha(chromosome, key, () => Rnd.NextDouble() * (AlphaMax - AlphaMin) + AlphaMin);
        }

        public static void TryMutate(NodeChromosome chromosome, string key)
        {
            ChangeGamma(chromosome, key, () =>
            {
                var guess = chromosome.Genes[key].Gamma + 0.25*(0.5 - Rnd.NextDouble());
                if (guess < GammaMin) guess = GammaMin;
                if (guess > GammaMax) guess = GammaMax;
                return guess;

            });
            ChangeAlpha(chromosome, key, () =>
            {
                var guess = chromosome.Genes[key].Alpha + 0.10*(0.5 - Rnd.NextDouble());
                if (guess < AlphaMin) guess = AlphaMin;
                if (guess > AlphaMax) guess = AlphaMax;
                return guess;
            });
        }

        public static bool Renormalize(NodeChromosome chromosome)
        {
            var rescaling = GammaRescaling(chromosome);
            if (rescaling.Equals(double.NegativeInfinity)) return false;

            ScaleGamma(chromosome, rescaling);
            return true;
        }

        private static void ChangeGamma(NodeChromosome chromosome, string key, Func<double> change)
        {
            var gamma = change();
            var rescaling = GammaRescaling(chromosome, key, gamma, chromosome.Genes[key].Alpha);
            if (rescaling.Equals(double.NegativeInfinity)) return;
            chromosome.Genes[key].Gamma = gamma;
            ScaleGamma(chromosome, rescaling);
        }

        private static void ChangeAlpha(NodeChromosome chromosome, string key, Func<double> change)
        {
            var alpha = change();
            var rescaling = GammaRescaling(chromosome, key, chromosome.Genes[key].Gamma, alpha);
            if (rescaling.Equals(double.NegativeInfinity)) return;
            chromosome.Genes[key].Alpha = alpha;
            ScaleGamma(chromosome, rescaling);
        }

        #endregion

        #region Util methods

        private static double GammaRescaling(NodeChromosome chromosome, string country, double gamma, double alpha)
        {
            var genes = chromosome.Genes;
            // Calculte new effective gamma.
            var wind = 0.0;
            var solar = 0.0;
            foreach (var key in genes.Keys)
            {
                var load = CountryInfo.GetMeanLoad(key);
                if (key.Equals(country))
                {
                    wind += gamma * load * alpha;
                    solar += gamma * load * (1 - alpha);
                }
                else
                {
                    wind += genes[key].Gamma * load * genes[key].Alpha;
                    solar += genes[key].Gamma * load * (1 - genes[key].Alpha);
                }
            }
            var effGamma = (wind + solar) / CountryInfo.GetMeanLoadSum();
            // Check if the new effective gamma violates the contstraints.
            foreach (var value in genes.Values.Select(item => item.Gamma * chromosome.Gamma / effGamma))
            {
                if (value < GammaMin) return double.NegativeInfinity;
                if (value > GammaMax) return double.NegativeInfinity;
            }
            // Return the scaling factor.
            return chromosome.Gamma/effGamma;
        }

        private static double GammaRescaling(NodeChromosome chromosome)
        {
            var genes = chromosome.Genes;
            // Calculte new effective gamma.
            var wind = 0.0;
            var solar = 0.0;
            foreach (var key in genes.Keys)
            {
                var load = CountryInfo.GetMeanLoad(key);
                wind += genes[key].Gamma * load * genes[key].Alpha;
                solar += genes[key].Gamma * load * (1 - genes[key].Alpha);
            }
            var effGamma = (wind + solar) / CountryInfo.GetMeanLoadSum();
            // Check if the new effective gamma violates the contstraints.
            foreach (var value in genes.Values.Select(item => item.Gamma * chromosome.Gamma / effGamma))
            {
                if (value < GammaMin) return double.NegativeInfinity;
                if (value > GammaMax) return double.NegativeInfinity;
            }
            // Return the scaling factor.
            return chromosome.Gamma/effGamma;
        }

        private static void ScaleGamma(NodeChromosome chromosome, double scaling)
        {
            foreach (var gene in chromosome.Genes) gene.Value.Gamma *= scaling;
        }

        #endregion

    }
}
