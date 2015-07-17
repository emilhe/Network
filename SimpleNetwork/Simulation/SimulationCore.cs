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
        public bool LogFlows { get; set; }

        #region Fields

        private readonly Stopwatch _mWatch;
        private readonly bool _mDebug;
        private bool _mSuccess;
        private int _mTicks;

        private List<ITickListener> _mTickListeners; 
        private readonly NetworkModel _mModel;

        #endregion

        #region Model delegation

        public INode[] Nodes
        {
            get { return _mModel.Nodes; }
            set { _mModel.Nodes = value; }
        }

        public IFailureStrategy FailureStrategy
        {
            get { return _mModel.FailureStrategy; }
            set { _mModel.FailureStrategy = value; }
        }

        public IExportScheme ExportScheme
        {
            get { return _mModel.ExportScheme; }
            set { _mModel.ExportScheme = value; }
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
            }

            CreateOutput();
        }

        /// <summary>
        /// Reset storages (if a new simulation is initiated).
        /// </summary>
        private void ResetStorages()
        {
            foreach (var item in Nodes.SelectMany(item => item.Storages))
            {
                item.ResetEnergy();
            }
        }

        /// <summary>
        /// Setup tick listeners.
        /// </summary>
        private void SetupTickListeners()
        {
            _mTickListeners = new List<ITickListener>();
            _mTickListeners.AddRange(Nodes);
        }

        /// <summary>
        /// Setup loggers.
        /// </summary>
        private void SetupLoggers()
        {
            foreach (var node in Nodes)
            {
                // Balancing loggers.
                node.Balancing.Start(_mTicks);
                // Nodal loggers.
                if (LogAllNodeProperties) node.Start(_mTicks);
                else node.Clear();
            }

            // Export scheme loggers.
            if (LogFlows) ExportScheme.Start(_mTicks);
            else ExportScheme.Clear();
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
            if (LogFlows) ExportScheme.Sample(tick);

            foreach (var node in Nodes)
            {
                node.Balancing.Sample(tick);
                if (LogAllNodeProperties) node.Sample(tick);                
            }
        }

        /// <summary>
        /// Wrap output data.
        /// </summary>
        private void CreateOutput()
        {
            var ts = new List<ITimeSeries>();
            ts.AddRange(Nodes.SelectMany(item => item.Balancing.CollectTimeSeries()));            
            if (LogFlows) ts.AddRange(ExportScheme.CollectTimeSeries());
            if (LogAllNodeProperties) ts.AddRange(Nodes.SelectMany(item => item.CollectTimeSeries()));

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
