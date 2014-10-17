using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Interfaces;

namespace Controls.Charting
{
    public interface ITimeSeriesView
    {

        Chart MainChart { get; }
        void AddData(ITimeSeries ts);

    }
}
