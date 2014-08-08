using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using SimpleNetwork.Interfaces;

namespace SimpleNetwork
{
    public class NodeCollection
    {

        private readonly List<Tuple<DenseTimeSeries, DenseTimeSeries, double>> _mProductionTuples;
        private readonly List<IStorage> _mStorages;
        private double _mMixing;
        private double _mPenetration;

        public List<Node> Nodes { get; set; }

        public double Mixing
        {
            get { return _mMixing; }
            set
            {
                _mMixing = value;
                UpdateTuples();
            }
        }

        public double Penetration
        {
            get { return _mPenetration; }
            set
            {
                _mPenetration = value;
                UpdateTuples();
            }
        }

        /// <summary>
        /// Construction. More/different constuctors to be added...
        /// </summary>
        /// <param name="data"> data to build nodes from </param>
        public NodeCollection(List<CountryData> data)
        {
            Nodes = new List<Node>(data.Count);
            _mProductionTuples = new List<Tuple<DenseTimeSeries, DenseTimeSeries, double>>(data.Count);
            _mStorages = new List<IStorage>(data.Count);

            foreach (var country in data)
            {
                var load = country.TimeSeries.Single(item => item.Name.Equals("L"));
                var node = new Node(country.Abbreviation, load);
                var avgLoad = load.GetAverage();
                var wind = country.TimeSeries.Single(item => item.Name.Equals("Gw"));
                var solar = country.TimeSeries.Single(item => item.Name.Equals("Gs"));
                _mProductionTuples.Add(new Tuple<DenseTimeSeries, DenseTimeSeries, double>(wind, solar, avgLoad));

                node.PowerGenerators = new List<IGenerator>
                        {
                            new TsGenerator("WindPower", wind),
                            new TsGenerator("SolarPower", solar)
                        };
                node.Storages = new Dictionary<int, IStorage>
                {
                    {0, new BatteryStorage(6*avgLoad)},
                    {1, new HydrogenStorage(50*avgLoad)},
                    {2, new BiomassBackup(300*avgLoad)}
                };
                _mStorages.AddRange(node.Storages.Values);

                Nodes.Add(node);
            }
        }

        private void UpdateTuples()
        {
            foreach (var tuple in _mProductionTuples)
            {
                tuple.Item1.SetScale(_mMixing * _mPenetration * tuple.Item3);
                tuple.Item2.SetScale((1 - _mMixing) * _mPenetration * tuple.Item3);
            }
        }

    }
}
