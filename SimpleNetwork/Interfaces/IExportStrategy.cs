using System.Collections.Generic;

namespace SimpleNetwork.Interfaces
{
    public interface IExportStrategy
    {

        void Bind(List<Node> nodes, double[] mismatches);
        void BalanceSystem(int tick);
        double Tolerance { get; }

    }
}
