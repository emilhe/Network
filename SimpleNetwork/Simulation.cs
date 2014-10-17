using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using BusinessLogic.TimeSeries;
using ITimeSeries = BusinessLogic.Interfaces.ITimeSeries;

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
        private int _mTick;

        #endregion

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
            if (log) StartLogging();
            else ClearLogs();

            // Simulation main loop.
            _mTick = 0;
            _mSuccess = true;
            while (_mTick < ticks)
            {
                if (_mDebug) _mWatch.Restart();
                Model.Evaluate(_mTick);
                if (log) _mSystemTimeSeries["Mismatch"].AppendData(Model.Mismatch);
                if (log) _mSystemTimeSeries["Curtailment"].AppendData(Model.Curtailment);
                if (Model.Failure)
                {
                    _mSuccess = false;
                }
                if (_mDebug) Console.WriteLine("Total: " + _mWatch.ElapsedMilliseconds);
                _mTick++;
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
            foreach (var storage in Model.Nodes.SelectMany(item => item.StorageCollection.Storages()))
                storage.ResetCapacity();
        }

        /// <summary>
        /// Reset simulation parameters.
        /// </summary>
        private void ClearLogs()
        {
            _mSystemTimeSeries = null;

            // Reset node time series.
            foreach (var node in Model.Nodes) node.Reset();
            // Reset edge time series.
            Model.ExportStrategy.Reset();
        }

        /// <summary>
        /// Initialize simulation parameters.
        /// </summary>
        private void StartLogging()
        {
            // System time series setup.
            var systemTimeSeries = new List<ITimeSeries>
            {
                new DenseTimeSeries("Mismatch"),
                new DenseTimeSeries("Curtailment")
            };
            _mSystemTimeSeries = systemTimeSeries.ToDictionary(item => item.Name, item => item);

            // Node time series setup.
            foreach (var node in Model.Nodes) node.StartMeasurement();
            // Reset edge time series.
            Model.ExportStrategy.StartMeasurement();
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
