using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost.Optimization;
using Utils;

namespace BusinessLogic.Cost
{
    public class

        GenePool
    {

        // ALPHA/GAMMA LIMITS
        public static double K
        {
            get { return GammaMax; }
            set
            {
                GammaMax = value;
                GammaMin = 1.0 / value;
            }
        }
        public static double AlphaMin = 0;
        public static double AlphaMax = 1;

        public static double GammaMin = 1;
        public static double GammaMax = 1;
        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);

        public static double StepScale = 0.1;
        public static double LevyAlpha = 1.5; //0.5
        public static double LevyBeta = 0; // 1

        // Offshore fractions
        public static Dictionary<string, double> OffshoreFractions;


        #region Gene modification

        public static NodeGene RndGene()
        {
            return new NodeGene
            {
                Alpha = RndAlpha(),
                Gamma = RndGamma()
            };
        }

        public static double RndGamma()
        {
            return Rnd.NextDouble() * (GammaMax - GammaMin) + GammaMin;
        }

        public static double RndAlpha()
        {
            return Rnd.NextDouble() * (AlphaMax - AlphaMin) + AlphaMin;
        }

        #endregion

        #region Safe (respects gamma limits) gene modification.

        #region Chromosome

        public static NodeChromosome SpawnChromosome()
        {
            double rescaling;
            NodeChromosome guess;
            //var guess = new NodeChromosome(new NodeGenes(RndGene));
            //RecursiveGammaRescaling(guess, new List<string>());
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
                var guess = chromosome.Genes[key].Gamma + 0.25 * (0.5 - Rnd.NextDouble());
                if (guess < GammaMin) guess = GammaMin;
                if (guess > GammaMax) guess = GammaMax;
                return guess;

            });
            ChangeAlpha(chromosome, key, () =>
            {
                var guess = chromosome.Genes[key].Alpha + 0.10 * (0.5 - Rnd.NextDouble());
                if (guess < AlphaMin) guess = AlphaMin;
                if (guess > AlphaMax) guess = AlphaMax;
                return guess;
            });
        }

        #endregion

        #region Levy flight

        public static NodeChromosome DoLevyFlight(NodeChromosome chromosome, NodeChromosome best, double stepSize = 0)
        {
            double rescaling;
            NodeChromosome guess;
            //var guess = LevyFlight(chromosome, best);
            //RecursiveGammaRescaling(guess, new List<string>());
            do
            {
                guess = LevyFlight(chromosome, best, stepSize);
                rescaling = GammaRescaling(guess);
            } while (rescaling.Equals(double.NegativeInfinity));
            ScaleGamma(guess, rescaling);
            ApplyOffshoreFraction(guess);
            return guess;

        }

        private static NodeChromosome LevyFlight(NodeChromosome chromosome, NodeChromosome best, double stepSize = 0)
        {
            var genes = new Dictionary<string, NodeGene>();
            var levy = LevyStep();
            if (stepSize == 0) stepSize = StepScale;

            foreach (var key in chromosome.Genes.Keys.ToArray())
            {
                var bestGene = best.Genes[key];
                var oldGene = chromosome.Genes[key];
                var newGene = new NodeGene { Alpha = oldGene.Alpha, Gamma = oldGene.Gamma };
                // First do alpha.
                newGene.Alpha += stepSize * levy * (bestGene.Alpha - oldGene.Alpha);
                if (newGene.Alpha < AlphaMin) newGene.Alpha = AlphaMin;
                if (newGene.Alpha > AlphaMax) newGene.Alpha = AlphaMax;
                // Then gamma.
                newGene.Gamma += stepSize * levy * (bestGene.Gamma - oldGene.Gamma);
                if (newGene.Gamma < GammaMin) newGene.Gamma = GammaMin;
                if (newGene.Gamma > GammaMax) newGene.Gamma = GammaMax;
                genes.Add(key, newGene);
            }

            return new NodeChromosome(new NodeGenes(genes));
        }

        private static double LevyStep()
        {
            return Rnd.NextLevy(LevyAlpha, LevyBeta);
        }

        #endregion

        #region Differential evolution

        public static NodeChromosome DoDifferentialEvolution(NodeChromosome chromosome, NodeChromosome[] chromosomes, int i, int j)
        {
            double rescaling;
            NodeChromosome guess;
            //var guess = DifferentialEvolution(chromosome, chromosomes[i], chromosomes[j]);
            //RecursiveGammaRescaling(guess, new List<string>());
            do
            {
                guess = DifferentialEvolution(chromosome, chromosomes[i], chromosomes[j]);
                rescaling = GammaRescaling(guess);
                i = (int)Math.Round(Rnd.NextDouble() * (chromosomes.Length - 1));
                j = (int)Math.Round(Rnd.NextDouble() * (chromosomes.Length - 1));
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
                newGene.Alpha += (gene1.Alpha - gene2.Alpha) * Rnd.NextDouble();
                if (newGene.Alpha < AlphaMin) newGene.Alpha = AlphaMin;
                if (newGene.Alpha > AlphaMax) newGene.Alpha = AlphaMax;
                // Then gamma.
                newGene.Gamma += (gene1.Gamma - gene2.Gamma) * Rnd.NextDouble();
                if (newGene.Gamma < GammaMin) newGene.Gamma = GammaMin;
                if (newGene.Gamma > GammaMax) newGene.Gamma = GammaMax;
                genes.Add(key, newGene);
            }

            return new NodeChromosome(new NodeGenes(genes));
        }

        #endregion

        #endregion

        #region Util methods

        public static bool Renormalize(double[] vec)
        {
            var rescaling = GammaRescaling(vec);
            if (rescaling.Equals(double.NegativeInfinity)) return false;

            ScaleGamma(vec, rescaling);
            return true;
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
            return chromosome.Gamma / effGamma;
        }

        public static double GammaRescaling(NodeChromosome chromosome)
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
            return chromosome.Gamma / effGamma;
        }

        private static double GammaRescaling(double[] vec)
        {
            var n = NodeVec.Labels.Count;
            // Calculte new effective gamma.
            var wind = 0.0;
            var solar = 0.0;
            for (int i = 0; i < n; i++)
            {
                var load = CountryInfo.GetMeanLoad(NodeVec.Labels[i]);
                wind += vec[i] * load * vec[i + n];
                solar += vec[i] * load * (1 - vec[i + n]);
            }
            var effGamma = (wind + solar) / CountryInfo.GetMeanLoadSum();
            // Check if the new effective gamma violates the contstraints.
            for (int i = 0; i < n; i++)
            {
                var value = vec[i] / effGamma;
                if (GammaMin - value > 1e-5) return double.NegativeInfinity;
                if (GammaMax - value < -1e-5) return double.NegativeInfinity;
            }
            // Return the scaling factor (TODO: NOT ONE; SHOULD BE ALL OVER GAMMA).
            return 1 / effGamma;
        }

        public static double Penalty(NodeVec vec)
        {

            var n = NodeVec.Labels.Count;
            var penalty = 0.0;
            var delta = 1e-6;
            // Calculate alpha/gamma penalties.
            for (int i = 0; i < n; i++)
            {
                if (vec[i] < GammaMax + delta) penalty -= Math.Log(GammaMax + (delta - vec[i]));
                else penalty += 1e3;
                if (vec[i] > GammaMin - delta) penalty -= Math.Log(vec[i] - (GammaMin - delta));
                else penalty += 1e3;
                if (vec[i + n] < AlphaMax + delta) penalty -= Math.Log(AlphaMax + (delta - vec[i + n]));
                else penalty += 1e3;
                if (vec[i + n] > AlphaMin - delta) penalty -= Math.Log(vec[i] - (AlphaMin - delta));
                else penalty += 1e3;
            }
            return penalty / 1000;
        }

        private static void ScaleGamma(double[] vec, double scaling)
        {
            for (int i = 0; i < NodeVec.Labels.Count; i++) vec[i] *= scaling;
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

        public static List<string> RecursiveGammaRescaling(NodeChromosome chromosome, List<string> fixedKeys)
        {
            var genes = chromosome.Genes;

            // Calculte new effective gamma (for VARAIBLE .
            var varWind = 0.0;
            var fixedWind = 0.0;
            var varSolar = 0.0;
            var fixedSolar = 0.0;
            foreach (var key in genes.Keys)
            {
                var load = CountryInfo.GetMeanLoad(key);
                var wind = genes[key].Gamma * load * genes[key].Alpha;
                var solar = genes[key].Gamma * load * (1 - genes[key].Alpha);
                if (fixedKeys.Contains(key))
                {
                    fixedWind += wind;
                    fixedSolar += solar;
                }
                else
                {
                    varWind += wind;
                    varSolar += solar;
                }
            }
            var effGamma = (varWind + varSolar) / (CountryInfo.GetMeanLoadSum() - (fixedWind + fixedSolar));

            // Check if the new effective gamma violates the contstraints.
            var failure = false;
            foreach (var key in genes.Keys)
            {
                if (fixedKeys.Contains(key)) continue;
                var newGamma = genes[key].Gamma * chromosome.Gamma / effGamma;
                if (GammaMin - newGamma > 1e-5)
                {
                    // Normalization NOT possible. Let's fix this value and try again.
                    genes[key].Gamma = GammaMin;
                    fixedKeys.Add(key);
                    failure = true;

                }
                if (GammaMax - newGamma < -1e-5)
                {
                    // Normalization NOT possible. Let's fix this value and try again.
                    genes[key].Gamma = GammaMax;
                    fixedKeys.Add(key);
                    failure = true;
                }
            }
            // Violation; try fixing the problematic values and do another rescaling.
            if (failure) return RecursiveGammaRescaling(chromosome, fixedKeys);

            // No violation; just do the rescaling!
            foreach (var key in chromosome.Genes.Keys)
            {
                if (fixedKeys.Contains(key)) continue;
                chromosome.Genes[key].Gamma /= effGamma;
            }

            return fixedKeys;
        }

        #endregion

    }
}