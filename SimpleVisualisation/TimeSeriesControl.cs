using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DataItems;
using SimpleNetwork.Interfaces;

namespace SimpleVisualisation
{
    public partial class TimeSeriesControl : UserControl
    {

        private Dictionary<string, ITimeSeries> _mTimeSeriesMap;

        public TimeSeriesControl()
        {
            InitializeComponent();
            ChartUtils.EnableZooming(chart);

            seriesListView.ItemSelectionChanged += seriesListView_ItemSelectionChanged;
        }

        public void SetData(List<ITimeSeries> timeSeries)
        {
            _mTimeSeriesMap = timeSeries.ToDictionary(item => item.Name, item => item);

            chart.Series.Clear();
            seriesListView.Items.Clear();

            foreach (var ts in timeSeries)
            {
                seriesListView.Items.Add(new ListViewItem(ts.Name));
            }
        }

        void seriesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            chart.Series.Clear();
            if (seriesListView.SelectedItems.Count == 0) return;

            for (int i = 0; i < seriesListView.SelectedItems.Count; i++)
            {
                AddTimeSeries(_mTimeSeriesMap[seriesListView.SelectedItems[i].Text]);
            }
        }

        private void AddTimeSeries(ITimeSeries ts)
        {
            // Construct new time series.
            var series = new Series(ts.Name);
            foreach (var tsItem in ts)
            {
                series.Points.AddXY(tsItem.TimeStamp, tsItem.Value);
                series.ChartType = SeriesChartType.FastLine;
            }
            chart.Series.Add(series);
        }

    }
}
