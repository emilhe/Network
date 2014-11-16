using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using BusinessLogic.Interfaces;
using BusinessLogic.TimeSeries;

namespace BusinessLogic
{
    public class Simulation
    {

        #region Fields

        // System fields.
        private Dictionary<string, ITimeSeries> _mSystemTimeSeries;
        private readonly Stopwatch _mWatch;
        private readonly bool _mDebug;
        private bool _mSuccess;

        // Current iteration fields.
        private List<ITickListener> _mTickListeners; 
        private List<IMeasureable> _mMeasureables;

        #endregion

        /// <summary>
        /// The simulation model.
        /// </summary>
        public NetworkModel Model { get; set; }

        /// <summary>
        /// The latest simulation output (if any).
        /// </summary>
        public SimulationOutput Output { get; set; }

        /// <summary>
        /// Construction.
        /// </summary>
        /// <param name="model"> model to evaluate </param>
        /// <param name="debug"> if true, debug info is printed to console </param>
        public Simulation(NetworkModel model, bool debug = false)
        {
            _mDebug = debug;
            Model = model;

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
            SetupTickListeners();
            if (log) SetupLoggers();
            else Reset();

            // Simulation main loop.
            var tick = 0;
            _mSuccess = true;
            while (tick < ticks)
            {
                if (_mDebug) _mWatch.Restart();
                SignalTickChanged(tick);
                Model.Evaluate(tick);
                if (log) SignalLoggers(tick);
                if (Model.Failure) _mSuccess = false;
                if (_mDebug) Console.WriteLine("Total: " + _mWatch.ElapsedMilliseconds);
                tick++;
                if (!log && Model.Failure) break;
                //if(_mTick % 10000 == 0) Console.WriteLine("Progress: {0} of {1}",_mTick, ticks);
            }

            CreateOutput(log);
        }

        /// <summary>
        /// Reset storages (if a new simulation is initiated).
        /// </summary>
        private void ResetStorages()
        {
            foreach (var item in Model.Nodes.SelectMany(item => item.StorageCollection))
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
            foreach (var node in Model.Nodes) node.Clear();
            // Reset edge time series.
            Model.ExportStrategy.Clear();
            Model.FailureStrategy.Reset();
        }

        /// <summary>
        /// Setup tick listeners.
        /// </summary>
        private void SetupTickListeners()
        {
            _mTickListeners = new List<ITickListener>();
            _mTickListeners.AddRange(Model.Nodes);
        }


        /// <summary>
        /// Setup loggers.
        /// </summary>
        private void SetupLoggers()
        {
            // System time series setup.
            var systemTimeSeries = new List<ITimeSeries>
            {
                new DenseTimeSeries("Mismatch"),
                new DenseTimeSeries("Curtailment")
            };
            _mSystemTimeSeries = systemTimeSeries.ToDictionary(item => item.Name, item => item);

            // Node time series setup.
            _mMeasureables = new List<IMeasureable> {Model.ExportStrategy};
            _mMeasureables.AddRange(Model.Nodes);
            foreach (var measureable in _mMeasureables) measureable.Start();
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
            _mSystemTimeSeries["Mismatch"].AppendData(Model.Mismatch);
            _mSystemTimeSeries["Curtailment"].AppendData(Model.Curtailment);
            foreach (var measureable in _mMeasureables) measureable.Sample(tick);
        }

        /// <summary>
        /// Wrap output data.
        /// </summary>
        private void CreateOutput(bool log)
        {
            var ts = new List<ITimeSeries>();
            if(log) ts.AddRange(_mSystemTimeSeries.Values);
            if (log) ts.AddRange(Model.ExportStrategy.CollectTimeSeries());
            if (log) ts.AddRange(Model.Nodes.SelectMany(item => item.CollectTimeSeries()));

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
