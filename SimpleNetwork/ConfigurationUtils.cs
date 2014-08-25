﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleImporter;
using SimpleNetwork.Generators;
using SimpleNetwork.Interfaces;
using SimpleNetwork.Storages;
using SimpleNetwork.TimeSeries;

namespace SimpleNetwork
{
    public class ConfigurationUtils
    {

        private const int Year = 2010;

        public static EdgeSet GetEuropeEdges(List<Node> nodes)
        {
            var result = new EdgeSet(nodes.Count);

            // TODO: Draw edges...

            return result;
        }

        #region Node setup

        public static List<Node> CreateNodesWithBackup(TsSource source = TsSource.ISET)
        {
            var client = new AccessClient();
            var nodes = client.GetAllCountryData(source);

            foreach (var node in nodes)
            {
                var load = node.LoadTimeSeries;
                var avgLoad = load.GetAverage();

                node.StorageCollection.Add(new BatteryStorage(6*avgLoad)); // Fixed for now
                node.StorageCollection.Add(new HydrogenStorage(68.18*avgLoad));
                    //  25TWh*(6hourLoad/2.2TWh) = 68.18; To be country dependent
                node.StorageCollection.Add(new BasicBackup("Hydro-bio backup", 409.09*avgLoad));
                    // 150TWh*(6hourLoad/2.2TWh) = 409.09; To be country dependent           
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
        public static void SetupNodesFromEcnData(List<Node> nodes, List<EcnDataRow> data)
        {
            AddStorages(nodes, data, "Pumped storage hydropower");
            AddBackups(nodes, data, "Hydropower");
            AddBackups(nodes, data, "Biomass");
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
        public static void AddStorages(List<Node> nodes, List<EcnDataRow> data, string type, double efficiency = 0.6)
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
