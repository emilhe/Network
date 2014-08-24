using System.Collections.Generic;
using System.Linq;
using SimpleNetwork.ExportStrategies;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class NetworkModel
    {
        public List<Node> Nodes { get; private set; }

        private readonly IExportStrategy _mExportStrategy;

        #region Current iteration fields

        public double Mismatch { get; private set; }
        public double Curtailment { get; private set; }
        public bool Failure { get; private set; }

        private readonly double[] _mMismatches;
        private int _mTick;

        #endregion

        public NetworkModel(List<Node> nodes, IExportStrategy exportStrategy)
        {
            Nodes = nodes;
            _mExportStrategy = exportStrategy;
            _mMismatches = new double[Nodes.Count];
            _mExportStrategy.Bind(Nodes, _mMismatches);
        }

        public void Respond(int tick)
        {
            _mTick = tick;

            CalculateMismatches();
            BalanceSystem();
            CurtailExcessEnergy();
        }

        private void BalanceSystem()
        {
            _mExportStrategy.BalanceSystem(_mTick);
        }

        /// <summary>
        /// Determine system response; charge or discharge.
        /// </summary>
        private void CalculateMismatches()
        {
            for (int i = 0; i < Nodes.Count; i++) _mMismatches[i] = Nodes[i].GetDelta(_mTick);
            Mismatch = _mMismatches.Sum();
        }

        /// <summary>
        /// Curtail all exess energy and report any negative curtailment (success = false).
        /// </summary>
        private void CurtailExcessEnergy()
        {
            Curtailment = _mMismatches.Sum();
            if (_mExportStrategy as NoExportStrategy != null)
            {
                Failure = _mMismatches.Any(item => item < -_mExportStrategy.Tolerance);
            }
            else
            {
                Failure = _mMismatches.Sum() < -_mExportStrategy.Tolerance*_mMismatches.Length;                
            }
        }

    }
}
