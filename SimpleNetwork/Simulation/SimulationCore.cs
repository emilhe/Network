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
        /// <param name="logLevel"> how much should be logged? </param>
        public void Simulate(int ticks, LogLevelEnum logLevel = LogLevelEnum.Full)
        {
            ResetStorages();
            SetupTickListeners();
            SetupLoggers(logLevel);
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
                SignalLoggers(tick, logLevel);
                if (_mModel.Failure) _mSuccess = false;
                if (_mDebug) Console.WriteLine("Total: " + _mWatch.ElapsedMilliseconds);
                tick++;
                if (logLevel == LogLevelEnum.None && _mModel.Failure) break;
                //if(_mTick % 10000 == 0) Console.WriteLine("Progress: {0} of {1}",_mTick, ticks);
            }

            CreateOutput(logLevel);
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
        /// Reset simulation parameters.
        /// </summary>
        private void Reset()
        {
            _mSystemTimeSeries = null;

            // Reset node time series.
            foreach (var node in Nodes) node.Clear();
            // Reset edge time series.
            ExportStrategy.Clear();
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
        private void SetupLoggers(LogLevelEnum logLevel)
        {
            if (logLevel == LogLevelEnum.None)
            {
                Reset();
                return;
            }

            // System time series setup.
            var systemTimeSeries = new List<DenseTimeSeries>
            {
                new DenseTimeSeries("Mismatch", _mTicks),
                new DenseTimeSeries("Curtailment", _mTicks)
            };
            _mSystemTimeSeries = systemTimeSeries.ToDictionary(item => item.Name, item => item);

            if (logLevel == LogLevelEnum.System) return;

            ExportStrategy.Start(_mTicks);

            if (logLevel == LogLevelEnum.Flow) return;

            foreach (var node in Nodes) node.Start(_mTicks);
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
        private void SignalLoggers(int tick, LogLevelEnum logLevel)
        {
            if (logLevel == LogLevelEnum.None) return;

            _mSystemTimeSeries["Mismatch"].AppendData(_mModel.Mismatch);
            _mSystemTimeSeries["Curtailment"].AppendData(_mModel.Curtailment);

            if (logLevel == LogLevelEnum.System) return;

            ExportStrategy.Sample(tick);

            if (logLevel == LogLevelEnum.Flow) return;

            foreach (var node in Nodes) node.Sample(tick);
        }

        /// <summary>
        /// Wrap output data.
        /// </summary>
        private void CreateOutput(LogLevelEnum logLevel)
        {
            var ts = new List<ITimeSeries>();
            var level = (int) logLevel;
            if (level > 0) ts.AddRange(_mSystemTimeSeries.Values);
            if (level > 1) ts.AddRange(ExportStrategy.CollectTimeSeries());
            if (level > 2) ts.AddRange(Nodes.SelectMany(item => item.CollectTimeSeries()));

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

    public enum LogLevelEnum
    {
        None = 0, System = 1, Flow = 2, Full = 3
    }

}
