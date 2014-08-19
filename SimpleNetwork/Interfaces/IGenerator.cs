using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;

namespace SimpleNetwork.Interfaces
{
    /// <summary>
    /// Power generation abstraction.
    /// </summary>
    public interface IGenerator : IMeasureable
    {
        string Name { get; }
        double GetProduction(int tick);
    }

}
