﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
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

        #region Basic node setup

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

        public static List<Node>CreateNodes(TsSource source = TsSource.ISET, double offset = 0)
        {
            var client = new AccessClient();
            return client.GetAllCountryData(source, (int)(offset * Utils.Utils.HoursInYear));
        }

        #endregion

        #region Storage/backup distribution

        public static void SetupHomoStuff(List<Node> nodes, int years, bool bat, bool storage, bool backup)
        {
            // Standard
            var loads = nodes.ToDictionary(item => item.CountryName, item => item.LoadTimeSeries.GetAverage());
            var loadSum = loads.Values.Sum();
            foreach (var node in nodes)
            {
                var avgLoad = loads[node.CountryName];

                if(bat) node.StorageCollection.Add(new BatteryStorage(6 * avgLoad)); // Should this be in TWh too?
                if (storage) node.StorageCollection.Add(new HydrogenStorage(25000 * avgLoad / loadSum));
                if (backup) node.StorageCollection.Add(new BasicBackup("Hydro-bio backup", 150000 * avgLoad / loadSum * years));
            }
        }

        public static void SetupHeterogeneousBackup(List<Node> nodes, int years)
        {
            // Note: All countries with below 10 TWh of hydro has been neglected. In addition romania has been neglected (primaryly run-of-river).
            var hydroCountries = new Dictionary<string, double>
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
            var sum = hydroCountries.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if (!hydroCountries.Keys.Contains(node.CountryName)) continue;

                node.StorageCollection.Add(new BasicBackup("Hydro reservoir",
                    (150000*years/sum)*hydroCountries[node.CountryName]));
            }
        }

        public static void SetupHeterogeneousStorage(List<Node> nodes, int years)
        {
            var germany = nodes.Single(item => item.CountryName.Equals("Germany"));
            germany.StorageCollection.Add(new HydrogenStorage(25000 * years));
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
