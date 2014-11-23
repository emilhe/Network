using System.Collections.Generic;
using BusinessLogic.Storages;

namespace BusinessLogic.Interfaces
{
    public interface INode : IMeasureable, ITickListener 
    {
        string Name { get; }
        string Abbreviation { get; }

        double GetDelta();

        StorageCollection StorageCollection { get; }
        List<IGenerator> Generators { get; }
        //ITimeSeries LoadTimeSeries { get; }
    }
}
