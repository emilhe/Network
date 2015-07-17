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
        List<IStorage> Storages { get; }
        Measureable Balancing { get; }
        //StorageCollection StorageCollection { get; }
        //ITimeSeries LoadTimeSeries { get; }
    }
}
