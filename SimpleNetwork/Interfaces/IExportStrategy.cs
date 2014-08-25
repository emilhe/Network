using System.Collections.Generic;
using SimpleNetwork.ExportStrategies;

namespace SimpleNetwork.Interfaces
{
    public interface IExportStrategy
    {

        void Bind(List<Node> nodes, double[] mismatches);
        BalanceResult BalanceSystem(int tick);

    }
}
