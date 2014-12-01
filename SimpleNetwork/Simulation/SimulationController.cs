using System;
using System.Collections.Generic;
using System.Diagnostics;
using BusinessLogic.Cost;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
using BusinessLogic.FailureStrategies;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Utils;
using Newtonsoft.Json;
using SimpleImporter;
using Utils;

namespace BusinessLogic.Simulation
{
    public class SimulationController
    {

        public bool CacheEnabled { get; set; }
        public bool InvalidateCache { get; set; }
        public LogLevelEnum LogLevel { get; set; } // Dangerous when using cache, NOT included so far.

        #region Input parameters

        // Mandatory parameters.
        public List<TsSourceInput> Sources { get; set; }
        public List<ExportStrategyInput> ExportStrategies { get; set; }
        // Optional parameters.
        public Dictionary<string, Func<TsSourceInput, List<CountryNode>>> NodeFuncs { get; set; }
        public Dictionary<string, Func<List<CountryNode>, EdgeSet>> EdgeFuncs { get; set; }
        public Dictionary<string, Func<IFailureStrategy>> FailFuncs { get; set; }
        
        // Current iteration parameters.
        private string _mNodeTag = "";
        private string _mEdgeTag = "";
        private string _mFailTag = "";
        private EdgeSet _mEdges;
        private List<CountryNode> _mNodes;
        private TsSourceInput _mSrcIn;
        private IFailureStrategy _mFail;
        private ExportStrategyInput _mExpStratIn;

        #endregion

        public SimulationController()
        {
            CacheEnabled = true;
            InvalidateCache = false;
            LogLevel = LogLevelEnum.Full;

            // Default way to construct nodes.
            NodeFuncs = new Dictionary<string, Func<TsSourceInput, List<CountryNode>>>();
            NodeFuncs.Add("6h batt (homo), 25TWh hydrogen (homo), 150 TWh hydro-bio (homo)",
                s => ConfigurationUtils.CreateNodesWithBackup(s.Source, s.Length, s.Offset));
            // Default way to construct edges.
            EdgeFuncs = new Dictionary<string, Func<List<CountryNode>, EdgeSet>> { { "Europe edges", ConfigurationUtils.GetEuropeEdges } };
            // Default way to define failures.
            FailFuncs = new Dictionary<string, Func<IFailureStrategy>>{{"No blackout", () => new NoBlackoutStrategy()}};

            Sources = new List<TsSourceInput>();
            ExportStrategies = new List<ExportStrategyInput>();
        }

        #region Execution

        public List<SimulationOutput> EvaluateTs(Chromosome genes)
        {
            return Execute(update =>
            {
                var prop = GetProperties(genes);
                SimulationOutput result = null;

                // Try to load from disk (unless cache is invalidated)
                if (CacheEnabled && !InvalidateCache)
                {
                    result = AccessClient.LoadSimulationOutput(prop.UniqueKey().ToString());
                }

                // If no result is found, calculate it.
                if (result == null)
                {
                    update();
                    result = RunSimulation(MapFromInput(_mExpStratIn), _mSrcIn.Length, genes);
                    foreach (var property in prop) result.Properties.Add(property.Key, property.Value);

                    // If cache is enabled, save the result.
                    if (CacheEnabled)
                    {
                        AccessClient.SaveSimulationOutput(result, prop.UniqueKey().ToString());
                    }
                }

                return result;
            });
        }

        public List<SimulationOutput> EvaluateTs(double penetration, double mixing)
        {
            return Execute(update =>
            {
                var prop = GetProperties(penetration, mixing);
                SimulationOutput result = null;
                
                // Try to load from disk (unless cache is invalidated)
                if (CacheEnabled && !InvalidateCache)
                {
                    result = AccessClient.LoadSimulationOutput(prop.UniqueKey().ToString());
                }

                // If no result is found, calculate it.
                if (result == null)
                {
                    update();
                    result = RunSimulation(MapFromInput(_mExpStratIn), _mSrcIn.Length, penetration, mixing);
                    foreach (var property in prop) result.Properties.Add(property.Key, property.Value);

                    // If cache is enabled, save the result.
                    if (CacheEnabled)
                    {
                        AccessClient.SaveSimulationOutput(result, prop.UniqueKey().ToString());
                    }
                }

                return result;
            });
        }

        public List<GridResult> EvaluateGrid(GridScanParameters grid)
        {
            return Execute(update =>
            {
                var prop = GetProperties(grid);
                GridResult result = null;
                // Try to load from disk (unless cache is invalidated)
                if (CacheEnabled && !InvalidateCache)
                {
                    result = AccessClient.LoadGridResult(prop.UniqueKey().ToString());
                }
                // If no result is found, calculate it.
                if (result == null)
                {
                    update();
                    result = new GridResult
                    {
                        Grid = RunSimulation(MapFromInput(_mExpStratIn), _mSrcIn.Length, grid),
                        Properties = DefaultProperties()
                    };
                }
                // If cache is enabled, save the result.
                if (CacheEnabled)
                {
                    AccessClient.SaveGridResult(result, prop.UniqueKey().ToString());
                }

                return result;
            });
        }

        private List<T> Execute<T>(Func<Action, T> function)
        {
            var results = new List<T>();

            foreach (var nodeFunc in NodeFuncs)
            {
                foreach (var source in Sources)
                {
                    foreach (var exportStrategy in ExportStrategies)
                    {
                        foreach (var edgeFunc in EdgeFuncs)
                        {
                            foreach (var failFunc in FailFuncs)
                            {
                                // Refresh key dependent values here.
                                _mSrcIn = source;
                                _mNodeTag = nodeFunc.Key;
                                _mEdgeTag = edgeFunc.Key;
                                _mFailTag = failFunc.Key;
                                _mExpStratIn = exportStrategy;

                                // Add sub result.
                                results.Add(function(() =>
                                {
                                    // Delay expensive non key dependent validations to here.
                                    _mNodes = nodeFunc.Value(source);
                                    _mEdges = edgeFunc.Value(_mNodes);
                                    _mFail = failFunc.Value();
                                }));
                            }
                        }
                    }
                }
            }

            return results;
        }

        #endregion

        private Dictionary<string, string> GetProperties(Chromosome genes)
        {
            var result = DefaultProperties();
            result.Add("Chromosome", JsonConvert.SerializeObject(genes));
            return result;
        }

        private Dictionary<string, string> GetProperties(double penetration, double mixing)
        {
            var result = DefaultProperties();
            result.Add("Pentration", penetration.ToString());
            result.Add("Mixing", mixing.ToString());
            return result;
        }

        private Dictionary<string, string> GetProperties(GridScanParameters grid)
        {
            var result = DefaultProperties();
            result.Add("Pentration", grid.PenetrationFrom + ":" + grid.PenetrationTo + "," + grid.PenetrationSteps);
            result.Add("Mixing", grid.MixingFrom + ":" + grid.MixingTo + "," + grid.MixingSteps);
            return result;
        }

        private Dictionary<string, string> DefaultProperties()
        {
            return new Dictionary<string, string>
                    {
                        {"DistributionStrategy", ((byte) _mExpStratIn.DistributionStrategy).ToString()},
                        {"ExportStrategy", ((byte) _mExpStratIn.ExportStrategy).ToString()},
                        {"TsSource", ((byte) _mSrcIn.Source).ToString()},
                        {"Length", _mSrcIn.Length.ToString()},
                        {"Offset", _mSrcIn.Offset.ToString()},
                        {"NodeTag", _mNodeTag},
                        {"EdgeTag", _mEdgeTag},
                        {"FailureTag", _mFailTag},
                    };
        } 

        private IExportStrategy MapFromInput(ExportStrategyInput input)
        {
            switch (input.ExportStrategy)
            {
                case ExportStrategy.None:
                    return new NoExportStrategy();
                case ExportStrategy.Selfish:
                    return new SelfishExportStrategy(MapFromEnum(input.DistributionStrategy));
                case ExportStrategy.Cooperative:
                    return new CooperativeExportStrategy(MapFromEnum(input.DistributionStrategy));
                case ExportStrategy.ConstrainedFlow:
                    return new ConstrainedFlowExportStrategy(_mNodes, _mEdges);
            }

            throw new ArgumentException("No strategy matching {0}.", input.ExportStrategy.GetDescription());
        }

        private IDistributionStrategy MapFromEnum(DistributionStrategy strategy)
        {
            switch (strategy)
            {
                case DistributionStrategy.SkipFlow:
                    return new SkipFlowStrategy();
                case DistributionStrategy.MinimalFlow:
                    return new MinimalFlowStrategy(_mNodes, _mEdges);
            }

            throw new ArgumentException("No strategy matching {0}.", strategy.GetDescription());
        }

        private bool[,] RunSimulation(IExportStrategy strategy, double years, GridScanParameters grid)
        {
            var model = new NetworkModel(_mNodes, strategy, _mFail);
            var simulation = new SimulationCore(model);
            //var mCtrl = new MixController(_mNodes);
            var watch = new Stopwatch();

            // Eval grid.
            return GridEvaluator.EvalSparse(delegate(int[] idxs)
            {
                var pen = grid.PenetrationFrom + grid.PenetrationStep * idxs[0];
                var mix = grid.MixingFrom + grid.MixingStep * idxs[1];
                foreach (var node in _mNodes)
                {
                    node.Model.Gamma = pen;
                    node.Model.Alpha = mix;
                }
                // Do simulation.
                watch.Restart();
                simulation.Simulate((int) (Utils.Utils.HoursInYear*years), LogLevelEnum.None);
                Console.WriteLine("Mix " + mix + "; Penetation " + pen + ": " +
                                  watch.ElapsedMilliseconds + ", " + (simulation.Output.Success ? "SUCCESS" : "FAIL"));
                return simulation.Output.Success;
            }, new[] { grid.PenetrationSteps, grid.MixingSteps });
        }

        private SimulationOutput RunSimulation(IExportStrategy strategy, double years, double penetration, double mixing)
        {
            var model = new NetworkModel(_mNodes, strategy, _mFail);
            var simulation = new SimulationCore(model);
            var watch = new Stopwatch();
            watch.Start();
            foreach (var node in _mNodes)
            {
                node.Model.Gamma = penetration;
                node.Model.Alpha = mixing;
            }
            simulation.Simulate((int)(Utils.Utils.HoursInYear * years), LogLevel);
            Console.WriteLine("Mix " + mixing + "; Penetation " + penetration + ": " +
                  watch.ElapsedMilliseconds + ", " + (simulation.Output.Success ? "SUCCESS" : "FAIL"));

            return simulation.Output;
        }

        private SimulationOutput RunSimulation(IExportStrategy strategy, double years, Chromosome genes)
        {
            var model = new NetworkModel(_mNodes, strategy, _mFail);
            var simulation = new SimulationCore(model);
            var watch = new Stopwatch();
            watch.Start();
                foreach (var node in _mNodes)
                {
                    node.Model.Gamma = genes[node.Name].Gamma;
                    node.Model.Alpha = genes[node.Name].Alpha;
                }
            simulation.Simulate((int) (Utils.Utils.HoursInYear * years), LogLevel);
            Console.WriteLine("Chromosome: " +
                  watch.ElapsedMilliseconds + ", " + (simulation.Output.Success ? "SUCCESS" : "FAIL"));

            return simulation.Output;
        }

    }

    public class TsSourceInput
    {
        public TsSource Source { get; set; }
        public int Length { get; set; } // In years
        public double Offset { get; set; } // In years

        public string Description
        {
            get { return string.IsNullOrEmpty(_mDescription) ? Source.GetDescription() + string.Format("-{0}-{1}", Offset, Length) : _mDescription; }
            set { _mDescription = value; }
        }

        private string _mDescription;
    }

    public class ExportStrategyInput
    {
        public ExportStrategy ExportStrategy { get; set; }
        public DistributionStrategy DistributionStrategy { get; set; }

        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(_mDescription)
                    ? ExportStrategy.GetDescription() + ", " + DistributionStrategy.GetDescription()
                    : _mDescription;
            }
            set { _mDescription = value; }
        }

        private string _mDescription;
    }

    public class GridResult
    {
        public Dictionary<string, string> Properties { get; set; }
        public bool[,] Grid { get; set; }
        public double[] Rows { get; set; }
        public double[] Columns { get; set; }

        public string Description
        {
            get { return string.IsNullOrEmpty(_mDescription) ? BuildPropertyString() : _mDescription; }
            set { _mDescription = value; }
        }

        private string BuildPropertyString()
        {
            var result = string.Empty;
            if (Properties == null) return result;
            var idx = 0;
            foreach (var property in Properties)
            {
                result += property.Value;
                if (idx < Properties.Count - 1) result += ", ";
                idx++;
            }
            return result;
        }

        private string _mDescription;
    }

}
