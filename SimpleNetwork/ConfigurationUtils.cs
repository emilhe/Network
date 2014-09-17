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

        public static List<Node> CreateNodesWithBackup(TsSource source = TsSource.ISET, double years = 1, double offset = 0)
        {
            var nodes = CreateNodes(source, offset);
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

        public static List<Node> CreateNodes(TsSource source = TsSource.ISET, double offset = 0)
        {
            var client = new AccessClient();
            return client.GetAllCountryData(source, (int)(offset * Utils.Utils.HoursInYear));
        }

        public static void SetupHydroStuff(List<Node> nodes, int years)
        {
            // Standard
            var loads = nodes.ToDictionary(item => item.CountryName, item => item.LoadTimeSeries.GetAverage());
            var loadSum = loads.Values.Sum();
            foreach (var node in nodes)
            {
                var avgLoad = loads[node.CountryName];

                node.StorageCollection.Add(new BatteryStorage(6 * avgLoad)); // Should this be in TWh too?
                node.StorageCollection.Add(new HydrogenStorage(25000 * avgLoad / loadSum));
            }

            // Note: All countries with below 10 TWh of hydro has been neglected. In addition romania has been neglected (primaryly run-of-river).
            var hydroCountries = new Dictionary<string, double>
            {
                // Data source: "Nord Pool, http://www.nordpoolspot.com/Market-data1/Power-system-data/Hydro-Reservoir/Hydro-Reservoir/ALL/Hourly/".
                {"Norway", 82.244},
                {"Sweden", 33.675},
                {"Finland", 5.530},
                // Data source: "The impact of global change on the hydropower potential of Europe: a model-based analysis". 
                // The total production is scaled by the resoir capacity (simple approach)
                {"Austria", 42.2/(5.5 + 5.4)*5.4},
                {"France", 66.9/(10.8 + 11.6 + 1.9)*11.6},
                {"Germany", 23.6/(2.7 + 1.4 + 4.2)*1.4},
                {"Italy", 50.3/(8.2 + 7.4 + 4.2)*7.4},
                {"Portugal", 11.6/(2.1 + 2.1)*2.1},
                {"Spain", 31.4/(6.1 + 7.7 + 2.5)*7.7},
                {"Switzerland", 37.8/(4.0 + 9.5 + 0.3)*9.5},
                {"Bosnia", 13.2/(1.9 + 2.0 + 0.7)*2.0}
            };
            var sum = hydroCountries.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if(!hydroCountries.Keys.Contains(node.CountryName)) continue;

                node.StorageCollection.Add(new BasicBackup("Hydro reservoir", (150000*years/sum)*hydroCountries[node.CountryName]));
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
