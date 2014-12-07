using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class SaOptimizer<T> where T : ISolution
    {

        public double Temperature { get; set; }
        public double Epsilon { get; set; }
        public double Alpha { get; set; }

        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);        

        public T Optimize(T current)
        {
            var temperature = Temperature;
            var iteration = 0.0;

            // Main loop.
            while (temperature > Epsilon)
            {
                iteration++;

                var next = (T) current.Clone();
                next.Mutate();
                var delta = next.Cost - current.Cost;

                // If the next is better, go for it.
                if (delta < 0.0) current = next;
                else
                {
                    // If the next is worse, go for it with temperature dependent probability (Boltzmann).
                    var prob = Rnd.NextDouble();
                    if (!(prob < Math.Exp(-delta/temperature))) continue;
                    current = next;
                }

                // Cool down.
                temperature *= Alpha;
                
                // Debug info.
                if (iteration % 10 == 0) Console.WriteLine(current.Cost);
            }

            return current;
        }

    }
}
