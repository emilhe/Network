using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{

    public class BetaWrapper
    {
        public double Beta { get; set; }
        public double K { get; set; }
        public double[] MaxCfX { get; set; }
        public double[] MaxCfY { get; set; }
        public double[] BetaX { get; set; }
        public double[] BetaY { get; set; }
        public double GeneticX { get; set; }
        public double GeneticY { get; set; }
        public string Note { get; set; }

        //public double CustomX { get; set; }
        //public double CustomY { get; set; }
        //public double[] CustomXs { get; set; }
        //public double[] CustomYs { get; set; }   

        public string BetaLabel
        {
            get { return string.Format("β = {0}, Beta {1}", (K == -1) ? @"∞" : Beta.ToString("0.00"), Note); }
        }

        public string BetaLabelK
        {
            get { return string.Format("K = {0}, Beta {1}", (K == -1) ? @"∞" : K.ToString(), Note); }
        }

        public string LabelK
        {
            get { return string.Format("K = {0}, {1}", (K == -1) ? @"∞" : K.ToString(), Note); }
        }

        public string GeneticLabel
        {
            get { return string.Format("K = {0}, GA {1}", (K == -1) ? @"∞" : K.ToString(), Note); }
        }

    }

    public class CostWrapper
    {

        public List<string> Labels { get; set; }
        public Dictionary<string, List<double>> Costs { get; set; } 

    }
}
