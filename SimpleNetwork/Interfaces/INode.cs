using System.Collections.Generic;
using BusinessLogic.Storages;

namespace BusinessLogic.Interfaces
{
    public interface INode : IMeasureable, ITickListener 
    {
        string Name { get; }
        string Abbreviation { get; }

        double GetDelta();
        double Curtailment { get; }
        double Backup { get; }

        StorageCollection StorageCollection { get; }
        List<IGenerator> Generators { get; }
        BalancingStorage Balancing { get; }
        //ITimeSeries LoadTimeSeries { get; }
    }
}
