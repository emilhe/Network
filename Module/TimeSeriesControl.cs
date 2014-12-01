using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusinessLogic.Simulation;
using Controls.Charting;
using BusinessLogic;
using BusinessLogic.Interfaces;
using Utils;

namespace Controls
{
    public partial class TimeSeriesControl : UserControl
    {

        public ITimeSeriesView MainView { get { return _mView; } } 

        private Dictionary<string, ITimeSeries> _mTimeSeriesMap;
        private ITimeSeriesView _mView;

        public TimeSeriesControl()
        {
            InitializeComponent();

            _mView = new TimeSeriesView();
            BindView();

            seriesListView.ItemSelectionChanged += seriesListView_ItemSelectionChanged;
            foreach (TimeSeriesViews value in Enum.GetValues(typeof(TimeSeriesViews)))
                cbxView.Items.Add(value.GetDescription());

            // Hack : http://stackoverflow.com/questions/2309046/making-list-view-scroll-in-vertical-direction
            var header = new ColumnHeader { Text = "", Name = "col1", Width = seriesListView.Width-25};
            seriesListView.Columns.Add(header);
        }

        private void BindView()
        {
            var view = _mView as Control;
            if(view == null) throw new ArgumentException("Invalid view.");

            view.Dock = DockStyle.Fill;
            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(view);
        }

        public void SetData(SimulationOutput output)
        {
            SetData(output.TimeSeries);
        }

        public void SetData(List<ITimeSeries> timeSeries)
        {
            _mTimeSeriesMap = timeSeries.ToDictionary(item => item.Name, item => item);

            _mView.MainChart.Series.Clear();
            seriesListView.Items.Clear();

            foreach (var ts in timeSeries)
            {
                seriesListView.Items.Add(new ListViewItem(ts.Name));
            }
        }

        private void seriesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            _mView.MainChart.Series.Clear();
            if (seriesListView.SelectedItems.Count == 0) return;

            UpdateView();
        }

        private void cbxView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxView.SelectedIndex.Equals((byte)TimeSeriesViews.TimeSeriesView)) _mView = new TimeSeriesView();
            if (cbxView.SelectedIndex.Equals((byte)TimeSeriesViews.HistrogramView)) _mView = new HistogramView();
            BindView();
            UpdateView();
        }

        private void UpdateView()
        {
            for (int i = 0; i < seriesListView.SelectedItems.Count; i++)
            {
                _mView.AddData(_mTimeSeriesMap[seriesListView.SelectedItems[i].Text]);
            }
        }

        private enum TimeSeriesViews : byte
        {
            [Description("Default")] TimeSeriesView = 0,
            [Description("Histogram")] HistrogramView = 1
        }

    }
}
