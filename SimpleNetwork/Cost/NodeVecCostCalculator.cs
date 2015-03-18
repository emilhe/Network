using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost.Optimization;
using Optimization;

namespace BusinessLogic.Cost
{
    // ADAPTER!!!
    public class NodeVecCostCalculator : ICostCalculator<NodeVec>
    {
        readonly ParallelNodeCostCalculator _mCore = new ParallelNodeCostCalculator();

        public int Evaluations
        {
            get { return _mCore.Evaluations; }
        }

        public void UpdateCost(IList<NodeVec> solutions)
        {
            var map = solutions.ToDictionary(item => item, item => new NodeChromosome(item));
            _mCore.UpdateCost(map.Values.ToList());
            foreach (var solution in solutions)
            {
                solution.Cost = map[solution].Cost;
            }
        }
    }
}
