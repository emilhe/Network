using System.Collections.Generic;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Simulation
{
    public interface ISimulation
    {

        IList<INode> Nodes { get; set; }
        IFailureStrategy FailureStrategy { get; set; }
        IExportStrategy ExportStrategy { get; set; }

        void Simulate(int ticks, LogLevelEnum logLevel = LogLevelEnum.Full);

        SimulationOutput Output { get; }

    }
}
