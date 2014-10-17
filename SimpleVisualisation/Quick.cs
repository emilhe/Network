using System.Windows.Forms;
using Controls.Charting;
using BusinessLogic.Interfaces;

namespace Main
{
    public class Quick
    {

        public static void Hist(ITimeSeries ts, IWin32Window owner = null)
        {
            var form = new Form();
            var view = new HistogramView();
            view.AddData(ts);
            view.Dock = DockStyle.Fill;
            form.Controls.Add(view);
            form.Show(owner);
        }

        public static void Plot(ITimeSeries ts, IWin32Window owner = null)
        {
            var form = new Form();
            var view = new TimeSeriesView();
            view.AddData(ts);
            view.Dock = DockStyle.Fill;
            form.Controls.Add(view);
            form.Show(owner);
        }

    }
}
