using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;

namespace BusinessLogic.TimeSeries
{
    internal class SuperTimeSeries : ITimeSeries
    {

        public string Name { get; set; }

        public List<ITimeSeries> Children { get; private set; }

        public SuperTimeSeries(List<ITimeSeries> children)
        {
            Children = children;
        }

        public int Count { get { return Children.Select(item => item.Count).Max(); } }

        public double GetValue(int tick)
        {
            return Children.Sum(child => child.GetValue(tick));
        }

        #region Unsupported operations

        public IEnumerator<ITimeSeriesItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void AddData(int tick, double value)
        {
            throw new NotImplementedException();
        }

        public void AppendData(double value)
        {
            throw new NotImplementedException();
        }

        public double GetAverage()
        {
            throw new NotImplementedException();
        }

        public void SetScale(double scale)
        {
            throw new NotImplementedException();
        }

        public List<double> GetAllValues()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> Properties { get; private set; }
        public List<string> DisplayProperties { get; private set; }

        #endregion

    }
}
