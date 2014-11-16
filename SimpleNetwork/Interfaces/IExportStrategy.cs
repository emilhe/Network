using System.Collections.Generic;
using BusinessLogic.ExportStrategies;

namespace BusinessLogic.Interfaces
{
    public interface IExportStrategy : IMeasureable
    {

        void Bind(List<Node> nodes, double[] mismatches);
        BalanceResult BalanceSystem();

    }
}
