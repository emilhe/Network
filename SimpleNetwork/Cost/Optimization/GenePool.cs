using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace BusinessLogic.Cost
{
    public class 
        GenePool
    {

        // ALPHA/GAMMA LIMITS
        public static double K {
            get { return GammaMax; }
            set
            {
                GammaMax = value;
                GammaMin = 1.0/value;
            }
        } 
        public static double AlphaMin = 0;
        public static double AlphaMax = 1;

        public static double GammaMin = 1;
        public static double GammaMax = 1;
        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
            
        // Offshore fractions
        public static Dictionary<string, double> OffshoreFractions;
        private const double StepScale = 1;

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

        #region Chromosome

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
            ApplyOffshoreFraction(guess);
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

        #endregion

        #region Levy flight

        public static NodeChromosome DoLevyFlight(NodeChromosome chromosome, NodeChromosome best)
        {
            double rescaling;
            NodeChromosome guess;
            do
            {
                guess = LevyFlight(chromosome, best);
                rescaling = GammaRescaling(guess);
            } while (rescaling.Equals(double.NegativeInfinity));
            ScaleGamma(guess, rescaling);
            ApplyOffshoreFraction(guess);
            return guess;
                
        }

        private static NodeChromosome LevyFlight(NodeChromosome chromosome, NodeChromosome best)
        {
            var genes = new Dictionary<string, NodeGene>();
            var levy = LevyStep();

            foreach (var key in chromosome.Genes.Keys.ToArray())
            {
                var bestGene = best.Genes[key];
                var oldGene = chromosome.Genes[key];
                var newGene = new NodeGene {Alpha = oldGene.Alpha, Gamma = oldGene.Gamma};
                // First do alpha.
                newGene.Alpha += StepScale*levy*Rnd.NextDouble()*(bestGene.Alpha - oldGene.Alpha);
                if (newGene.Alpha < AlphaMin) newGene.Alpha = AlphaMin;
                if (newGene.Alpha > AlphaMax) newGene.Alpha = AlphaMax;
                // Then gamma.
                newGene.Gamma += StepScale*levy*Rnd.NextDouble()*(bestGene.Gamma - oldGene.Gamma);
                if (newGene.Gamma < GammaMin) newGene.Gamma = GammaMin;
                if (newGene.Gamma > GammaMax) newGene.Gamma = GammaMax;
                genes.Add(key, newGene);
            }

            return new NodeChromosome(new NodeGenes(genes));
        }

        private static double LevyStep()
        {
            return Rnd.NextLevy(0.5, 1);
        }

        #endregion

        #region Differential evolution

        public static NodeChromosome DoDifferentialEvolution(NodeChromosome chromosome, NodeChromosome[] chromosomes)
        {
            double rescaling;
            NodeChromosome guess;
            do
            {
                var i = (int) Math.Round(Rnd.NextDouble()*(chromosomes.Length - 1));
                var j = (int)Math.Round(Rnd.NextDouble() * (chromosomes.Length - 1));
                guess = DifferentialEvolution(chromosome, chromosomes[i], chromosomes[j]);
                rescaling = GammaRescaling(guess);
            } while (rescaling.Equals(double.NegativeInfinity));
            ScaleGamma(guess, rescaling);
            ApplyOffshoreFraction(guess);
            return guess;

        }

        private static NodeChromosome DifferentialEvolution(NodeChromosome chromosome, NodeChromosome other1, NodeChromosome other2)
        {
            var genes = new Dictionary<string, NodeGene>();

            foreach (var key in chromosome.Genes.Keys.ToArray())
            {
                var gene = chromosome.Genes[key];
                var gene1 = other1.Genes[key];
                var gene2 = other2.Genes[key];
                var newGene = new NodeGene { Alpha = gene.Alpha, Gamma = gene.Gamma };
                // First do alpha.
                newGene.Alpha += (gene1.Alpha - gene2.Alpha)*Rnd.NextDouble();
                if (newGene.Alpha < AlphaMin) newGene.Alpha = AlphaMin;
                if (newGene.Alpha > AlphaMax) newGene.Alpha = AlphaMax;
                // Then gamma.
                newGene.Gamma += (gene1.Gamma - gene2.Gamma)*Rnd.NextDouble();
                if (newGene.Gamma < GammaMin) newGene.Gamma = GammaMin;
                if (newGene.Gamma > GammaMax) newGene.Gamma = GammaMax;
                genes.Add(key, newGene);
            }

            return new NodeChromosome(new NodeGenes(genes));
        }

        #endregion


        #endregion

        #region Util methods

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
                if (GammaMin - value > 1e-5) return double.NegativeInfinity;
                if (GammaMax - value < -1e-5) return double.NegativeInfinity;
            }
            // Return the scaling factor.
            return chromosome.Gamma/effGamma;
        }

        private static void ScaleGamma(NodeChromosome chromosome, double scaling)
        {
            foreach (var gene in chromosome.Genes) gene.Value.Gamma *= scaling;
        }

        private static void ApplyOffshoreFraction(NodeChromosome chromosome)
        {
            ApplyOffshoreFraction(chromosome.Genes);
        }

        public static void ApplyOffshoreFraction(NodeGenes genes)
        {
            if (OffshoreFractions == null) return;
            foreach (var key in OffshoreFractions.Keys)
            {
                genes[key].OffshoreFraction = OffshoreFractions[key];
            }
        }

        #endregion

    }
}
