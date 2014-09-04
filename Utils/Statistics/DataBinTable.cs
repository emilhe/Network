namespace Utils.Statistics
{
    public class DataBinTable
    {

        public double[] Midpoints { get; private set; }
        public double[] Values { get; private set; }
        public double BinSize { get; private set; }

        public DataBinTable(double[] midpoints, double[] values)
        {
            Midpoints = midpoints;
            Values = values;
            var binSize = midpoints.Length > 1 ? midpoints[1] - midpoints[0] : 1;
            BinSize = (binSize > 0) ? binSize : 1;
        }
    }
}
