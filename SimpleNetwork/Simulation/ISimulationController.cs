using System.Collections.Generic;
using BusinessLogic.Cost;

namespace BusinessLogic.Simulation
{
    public interface ISimulationController
    {
        //bool PrintDebugInfo { get; set; }
        bool CacheEnabled { get; set; }
        bool InvalidateCache { get; set; }
        //bool LogAllNodeProperties { get; set; }
        //bool LogFlows { get; set; }
        //List<TsSourceInput> Sources { get; set; }
        //List<ExportSchemeInput> ExportStrategies { get; set; }
        //Dictionary<string, Func<TsSourceInput, List<CountryNode>>> NodeFuncs { get; set; }
        //Dictionary<string, Func<List<CountryNode>, EdgeCollection>> EdgeFuncs { get; set; }
        //Dictionary<string, Func<IFailureStrategy>> FailFuncs { get; set; }
        List<SimulationOutput> EvaluateTs(NodeGenes genes);
        List<SimulationOutput> EvaluateTs(double penetration, double mixing);
        //List<GridResult> EvaluateGrid(GridScanParameters grid);
    }
}