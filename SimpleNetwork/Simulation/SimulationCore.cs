using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Simulation
{
    public class SimulationCore : ISimulation
    {

        public bool LogAllNodeProperties { get; set; }
        public bool LogSystemProperties { get; set; }
        public bool LogNodalBalancing { get; set; }
        public bool LogFlows { get; set; }

        #region Fields

        private Dictionary<string, DenseTimeSeries> _mSystemTimeSeries;
        private readonly Stopwatch _mWatch;
        private readonly bool _mDebug;
        private bool _mSuccess;
        private int _mTicks;

        private List<ITickListener> _mTickListeners; 
        private readonly NetworkModel _mModel;

        #endregion

        #region Model delegation

        public IList<INode> Nodes
        {
            get { return _mModel.Nodes; }
            set { _mModel.Nodes = value; }
        }

        public IFailureStrategy FailureStrategy
        {
            get { return _mModel.FailureStrategy; }
            set { _mModel.FailureStrategy = value; }
        }

        public IExportStrategy ExportStrategy
        {
            get { return _mModel.ExportStrategy; }
            set { _mModel.ExportStrategy = value; }
        }

        #endregion

        /// <summary>
        /// The latest simulation output (if any).
        /// </summary>
        public SimulationOutput Output { get; set; }

        /// <summary>
        /// Construction.
        /// </summary>
        /// <param name="model"> model to evaluate </param>
        /// <param name="debug"> if true, debug info is printed to console </param>
        public SimulationCore(NetworkModel model, bool debug = false)
        {
            _mDebug = debug;
            _mModel = model;

            if (_mDebug) {_mWatch = new Stopwatch();}
        }

        /// <summary>
        /// Simulate a number of ticks.
        /// </summary>
        /// <param name="ticks"> number of ticks to simulate </param>
        public void Simulate(int ticks)
        {
            ResetStorages();
            SetupTickListeners();
            SetupLoggers();
            FailureStrategy.Reset();

            // SimulationCore main loop.
            var tick = 0;
            _mSuccess = true;
            _mTicks = ticks;
            while (tick < ticks)
            {
                if (_mDebug) _mWatch.Restart();
                SignalTickChanged(tick);
                _mModel.Evaluate(tick);
                if (_mModel.Backup > 1e-3) _mSuccess = false; // Succes indicates that NO backup is needed.
                SignalLoggers(tick);
                if (_mModel.Failure) _mSuccess = false;
                if (_mDebug) Console.WriteLine("Total: " + _mWatch.ElapsedMilliseconds);
                tick++;
                //if (logLevel == LogLevelEnum.None && _mModel.Failure) break;
                //if(_mTick % 10000 == 0) Console.WriteLine("Progress: {0} of {1}",_mTick, ticks);
            }

            CreateOutput();
        }

        /// <summary>
        /// Reset storages (if a new simulation is initiated).
        /// </summary>
        private void ResetStorages()
        {
            foreach (var item in Nodes.SelectMany(item => item.StorageCollection))
            {
                item.Value.ResetEnergy();
            }
        }

        /// <summary>
        /// Setup tick listeners.
        /// </summary>
        private void SetupTickListeners()
        {
            _mTickListeners = new List<ITickListener>();
            _mTickListeners.AddRange(Nodes);
            _mTickListeners.AddRange(Nodes.Select(item => item.Balancing));
        }

        /// <summary>
        /// Setup loggers.
        /// </summary>
        private void SetupLoggers()
        {
            // System loggers.
            if (LogSystemProperties)
            {
                var systemTimeSeries = new List<DenseTimeSeries>
                {
                    new DenseTimeSeries("Curtailment", _mTicks),
                    new DenseTimeSeries("Mismatch", _mTicks),
                    new DenseTimeSeries("Backup", _mTicks),
                };
                _mSystemTimeSeries = systemTimeSeries.ToDictionary(item => item.Name, item => item);
            }
            else _mSystemTimeSeries = null;

            // Export strategy loggers.
            if (LogFlows) ExportStrategy.Start(_mTicks);
            else ExportStrategy.Clear();

            foreach (var node in Nodes)
            {
                // Nodal loggers.
                if (LogAllNodeProperties) node.Start(_mTicks);
                else node.Clear();
                // Balancing loggers.
                if (LogNodalBalancing) node.Balancing.Start(_mTicks);
                else node.Balancing.Clear();
            }
        }

        /// <summary>
        /// Signal to update models.
        /// </summary>
        private void SignalTickChanged(int tick)
        {
            foreach (var listener in _mTickListeners) listener.TickChanged(tick);
        }

        /// <summary>
        /// Signal to the measureable to sample.
        /// </summary>
        private void SignalLoggers(int tick)
        {
            if (LogSystemProperties)
            {
                _mSystemTimeSeries["Curtailment"].AppendData(_mModel.Curtailment);
                _mSystemTimeSeries["Mismatch"].AppendData(_mModel.Mismatch);
                _mSystemTimeSeries["Backup"].AppendData(_mModel.Backup);
            }

            if (LogFlows) ExportStrategy.Sample(tick);

            foreach (var node in Nodes)
            {
                if (LogNodalBalancing) node.Balancing.Sample(tick);
                if (LogAllNodeProperties) node.Sample(tick);                
            }
        }

        /// <summary>
        /// Wrap output data.
        /// </summary>
        private void CreateOutput()
        {
            var ts = new List<ITimeSeries>();
            if (LogFlows) ts.AddRange(ExportStrategy.CollectTimeSeries());
            if (LogSystemProperties) ts.AddRange(_mSystemTimeSeries.Values);
            if (LogAllNodeProperties) ts.AddRange(Nodes.SelectMany(item => item.CollectTimeSeries()));
            if (!LogAllNodeProperties && LogNodalBalancing) ts.AddRange(Nodes.SelectMany(item => item.Balancing.CollectTimeSeries()));

            Output = new SimulationOutput
            {
                TimeSeries = ts,
                Success = _mSuccess
            };
        }

    }

    public class SimulationOutput
    {

        /// <summary>
        /// All times series data collected during the simulation.
        /// </summary>
        public List<ITimeSeries> TimeSeries { get; set; }

        /// <summary>
        /// Did the system work; false if the backup system was exhausted.
        /// </summary>
        public bool Success
        {
            get
            {
                if (Properties == null) return false;
                if (!Properties.ContainsKey("Success")) return false;
                return Boolean.Parse(Properties["Success"]);
            }
            set
            {
                if (Properties == null) Properties = new Dictionary<string, string>();
                if (!Properties.ContainsKey("Success")) Properties.Add("Success", "");
                Properties["Success"] = value.ToString();
            }
        }

        /// <summary>
        /// Information about the simulation.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } 

    }

}
