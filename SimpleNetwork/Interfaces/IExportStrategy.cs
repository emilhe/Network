using System.Collections.Generic;

namespace SimpleNetwork.Interfaces
{
    public interface IExportStrategy
    {

        void Bind(List<Node> nodes, double[] mismatches, double tolerance = 0);
        double TraverseStorageLevels(int tick);

    }
}
