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
using DataItems.TimeSeries;
using SimpleNetwork.Interfaces;
using ITimeSeries = SimpleNetwork.Interfaces.ITimeSeries;

namespace SimpleNetwork
{
    public class Simulation
    {

        #region Fields

        // System fields.
        private Dictionary<string, ITimeSeries> _mSystemTimeSeries;
        private readonly NetworkModel _mModel;
        private readonly Stopwatch _mWatch;
        private readonly bool _mDebug;
        private bool _mSuccess;

        // Current iteration fields.
        private int _mTick;

        #endregion

        /// <summary>
        /// The list of simulation nodes; each node represents a geographic entity.
        /// </summary>
        public List<Node> Nodes { get; set; }

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
            _mModel = model;
            Nodes = _mModel.Nodes;

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
            while (_mTick <= ticks)
            {
                if (_mDebug) _mWatch.Restart();
                _mModel.Respond(_mTick);
                if (log) _mSystemTimeSeries["Mismatch"].AddData(_mTick, _mModel.Mismatch);
                if (log) _mSystemTimeSeries["Curtailment"].AddData(_mTick, _mModel.Curtailment);
                if (_mModel.Failure) _mSuccess = false;
                if (_mDebug) Console.WriteLine("Total: " + _mWatch.ElapsedMilliseconds);
                _mTick++;
                if (!log && _mModel.Failure) break;
            }

            CreateOutput();
        }

        /// <summary>
        /// Reset storages (if a new simulation is initiated).
        /// </summary>
        private void ResetStorages()
        {
            foreach (var storage in Nodes.SelectMany(item => item.Storages.Values)) storage.ResetCapacity();
        }

        /// <summary>
        /// Reset simulation parameters.
        /// </summary>
        private void ClearLogs()
        {
            _mSystemTimeSeries = null;

            // Reset node time series.
            foreach (var measureable in Nodes.SelectMany(item => item.Measureables))
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
            foreach (var measureable in Nodes.SelectMany(item => item.Measureables))
            {
                measureable.StartMeasurement();
            }
        }

        /// <summary>
        /// Wrap output data.
        /// </summary>
        private void CreateOutput()
        {
            Output = new SimulationOutput
            {
                SystemTimeSeries = _mSystemTimeSeries,
                CountryTimeSeriesMap = new Dictionary<string, List<ITimeSeries>>(),
                Success = _mSuccess
            };
            foreach (var node in Nodes)
            {
                Output.CountryTimeSeriesMap.Add(node.CountryName, node.CollectTimeSeries());
            }
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
