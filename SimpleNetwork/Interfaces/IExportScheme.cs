using System.Collections.Generic;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Nodes;

namespace BusinessLogic.Interfaces
{
    public interface IExportScheme : IMeasureable
    {

        void Bind(double[] mismatches);
        void BalanceSystem();

    }
}
