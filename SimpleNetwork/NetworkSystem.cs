using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class NetworkSystem : IDisposable
    {

        private const double Delta = 1e-4;

        #region Fields

        // System fields.
        private Dictionary<string, ITimeSeries> _mSystemTimeSeries;
        private readonly FlowOptimizer _flowOptimizer;
        private int _mMaximumStorageLevel = -1;
        private List<Node> _mNodes;
        private bool _mSuccess = true;
        private bool _mLogging;        
        private readonly Stopwatch _mWatch;
        private readonly bool _mDebug;

        // Current iteration fields.
        private readonly double[] _mDeltas;
        private readonly double[] _mLoLims;
        private readonly double[] _mHiLims;
        private double _mDeltaSum;
        private int _mStorageLevel;
        private int _mTick;

        private bool OutOfStorage
        {
            get { return _mStorageLevel > _mMaximumStorageLevel; }
        }
        private Response SystemResponse
        {
            get { return (_mDeltaSum > 0) ? Response.Charge : Response.Discharge; }
        }

        #endregion

        /// <summary>
        /// The list of simulation nodes; each node represents a country.
        /// </summary>
        public List<Node> Nodes
        {
            get { return _mNodes; }
            set
            {
                _mNodes = value;
                if (_mNodes == null) return;

                // Auto detect the maximum storage level.
                _mMaximumStorageLevel = _mNodes.SelectMany(item => item.Storages.Keys).Max();
            }
        }

        /// <summary>
        /// The latest simulation output (if any).
        /// </summary>
        public SimulationOutput Output { get; set; }

        /// <summary>
        /// Construction.
        /// </summary>
        /// <param name="nodes"> the nodes of the system </param>
        /// <param name="edges"> network edges </param>
        public NetworkSystem(List<Node> nodes, EdgeSet edges, bool debug = false)
        {
            Nodes = nodes;

            _flowOptimizer = new FlowOptimizer(Nodes.Count);
            _flowOptimizer.SetEdges(edges);

            _mDeltas = new double[Nodes.Count];
            _mLoLims = new double[Nodes.Count];
            _mHiLims = new double[Nodes.Count];
            _mDebug = debug;

            if (_mDebug) {_mWatch = new Stopwatch();}
        }

        /// <summary>
        /// Simulate a number of ticks.
        /// </summary>
        /// <param name="ticks"> number of ticks to simulate </param>
        /// <param name="log"> is ts should be logged for system paramters </param>
        public void Simulate(int ticks, bool log = true)
        {
            ResetStorages();
            if (log) StartLogging();
            else ClearLogs();
            _mLogging = log;

            // Simulation main loop.
            _mSuccess = true;
            _mTick = 0;
            while (_mTick <= ticks)
            {
                Evaluate();
                if(_mLogging) _mSystemTimeSeries["Mismatch"].AddData(_mTick, _mDeltaSum);
                _mTick++;
                if(!_mLogging && !_mSuccess) break;
            }

            CreateOutput();
        }

        private void ResetStorages()
        {
            foreach (var storage in _mNodes.SelectMany(item => item.Storages.Values))
            {
                storage.ResetCapacity();
            }
        }

        /// <summary>
        /// Reset simulation parameters.
        /// </summary>
        private void ClearLogs()
        {
            _mSystemTimeSeries = null;

            // Reset node time series.
            foreach (var measureable in _mNodes.SelectMany(item => item.Measureables))
            {
                measureable.Reset();
            }
        }

        /// <summary>
        /// Initialize simulation parameters.
        /// </summary>
        private void StartLogging()
        {
            // System time series setup.
            var systemTimeSeries = new List<ITimeSeries>
            {
                new SparseTimeSeries("Mismatch"),
                new SparseTimeSeries("Curtailment")
            };
            _mSystemTimeSeries = systemTimeSeries.ToDictionary(item => item.Name, item => item);

            // Node time series setup.
            foreach (var measureable in _mNodes.SelectMany(item => item.Measureables))
            {
                measureable.StartMeasurement();
            }
        }

        /// <summary>
        /// Evaluate the simulation at _mTick. NB: INTEGER flow opt. = ~ 1 ms; CONTINOUS = ~ 500 ms.
        /// Evaluate the simulation at _mTick. NB: INTEGER at later points; 200 ms... 
        /// </summary>
        private void Evaluate()
        {
            if (_mDebug) _mWatch.Restart();
            DetermineSystemResponse();
            if (_mDebug) Console.WriteLine("System response: " + _mWatch.ElapsedMilliseconds);
            TraverseStorageLevels();
            if (_mDebug) Console.WriteLine("Traverse storage: " + _mWatch.ElapsedMilliseconds);
            OptimizeEnergyFlows();
            if (_mDebug) Console.WriteLine("Optimization: " + _mWatch.ElapsedMilliseconds);
            CurtailExcessEnergy();
            if (_mDebug) Console.WriteLine("Total: " + _mWatch.ElapsedMilliseconds);
        }

        private void CreateOutput()
        {
            Output = new SimulationOutput
            {
                SystemTimeSeries = _mSystemTimeSeries,
                CountryTimeSeriesMap = new Dictionary<string, List<ITimeSeries>>(),
                Success = _mSuccess
            };
            foreach (var node in _mNodes)
            {
                Output.CountryTimeSeriesMap.Add(node.CountryName, node.CollectTimeSeries());
            }
        }

        #region Evaluation subroutines.

        /// <summary>
        /// Determine system response; charge or discharge.
        /// </summary>
        private void DetermineSystemResponse()
        {
            for (int i = 0; i < Nodes.Count; i++) _mDeltas[i] = Nodes[i].GetDelta(_mTick);
            _mDeltaSum = _mDeltas.Sum();
        }

        /// <summary>
        /// Detmine the storage level at which the flow optimisation is to take place. Restore/drain all lower levels.
        /// </summary>
        private void TraverseStorageLevels()
        {
            _mStorageLevel = 0;
            while (InsufficientStorageAtCurrentLevel())
            {
                // Restore the lower storage level.
                for (int index = 0; index < Nodes.Count; index++)
                {
                    _mDeltas[index] += Nodes[index].Storages[_mStorageLevel].Restore(_mTick, SystemResponse);
                }
                // Go to the next storage level.
                _mStorageLevel++;
                if (OutOfStorage) return;
            }
            // Setup limits.
            var idx = 0;
            foreach (var storage in Nodes.Select(item => item.Storages[_mStorageLevel]))
            {
                _mLoLims[idx] = storage.RemainingCapacity(Response.Discharge);
                _mHiLims[idx] = storage.RemainingCapacity(Response.Charge);
                idx++;
            }
        }

        /// <summary>
        /// Optimize the energy flows and perform the optimal charges/discharges.
        /// </summary>
        private void OptimizeEnergyFlows()
        {
            if (OutOfStorage) return;

            // Determine FLOWS using Gurobi optimization.
            _flowOptimizer.SetNodes(_mDeltas, _mLoLims, _mHiLims);
            _flowOptimizer.Solve();

            // Charge based on flow optimization results.
            for (int index = 0; index < Nodes.Count; index++)
            {
                _mDeltas[index] = Nodes[index].Storages[_mStorageLevel].Inject(_mTick, _flowOptimizer.NodeOptimum[index]);
            }
        }

        /// <summary>
        /// Curtail all exess energy and report any negative curtailment (success = false).
        /// </summary>
        private void CurtailExcessEnergy()
        {
            var dSum = _mDeltas.Sum();
            if (dSum < -_mDeltas.Length*Delta)
            {
                _mSuccess = false;
            }
            if(_mLogging) _mSystemTimeSeries["Curtailment"].AddData(_mTick, dSum);
        }

        #endregion

        #region Help methods

        /// <summary>
        /// Determine if sufficient storage is availble at the current level.
        /// </summary>
        /// <returns> false if there is </returns>
        private bool InsufficientStorageAtCurrentLevel()
        {
            var storage = Nodes.Select(item => item.Storages[_mStorageLevel].RemainingCapacity(SystemResponse)).Sum();
            switch (SystemResponse)
            {
                case Response.Charge:
                    return storage < (_mDeltas.Sum() + _mDeltas.Length*Delta);
                case Response.Discharge:
                    // Flip sign signs; the numbers are negative.
                    return storage > (_mDeltas.Sum() - _mDeltas.Length*Delta);
                default:
                    throw new ArgumentException("Illegal Response.");
            }
        }

        #endregion

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            _flowOptimizer.Dispose();
        }

    }

    public class SimulationOutput
    {
        /// <summary>
        /// Time series for the complete system.
        /// </summary>
        public Dictionary<string, ITimeSeries> SystemTimeSeries { get; set; }

        /// <summary>
        /// Time series for each node in the system.
        /// </summary>
        public Dictionary<string, List<ITimeSeries>> CountryTimeSeriesMap { get; set; }

        /// <summary>
        /// Did the system work; false if the backup system was exhausted.
        /// </summary>
        public bool Success { get; set; }

    }

}
