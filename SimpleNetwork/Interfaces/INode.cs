using System.Collections.Generic;
using BusinessLogic.Storages;
using BusinessLogic.TimeSeries;

namespace BusinessLogic.Interfaces
{
    public interface INode : IMeasureable, ITickListener 
    {
        string Name { get; }
        string Abbreviation { get; }

        double GetDelta();
        double Curtailment { get; }
        double Backup { get; }

        List<IGenerator> Generators { get; }
        StorageCollection StorageCollection { get; }
        Measureable Balancing { get; }
        //ITimeSeries LoadTimeSeries { get; }
    }
}
