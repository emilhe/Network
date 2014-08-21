using System.Collections.Generic;

namespace SimpleNetwork.Interfaces
{
    public interface IExportStrategy
    {

        void Bind(List<Node> nodes, double[] mismatches, double tolerance = 0);
        int TraverseStorageLevels(int tick);

    }
}
