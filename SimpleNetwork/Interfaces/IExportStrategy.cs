using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Nodes;

namespace BusinessLogic.Interfaces
{
    public interface IExportStrategy : IMeasureable
    {

        void Bind(List<INode> nodes, double[] mismatches);
        BalanceResult BalanceSystem();

    }
}
