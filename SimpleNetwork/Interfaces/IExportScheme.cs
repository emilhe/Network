using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Nodes;

namespace BusinessLogic.Interfaces
{
    public interface IExportScheme : IMeasureable
    {

        void Bind(IList<INode> nodes, double[] mismatches);
        void BalanceSystem();

    }
}
