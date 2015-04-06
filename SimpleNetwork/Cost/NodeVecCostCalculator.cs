using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Cost.Optimization;
using BusinessLogic.Cost.Transmission;
using BusinessLogic.Utils;
using Optimization;

namespace BusinessLogic.Cost
{

    public class NodeVecCostCalculator : ICostCalculator<NodeVec>
    {
        
        private readonly NodeCostCalculator _mCalc;
        private int _mEvaluations;

        public int Evaluations
        {
            get { return _mEvaluations; }
        }

        public NodeVecCostCalculator()
        {
            // TODO: Enable changing the calc.
            _mCalc = new NodeCostCalculator(new ParameterEvaluator(false) { CacheEnabled = false }) { SolarCostModel = new SolarCostModelImpl() };            
        }

        public void UpdateCost(IList<NodeVec> solutions)
        {
            var toUpdate = solutions.Where(item => item.InvalidCost).ToArray();
            _mEvaluations += toUpdate.Length;
            foreach (var vec in toUpdate)
            {
                vec.Normalize();
                vec.Cost = _mCalc.SystemCost(new NodeChromosome(vec).Genes, true) + GenePool.Penalty(vec);
            }
        }
    }

}
