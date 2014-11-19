using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Xml.Linq;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;
using SimpleImporter;
using Utils;

namespace BusinessLogic
{
    public class ConfigurationUtils
    {

        private const int Year = 2010;

        public static EdgeSet GetEuropeEdges(List<Node> nodes)
        {
           return GetEdges(nodes, "NtcMatrix", 1);
        }

        public static EdgeSet GetEdges(List<Node> nodes, string key, double frac)
        {
            var result = new EdgeSet(nodes.Count);
            // Create mapping between countryname and index.
            var idxMap = new Dictionary<string, int>();
            for (int i = 0; i < nodes.Count; i++) idxMap.Add(nodes[i].CountryName, i);
            // Connect the countries.
            var ntcData = ProtoStore.LoadLinkData(key);
            foreach (var row in ntcData)
            {
                if (row.CountryFrom.Equals(row.CountryTo)) continue;
                // Skip non existing links.
                if (row.LinkCapacity < 1) continue;
                result.Connect(idxMap[row.CountryFrom], idxMap[row.CountryTo], 1, row.LinkCapacity * frac); // For now, don't add the capacity.
            }

            return result;
        }

        #region Basic node setup

        public static List<Node> CreateNodesWithBackup(TsSource source = TsSource.ISET, int years = 1, double offset = 0)
        {
            var nodes = CreateNodes(source, offset);
            SetupHomoStuff(nodes, years, true, true, true);
            return nodes;
        }

        public static List<Node> CreateNodes(TsSource source = TsSource.ISET, double offset = 0)
        {
            var client = new AccessClient();
            return client.GetAllCountryData(source, (int) (offset*Utils.Utils.HoursInYear));
        }

        #endregion

        #region Storage/backup - scaled distributions

        public static void SetupHomoStuff(List<Node> nodes, int years, bool bat, bool storage, bool backup)
        {
            SetupStuff(nodes, years, bat, storage, backup, LoadScaling(nodes));
        }

        public static void SetupStuff(List<Node> nodes, int years, bool bat, bool storage, bool backup, Dictionary<string, double> scaling)
        {
            foreach (var node in nodes)
            {
                var scale = scaling[node.CountryName];

                if (bat) node.StorageCollection.Add(new BatteryStorage(2200 * scale)); 
                if (storage) node.StorageCollection.Add(new HydrogenStorage(25000 * scale));
                if (backup) node.StorageCollection.Add(new BasicBackup("Hydro-bio backup", (150000 * years) * scale));
            }
        }

                public static void SetupOptimalBackup(List<Node> nodes, int years)
        {
            var opts = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalOptimalBackupBatteryAndHydrogenWithLinks.txt");

            SetupBackup(nodes, years, opts, "Optimal backup");
        }

        public static void SetupOptimalBackupDelta(List<Node> nodes, int years)
        {
            var opts = Parsing.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogenDelta.txt");

            SetupBackup(nodes, years, opts, "Optimal backup");
        }

        public static void SetupHeterogeneousBackup(List<Node> nodes, int years)
        {
            SetupBackup(nodes, years, HeterogeneousBackupScaling(nodes), "Hydro reservoir");
        }

        public static void SetupHeterogeneousStorage(List<Node> nodes, int years)
        {
            SetupHydrogenStorage(nodes, years, HeterogeneousStorageScaling(nodes));
        }

        #region Scalings

        public static Dictionary<string, double> HeterogeneousBackupScaling(List<Node> nodes)
        {
            // Note: All countries with below 10 TWh of hydro has been neglected. In addition romania has been neglected (primaryly run-of-river).
            return new Dictionary<string, double>
            {
                // Data source: "Nord Pool, http://www.nordpoolspot.com/Market-data1/Power-system-data/Hydro-Reservoir/Hydro-Reservoir/ALL/Hourly/".
                {"Norway", 82.244},
                {"Sweden", 33.675},
                {"Finland", 5.530},
                // Data source: "Feix 2000"
                {"Austria", 3.2},
                {"France", 9.8},
                {"Germany", 0.3},
                {"Greece", 2.4},
                {"Italy", 7.9},
                {"Portugal", 2.6},
                {"Spain", 18.4},
                {"Switzerland", 8.4},
                //{"Bosnia", 0.0}
            };
        } 

        public static Dictionary<string, double> HeterogeneousStorageScaling(List<Node> nodes)
        {
            return new Dictionary<string, double>{{"Germany", 1.0}};
        } 

        public static Dictionary<string, double> LoadScaling(List<Node> nodes)
        {
            var results = nodes.ToDictionary(item => item.CountryName, item => item.LoadTimeSeries.GetAverage());

            var sum = results.Values.Sum();
            foreach (var key in results.Keys.ToArray())
            {
                results[key] = results[key] / sum;
            }

            return results;
        }

        public static Dictionary<string, double> MismatchScaling(List<Node> nodes)
        {
            var results = new Dictionary<string, double>(nodes.Count);

            foreach (var node in nodes)
            {
                var negativeMicmathes = 0.0;
                var ticks = node.LoadTimeSeries.Count();
                for (int tick = 0; tick < ticks; tick++)
                {
                    var delta = node.GetDelta();
                    if (delta > 0) continue;
                    negativeMicmathes -= node.GetDelta();
                }
                results.Add(node.CountryName, negativeMicmathes);
            }

            var sum = results.Values.Sum();
            foreach (var key in results.Keys.ToArray())
            {
                results[key] = results[key] / sum;
            }

            return results;
        }

        #endregion

        private static void SetupBackup(List<Node> nodes, int years, Dictionary<string, double> scheme, string label)
        {
            var sum = scheme.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if (!scheme.Keys.Contains(node.CountryName)) continue;

                node.StorageCollection.Add(new BasicBackup(label,
                    (150000*years/sum)*scheme[node.CountryName]));
            }
        }

        private static void SetupHydrogenStorage(List<Node> nodes, int years, Dictionary<string, double> scheme)
        {
            var sum = scheme.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if (!scheme.Keys.Contains(node.CountryName)) continue;

                node.StorageCollection.Add(new HydrogenStorage(
                    (25000 * years / sum) * scheme[node.CountryName]));
            }
        }

        #endregion

        #region Special storage

        public static void SetupMegaStorage(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                node.StorageCollection.Add(new BasicBackup("Backup", node.LoadTimeSeries.GetAllValues().Sum()));
            }
        }

        public static void SetupHydroStorage(List<Node> nodes)
        {
            
        }

        private static void SetupHydro(Node node, double generatorCapacity, double resSize, ITimeSeries inflowPattern, double yearlyInflow)
        {
            // Initial filling level is assumed = 70%
            var internalReservoir = new BasicStorage("Hydro reservoir", 1, resSize, resSize * 0.7)
            {
                Capacity = generatorCapacity
            };
            node.StorageCollection.Add(new HydroReservoirStorage(internalReservoir));
            node.Generators.Add(new HydroReservoirGenerator(yearlyInflow, inflowPattern, internalReservoir));
            // TODO: Consider adding a pumped hydro storage too...
        }

        #endregion

        #region ECN data

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
