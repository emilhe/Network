//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusinessLogic.Interfaces;

//namespace BusinessLogic.Storages
//{
//    public class HydroModel : IStorage, IGenerator
//    {

//        private BasicStorage _mReservoir;

//        // Current iteration.
//        private double _mReservoirLevel;
 
//        // Fixed properties.
//        private double _mReservoirStartLevel = 0.7;

//        // Model properties.
//        private readonly double _mReservoirSize;
//        private readonly double _mYearlyInflow;
//        private readonly double _mPumpCapacity;
//        private readonly double _mProdCapacity;
//        private readonly ITimeSeries _mInflowPattern;

//        public HydroModel(double capacity, double reservoir, double inflow, double pump, ITimeSeries inflowTimeSeries)
//        {
//            _mReservoir = new BasicStorage("Reservoir", 1, reservoir, reservoir*0.7);

//            _mProdCapacity = capacity;
//            //_mReservoirSize = reservoir;
//            _mYearlyInflow = inflow;
//            _mPumpCapacity = pump;
//            _mInflowPattern = inflowTimeSeries;
//        }

//        public bool Measurering
//        {
//            get { return _mReservoir.Measurering; }
//        }

//        public ITimeSeries TimeSeries
//        {
//            get { return _mReservoir.TimeSeries; }
//        }

//        public void StartMeasurement()
//        {
//            ((IMeasureableLeaf)_mReservoir).StartMeasurement();
//        }

//        public void Reset()
//        {
//            ((IMeasureableLeaf)_mReservoir).Reset();
//        }

//        public string Name
//        {
//            get { return "Hydro model"; }
//        }

//        public double GetProduction(int tick)
//        {
//            return Math.Min(_mProdCapacity, _mReservoirLevel);
//        }

//        public double Efficiency
//        {
//            get { return _mReservoir.Efficiency; }
//        }

//        public double InitialCapacity
//        {
//            get { return _mReservoir.InitialCapacity; }
//        }

//        public double Capacity
//        {
//            get { return _mReservoir.Capacity; }
//        }

//        public double Inject(int tick, double amount)
//        {
//            return ((IStorage)_mCore).Inject(tick, amount);
//        }

//        public double Restore(int tick, Response response)
//        {
//            return ((IStorage)_mCore).Restore(tick, response);
//        }

//        public double RemainingCapacity(Response response)
//        {
//            return ((IStorage)_mCore).RemainingCapacity(response);
//        }

//        public void ResetCapacity()
//        {
//            ((IStorage)_mCore).ResetCapacity();
//        }

//    }
//}
