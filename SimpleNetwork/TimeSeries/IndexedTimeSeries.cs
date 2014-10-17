using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;

namespace BusinessLogic.TimeSeries
{
    public class IndexedSparseTimeSeries : ITimeSeries
    {

        private readonly SparseTimeSeries _mCore;
        private int[] _mIdx;

        public IndexedSparseTimeSeries(SparseTimeSeries ts)
        {
            _mCore = ts;
            RefreshIndexes();
        }

        public void RefreshIndexes()
        {
            _mIdx = _mCore.GetAllIndices().ToArray();
            Array.Sort(_mIdx);
        }

        public double GetLastValue(int tick)
        {
            var idx = Array.BinarySearch(_mIdx, tick);
            if (idx < 0) idx = ~idx-1;
            return _mCore.GetValue(_mIdx[idx]);
        }

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return _mCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _mCore).GetEnumerator();
        }

        public string Name
        {
            get { return _mCore.Name; }
            set { _mCore.Name = value; }
        }

        public double GetValue(int tick)
        {
            return ((ITimeSeries) _mCore).GetValue(tick);
        }

        public void AddData(int tick, double value)
        {
            ((ITimeSeries) _mCore).AddData(tick, value);
        }

        public void AppendData(double value)
        {
            ((ITimeSeries) _mCore).AppendData(value);
        }

        public double GetAverage()
        {
            return ((ITimeSeries) _mCore).GetAverage();
        }

        public void SetScale(double scale)
        {
            ((ITimeSeries) _mCore).SetScale(scale);
        }

        public List<double> GetAllValues()
        {
            return ((ITimeSeries) _mCore).GetAllValues();
        }

        public Dictionary<string, string> Properties
        {
            get { return _mCore.Properties; }
        }

        public List<string> DisplayProperties
        {
            get { return _mCore.DisplayProperties; }
        }
    }
}
