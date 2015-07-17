using System.Collections.Generic;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Simulation
{
    public interface ISimulation
    {

        INode[] Nodes { get; set; }
        IFailureStrategy FailureStrategy { get; set; }
        IExportScheme ExportScheme { get; set; }

        void Simulate(int ticks);

        SimulationOutput Output { get; }

    }
}
