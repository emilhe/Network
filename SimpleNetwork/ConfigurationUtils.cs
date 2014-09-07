using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Generators;
using BusinessLogic.Storages;
using SimpleImporter;

namespace BusinessLogic
{
    public class ConfigurationUtils
    {

        private const int Year = 2010;

        public static EdgeSet GetEuropeEdges(List<Node> nodes)
        {
            var result = new EdgeSet(nodes.Count);
            // Create mapping between countryname and index.
            var idxMap = new Dictionary<string, int>();
            for (int i = 0; i < nodes.Count; i++) idxMap.Add(nodes[i].CountryName, i);
            // Connect the countries.
            var ntcData = ProtoStore.LoadNtcData();
            foreach (var row in ntcData)
            {
                if(row.LinkCapacity == 0) continue;
                if (row.CountryFrom.Equals(row.CountryTo)) continue;
                result.AddEdge(idxMap[row.CountryFrom], idxMap[row.CountryTo]); // For now, don't add the capacity.
            }

            return result;
        }

        #region Node setup

        public static List<Node> CreateNodesWithBackup(TsSource source = TsSource.ISET, int years = 1, int offset = 0)
        {
            var client = new AccessClient();
            var nodes = client.GetAllCountryData(source, offset);
            var loads = nodes.ToDictionary( item => item.CountryName, item => item.LoadTimeSeries.GetAverage());
            var loadSum = loads.Values.Sum();

            foreach (var node in nodes)
            {
                var avgLoad = loads[node.CountryName];

                node.StorageCollection.Add(new BatteryStorage(6*avgLoad)); // Should this be in TWh too?
                node.StorageCollection.Add(new HydrogenStorage(25000*avgLoad/loadSum));
                node.StorageCollection.Add(new BasicBackup("Hydro-bio backup", 150000 * avgLoad / loadSum * years));
            }

            return nodes;
        }

        public static List<Node> CreateNodes(TsSource source = TsSource.ISET, bool battery = false)
        {
            var client = new AccessClient();
            var nodes = client.GetAllCountryData(source);

            if (!battery) return nodes;

            foreach (var node in nodes)
            {
                var load = node.LoadTimeSeries;
                var avgLoad = load.GetAverage();

                node.StorageCollection.Add(new BatteryStorage(6*avgLoad)); // Fixed for now                 
            }

            return nodes;
        }

        /// <summary>
        /// Setup the nodes in some default way using the ECN data.
        /// </summary>
        /// <param name="nodes"> the nodes on which the generators are to be added </param>
        /// <param name="data"> the data from which the generators are to be constructed </param>
        public static void SetupHydroStuff(List<Node> nodes, List<EcnDataRow> data, int years = 1)
        {
            // STANDARD
            var loads = nodes.ToDictionary(item => item.CountryName, item => item.LoadTimeSeries.GetAverage());
            var loadSum = loads.Values.Sum();
            foreach (var node in nodes)
            {
                var avgLoad = loads[node.CountryName];

                node.StorageCollection.Add(new BatteryStorage(6 * avgLoad)); // Should this be in TWh too?
                node.StorageCollection.Add(new HydrogenStorage(25000 * avgLoad / loadSum));
            }
            
            // SPECIAL
            var relevantCountries = new[] {"Finland", "Sweden", "Norway"};
            var relevantData = data.Where(item =>
                item.RowHeader.Equals("Hydropower") &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year) && 
                relevantCountries.Contains(item.Country)).ToDictionary(item => item.Country, item => item.Value);
            var sum = relevantData.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if(!relevantCountries.Contains(node.CountryName)) continue;

                node.StorageCollection.Add(new BasicBackup("Storage lakes", (150000*years/sum)*relevantData[node.CountryName]));
            }
        }

        /// <summary>
        /// Setup the nodes in some default way using the ECN data.
        /// </summary>
        /// <param name="nodes"> the nodes on which the generators are to be added </param>
        /// <param name="data"> the data from which the generators are to be constructed </param>
        public static void SetupNodesFromEcnData(List<Node> nodes, List<EcnDataRow> data)
        {
            AddStorages(nodes, data, "Pumped storage hydropower");
            AddBackups(nodes, data, "Biomass");
            AddGenerators(nodes, data, "Hydropower");
        }

        /// <summary>
        /// Add hydro generators to the nodes base on the 
        /// </summary>
        /// <param name="nodes"> the nodes on which the generators are to be added </param>
        /// <param name="data"> the data from which the generators are to be constructed </param>
        /// <param name="type"> which parameter (this method is generic) </param>
        public static void AddGenerators(List<Node> nodes, List<EcnDataRow> data, string type)
        {
            var relevantData = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();

            foreach (var node in nodes)
            {
                var match = relevantData.SingleOrDefault(item => item.Country.Equals(node.CountryName));
                if (match == null) continue;
                // We have a match, let's add the generator.
                node.Generators.Add(new ConstantGenerator(type, match.Value));
            }
        }

        /// <summary>
        /// Add a backup element to the nodes base on the input data.
        /// </summary>
        /// <param name="nodes"> the nodes on which the backups are to be added </param>
        /// <param name="data"> the data from which the backups are to be constructed </param>
        /// <param name="type"> which parameter (this method is generic) </param>
        public static void AddBackups(List<Node> nodes, List<EcnDataRow> data, string type)
        {
            var relevantData = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();

            foreach (var node in nodes)
            {
                var match = relevantData.SingleOrDefault(item => item.Country.Equals(node.CountryName));
                if (match == null) continue;
                // We have a match, let's add the backup.
                node.StorageCollection.Add(new BasicBackup(type, match.Value));
            }
        }

        /// <summary>
        /// Add a storage element to the nodes base on the input data.
        /// </summary>
        /// <param name="nodes"> the nodes on which the storages are to be added </param>
        /// <param name="data"> the data from which the storages are to be constructed </param>
        /// <param name="type"> which parameter (this method is generic) </param>
        /// <param name="efficiency"> the efficiency of the storage (one way) </param>
        public static void AddStorages(List<Node> nodes, List<EcnDataRow> data, string type, double efficiency = 0.9)
        {
            var hydroData = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();

            foreach (var node in nodes)
            {
                var match = hydroData.SingleOrDefault(item => item.Country.Equals(node.CountryName));
                if (match == null) continue;
                // We have a match, let's add the storage.
                node.StorageCollection.Add(new BasicStorage(type, efficiency, match.Value));
            }
        }

        #endregion

    }
}
