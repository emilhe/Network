using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;

namespace BusinessLogic.ExportStrategies
{
    class ConSyncScheme : IExportScheme
    {

        // TODO: Discuss implementation with magnus!!

        public bool Measuring
        {
            get { throw new NotImplementedException(); }
        }

        public void Start(int ticks)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Sample(int tick)
        {
            throw new NotImplementedException();
        }

        public List<ITimeSeries> CollectTimeSeries()
        {
            throw new NotImplementedException();
        }

        public void Bind(IList<INode> nodes, double[] mismatches)
        {
            throw new NotImplementedException();
        }

        public void BalanceSystem()
        {
            throw new NotImplementedException();
        }
    }
}
