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
    /// <summary>
    /// Wrapper for sparse time series; makes it possible to get the value at a particular time step.
    /// </summary>
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

        #region Delegation

        public string Name
        {
            get { return _mCore.Name; }
            set { _mCore.Name = value; }
        }

        public int Count
        {
            get { return _mCore.Count; }
        }

        public double GetValue(int tick)
        {
            return ((ITimeSeries) _mCore).GetValue(tick);
        }

        public double GetAverage()
        {
            return ((ITimeSeries) _mCore).GetAverage();
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

        public void SetScale(double scale)
        {
            ((ITimeSeries) _mCore).SetScale(scale);
        }

        public void SetOffset(int ticks)
        {
            ((ITimeSeries) _mCore).SetOffset(ticks);
        }

        #endregion

        #region Enumeration

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            return _mCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _mCore).GetEnumerator();
        }

        #endregion

    }
}
