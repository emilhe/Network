﻿using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Cost;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Generators;
using BusinessLogic.Interfaces;
using BusinessLogic.Nodes;
using BusinessLogic.Storages;
using SimpleImporter;
using Utils;

namespace BusinessLogic.Utils
{
    public class 
        ConfigurationUtils
    {

        private const int Year = 2010;

        //public static EdgeSet GetEuropeEdges(List<CountryNode> nodes)
        //{
        //    return GetEdges(nodes.Select(item => item.Name).ToList(), "NtcMatrix", Double.MaxValue);
        //}

        //public static EdgeSet GetEuropeEdges(List<string> nodes)
        //{
        //    return GetEdges(nodes, "NtcMatrix", Double.MaxValue);
        //}

        //public static EdgeSet GetEdges(List<INode> nodes, string key, double frac)
        //{
        //    return GetEdges(nodes.Select(item => item.Name).ToList(), key, frac);
        //}

        //public static EdgeSet GetEdges(List<string> nodes, string key, double frac)
        //{
        //    var result = new EdgeSet(nodes.Count);
        //    // Create mapping between countryname and index.
        //    var idxMap = new Dictionary<string, int>();
        //    for (int i = 0; i < nodes.Count; i++) idxMap.Add(nodes[i], i);
        //    // Connect the countries.
        //    var ntcData = ProtoStore.LoadLinkData(key);
        //    foreach (var row in ntcData)
        //    {
        //        if (row.From.Equals(row.To)) continue;
        //        // Skip non existing links.
        //        if (row.LinkCapacity < 1) continue;
        //        result.Connect(idxMap[row.From], idxMap[row.To], 1, row.LinkCapacity * frac); // For now, don't add the capacity.
        //    }

        //    return result;
        //}

        public static EdgeCollection GetEdgeObject(CountryNode[] nodes, string key, double frac)
        {
            return GetEdgeObject(nodes.Select(item => item.Name).ToList(), key, frac);
        }

        public static EdgeCollection GetEuropeEdgeObject(CountryNode[] nodes)
        {
            return GetEdgeObject(nodes.Select(item => item.Name).ToList(), "NtcMatrix", 1);
        }

        public static EdgeCollection GetEuropeEdgeObject(List<string> nodes)
        {
            return GetEdgeObject(nodes, "NtcMatrix", 1);
        }

        public static EdgeCollection GetEdgeObject(List<string> nodes, string key, double frac)
        {
            var keys = new List<string>();
            var links = new List<LinkDataRow>();
            foreach (var link in ProtoStore.LoadLinkData(key))
            {
                if (link.LinkCapacity < 1e-3) continue;
                if (link.From.Equals(link.To)) continue;
                if (keys.Contains(link.From + "-" + link.To)) continue;
                if (keys.Contains(link.To + "-" + link.From)) continue;
                link.LinkCapacity *= frac;
                links.Add(link);
                keys.Add(link.From + "-" + link.To);
            }

            return new EdgeCollection(nodes.ToArray(),links);
        }

        #region Basic CountryNode setup

        public static CountryNode[] CreateNodesWithBackup(TsSource source = TsSource.ISET, int years = 1, double offset = 0)
        {
            var nodes = CreateNodes(source, offset);
            SetupHomoStuff(nodes, years, true, true, true);
            return nodes;
        }

        public static CountryNode[] CreateNodes()
        {
            return CreateNodes(TsSource.ISET);
        }

        public static CountryNode[] CreateNodes(TsSource source = TsSource.ISET, double offset = 0)
        {
            return AccessClient.GetAllCountryDataOld(source, (int)(offset * Stuff.HoursInYear));
        }

        public static CountryNode[] CreateNodesNew(double offset = 0, Dictionary<string, double> offshoreFractions = null)
        {
            var nodes = AccessClient.GetAllCountryDataNew(TsSource.VE50PCT, (int)(offset * Stuff.HoursInYear));
            if (offshoreFractions == null) return nodes;

            foreach (var key in offshoreFractions.Keys)
            {
                var match = nodes.SingleOrDefault(item => item.Name.Equals(key));
                if(match == null) continue;

                match.Model.OffshoreFraction = offshoreFractions[key];
            }

            return nodes;
        }

        #endregion

        #region Storage/backup - scaled distributions

        public static void SetupHomoStuff(CountryNode[] nodes, int years, bool bat, bool storage, bool backup, double batCap = 5)
        {
            SetupStuff(nodes, years, bat, storage, backup, LoadScaling(nodes), batCap);
        }

        public static void SetupStuff(CountryNode[] nodes, int years, bool bat, bool storage, bool backup, Dictionary<string, double> scaling, double batCap = 5)
        {
            foreach (var node in nodes)
            {
                var scale = scaling[node.Name];
                var hour = nodes.Select(item => item.Model.AvgLoad).Sum();

                if (bat) node.Storages.Add(new BatteryStorage(batCap * hour * scale, batCap * hour * scale));
                if (storage) node.Storages.Add(new HydrogenStorage(35 * hour * scale, 35* hour * scale));
                if (backup) node.Storages.Add(new BasicBackup("Hydro-bio backup", (150000 * years) * scale));
            }
        }

        public static void SetupOptimalBackup(CountryNode[] nodes, int years)
        {
            var opts = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalOptimalBackupBatteryAndHydrogenWithLinks.txt");

            SetupBackup(nodes, years, opts, "Optimal backup");
        }

        public static void SetupOptimalBackupDelta(CountryNode[] nodes, int years)
        {
            var opts = FileUtils.DictionaryFromFile<string, double>(@"C:\proto\OptimalBatteryHydrogenDelta.txt");

            SetupBackup(nodes, years, opts, "Optimal backup");
        }

        public static void SetupHeterogeneousBackup(CountryNode[] nodes, int years)
        {
            SetupBackup(nodes, years, HeterogeneousBackupScaling(nodes), "Hydro reservoir");
        }

        public static void SetupHeterogeneousStorage(CountryNode[] nodes, int years)
        {
            SetupHydrogenStorage(nodes, years, HeterogeneousStorageScaling(nodes));
        }

        public static void SetupRealHydro(CountryNode[] nodes, bool pump = true)
        {         
            //var data = FileUtils.FromJsonFile<Dictionary<string, HydroInfo>>(@"C:\Users\Emil\Dropbox\Master Thesis\HydroData2005Kies.txt");
            var data = FileUtils.FromJsonFile<Dictionary<string, HydroInfo>>(@"C:\Users\Emil\Dropbox\Master Thesis\HydroDataExtended2005Kies.txt");
            var hydroFractions = new Dictionary<string, double>();
            // Create storages.
            foreach (var node in nodes)
            {
                var match = data.ContainsKey(node.Name)? data[node.Name] : new HydroInfo {InflowPattern = new double[365]};
                if(!pump) match.PumpCapacity = 0;
                //match.Capacity = 1e12;
                //match.ReservoirCapacity = 1e12;
                
                //if (match.InflowPattern.Average()/24 > node.Model.AvgLoad)
                //{
                //    // It is not allowed for a country to have more than 100% production from hydro.
                //    match.InflowPattern = match.InflowPattern.Mult(node.Model.AvgLoad/(match.InflowPattern.Average()/24));
                //}

                var hydro = new HydroReservoirGenerator(match);
                node.Storages.Add(hydro.InverseGenerator);
                node.Storages.Add(hydro.Pump);
                node.Generators.Add(hydro);
                // Adjust the model to take hydro production into account (inflow pattern is in days).
                var hourly = match.InflowPattern.Average()/24.0;
                node.Model.AvgProduction.Add("Hydro",hourly);
                hydroFractions.Add(node.Name, hourly/node.Model.AvgLoad);
            }
            // TODO: CHANGE - THIS IS DANGEROUS!!
            GenePool.HydroFractions = hydroFractions;
        }

        public static void SetupRealBiomass(CountryNode[] nodes)
        {
            const string type = "Biomass";
            var data = ProtoStore.LoadEcnData();
            var energy = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();
            var capacity = data.Where(item =>
            item.RowHeader.Equals(type) &&
            item.ColumnHeader.Equals("Installed capacity") &&
            item.Year.Equals(Year)).ToArray();
            var biomassFractions = new Dictionary<string, double>();
            // Create storages.
            foreach (var node in nodes)
            {
                var eMatch = energy.SingleOrDefault(item => item.Country.Equals(node.Name));
                var cMatch = capacity.SingleOrDefault(item => item.Country.Equals(node.Name));
                if (eMatch == null || cMatch == null)
                {
                    node.Storages.Add(new BasicBackup(type, 0) { Capacity = 0});          
                    continue;
                }
                // We have a match, let's add the backup.
                var hourly = eMatch.Value/(365*24);
                var cap = Math.Max(hourly * 2000, cMatch.Value);
                var gen = new BiomassGenerator(cap, eMatch.Value);
                node.Generators.Add(gen);
                node.Storages.Add(gen.InverseGenerator);
                node.Model.AvgProduction.Add("Biomass",hourly);
                biomassFractions.Add(node.Name, hourly / node.Model.AvgLoad);
            }
            // TODO: CHANGE - THIS IS DANGEROUS!!
            GenePool.BiomassFractions = biomassFractions;
        }

        //public static void SetupAutoBackup(CountryNode[] nodes, double frac)
        //{
        //    var amount = nodes.Select(item => item.Model.AvgLoad).Sum()*frac;
        //    var weights = nodes.Select(item => item.Model.AvgDeficit).ToArray().Norm(amount * 8766);
        //    // Create storages.
        //    for (int i = 0; i < nodes.Length; i++)
        //    {
        //        nodes[i].Generators.Add(new ConstantGenerator("Auto backup", weights[i]));
        //    }
        //}

        // public static void SetupTradeWindHydro(CountryNode[] nodes)
        //{
        //    // Create storages.
        //    foreach (var node in nodes)
        //    {
        //        var found = CountryInfo.Inflow.ContainsKey(node.Name);
        //        if(!found) Console.WriteLine(node.Name);
        //        var res = found ? CountryInfo.ReservoirCapacities[node.Name] : 0;
        //        var cap = found? CountryInfo.HydroCapacities[node.Name] : 0;
        //        var pump = found? CountryInfo.PumpCapacities[node.Name] : 0;
        //        var inflow = found? CountryInfo.Inflow[node.Name] : 0;
        //        var hydro = new HydroReservoirGenerator(res, cap, pump, inflow, null);
        //        node.Storages.Add(hydro.InverseGenerator);
        //        node.Storages.Add(hydro.Pump);
        //    }
        //}

        #region Scalings

        public static Dictionary<string, double> HeterogeneousBackupScaling(CountryNode[] nodes)
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

        public static Dictionary<string, double> HeterogeneousStorageScaling(CountryNode[] nodes)
        {
            return new Dictionary<string, double>{{"Germany", 1.0}};
        }

        public static Dictionary<string, double> LoadScaling(CountryNode[] nodes)
        {
            var results = nodes.ToDictionary(item => item.Name, item => item.Model.AvgLoad);

            var sum = results.Values.Sum();
            foreach (var key in results.Keys.ToArray())
            {
                results[key] = results[key] / sum;
            }

            return results;
        }

        public static Dictionary<string, double> MismatchScaling(CountryNode[] nodes)
        {
            var results = new Dictionary<string, double>(nodes.Length);

            foreach (var node in nodes)
            {
                var negativeMicmathes = 0.0;
                var ticks = (((CountryNode) node)).Model.Count;
                for (int tick = 0; tick < ticks; tick++)
                {
                    var delta = node.GetDelta();
                    if (delta > 0) continue;
                    negativeMicmathes -= node.GetDelta();
                }
                results.Add(node.Name, negativeMicmathes);
            }

            var sum = results.Values.Sum();
            foreach (var key in results.Keys.ToArray())
            {
                results[key] = results[key] / sum;
            }

            return results;
        }

        #endregion

        private static void SetupBackup(CountryNode[] nodes, int years, Dictionary<string, double> scheme, string label)
        {
            var sum = scheme.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if (!scheme.Keys.Contains(node.Name)) continue;

                node.Storages.Add(new BasicBackup(label,
                    (150000*years/sum)*scheme[node.Name]));
            }
        }

        private static void SetupHydrogenStorage(CountryNode[] nodes, int years, Dictionary<string, double> scheme)
        {
            var sum = scheme.Select(item => item.Value).Sum();

            foreach (var node in nodes)
            {
                if (!scheme.Keys.Contains(node.Name)) continue;

                node.Storages.Add(new HydrogenStorage(
                    (25000 * years / sum) * scheme[node.Name]));
            }
        }

        #endregion

        #region Special storage

        public static void SetupMegaStorage(CountryNode[] nodes)
        {
            foreach (var node in nodes)
            {
                node.Storages.Add(new BasicBackup("Backup", 1e9));
            }
        }

        public static void SetupHydroStorage(CountryNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Name.Equals("Norway")) SetupHydro(node, 300, 85000, null, 110000);
                else SetupHydro(node, 0, 0, null, 0);
            }
        }

        private static void SetupHydro(CountryNode countryNode, double generatorCapacity, double resSize, ITimeSeries inflowPattern, double yearlyInflow)
        {
            countryNode.Storages.Add(new SimpleHydroReservoirStorage(resSize, inflowPattern, yearlyInflow) { Capacity = generatorCapacity });
            //// Initial filling level is assumed = 70%
            //var internalReservoir = new BasicStorage("Internal hydro reservoir", 1, resSize, resSize * 0.7)
            //{
            //    Capacity = generatorCapacity
            //};
            //countryNode.Storages.Add(new VirtualStorage(internalReservoir));
            //countryNode.Generators.Add(new HydroReservoirGenerator(yearlyInflow, inflowPattern, internalReservoir));
        }

        #endregion

        #region ECN data

        /// <summary>
        /// Setup the nodes in some default way using the ECN data.
        /// </summary>
        /// <param name="nodes"> the nodes on which the generators are to be added </param>
        /// <param name="data"> the data from which the generators are to be constructed </param>
        public static void SetupNodesFromEcnData(INode[] nodes, List<EcnDataRow> data)
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
        public static void AddGenerators(INode[] nodes, List<EcnDataRow> data, string type)
        {
            var relevantData = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();

            foreach (var node in nodes)
            {
                var match = relevantData.SingleOrDefault(item => item.Country.Equals(node.Name));
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
        public static void AddBackups(INode[] nodes, List<EcnDataRow> data, string type)
        {
            var relevantData = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();

            foreach (var node in nodes)
            {
                var match = relevantData.SingleOrDefault(item => item.Country.Equals(node.Name));
                if (match == null) continue;
                // We have a match, let's add the backup.
                node.Storages.Add(new BasicBackup(type, match.Value));
            }
        }

        /// <summary>
        /// Add a storage element to the nodes base on the input data.
        /// </summary>
        /// <param name="nodes"> the nodes on which the storages are to be added </param>
        /// <param name="data"> the data from which the storages are to be constructed </param>
        /// <param name="type"> which parameter (this method is generic) </param>
        /// <param name="efficiency"> the efficiency of the storage (one way) </param>
        public static void AddStorages(INode[] nodes, List<EcnDataRow> data, string type, double efficiency = 0.9)
        {
            var hydroData = data.Where(item =>
                item.RowHeader.Equals(type) &&
                item.ColumnHeader.Equals("Gross electricity generation") &&
                item.Year.Equals(Year)).ToArray();

            foreach (var node in nodes)
            {
                var match = hydroData.SingleOrDefault(item => item.Country.Equals(node.Name));
                if (match == null) continue;
                // We have a match, let's add the storage.
                node.Storages.Add(new BasicStorage(type, efficiency, match.Value));
            }
        }

        #endregion

    }
}
