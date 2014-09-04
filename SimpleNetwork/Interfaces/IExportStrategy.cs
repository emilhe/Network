using System.Collections.Generic;
using BusinessLogic.ExportStrategies;

namespace BusinessLogic.Interfaces
{
    public interface IExportStrategy : IMeasureableNode
    {

        void Bind(List<Node> nodes, double[] mismatches);
        BalanceResult BalanceSystem(int tick);

    }
}
