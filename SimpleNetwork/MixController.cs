using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using SimpleImporter;
using BusinessLogic.Utils;
using Utils;

namespace BusinessLogic
{
    public class MixController
    {

        private readonly List<Tuple<ITimeSeries, ITimeSeries, double>> _mProductionTuples;
        
        public double[] Mixes { get; set; }
        public double[] Penetrations { get; set; }

        /// <summary>
        /// Object for controlling mix and penetration for a list of nodes.
        /// </summary>
        /// <param name="nodes"></param>
        public MixController(List<Node> nodes)
        {
            _mProductionTuples = new List<Tuple<ITimeSeries, ITimeSeries, double>>(nodes.Count);
            Mixes = new double[nodes.Count];
            Penetrations = new double[nodes.Count];

            foreach (var node in nodes)
            {
                var load = node.LoadTimeSeries;
                var avgLoad = load.GetAverage();
                var wind = ((TimeSeriesGenerator)node.Generators.Single(item => item.Name.Equals(TsType.Wind.GetDescription()))).UnderlyingTimeSeries;
                var solar = ((TimeSeriesGenerator)node.Generators.Single(item => item.Name.Equals(TsType.Solar.GetDescription()))).UnderlyingTimeSeries;
                _mProductionTuples.Add(new Tuple<ITimeSeries, ITimeSeries, double>(wind, solar, avgLoad));
            }

            // Default values.
            for (int i = 0; i < nodes.Count; i++)
            {
                Mixes[i] = 0.5;
                Penetrations[i] = 1;
            }
        }

        /// <summary>
        /// Edit all mixes at once; changes are NOT applied.
        /// </summary>
        public void SetMix(double mix)
        {
            for (int i = 0; i < Mixes.Length; i++) Mixes[i] = mix;
        }

        /// <summary>
        /// Edit all penetrations at once; changes are NOT applied.
        /// </summary>
        public void SetPenetration(double penetration)
        {
            for (int i = 0; i < Penetrations.Length; i++) Penetrations[i] = penetration;
        }

        /// <summary>
        /// Applies the mixes/penetrations.
        /// </summary>
        public void Execute()
        {
            for (int index = 0; index < _mProductionTuples.Count; index++)
            {
                _mProductionTuples[index].Item1.SetScale(Mixes[index] * Penetrations[index] * _mProductionTuples[index].Item3);
                _mProductionTuples[index].Item2.SetScale((1 - Mixes[index]) * Penetrations[index] * _mProductionTuples[index].Item3);
            }
        }

    }
}
