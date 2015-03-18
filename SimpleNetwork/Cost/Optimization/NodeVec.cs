using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Optimization;
using Utils;

namespace BusinessLogic.Cost.Optimization
{
    public class NodeVec : IVectorSolution<NodeVec>
    {

        private double _mCost;
        private readonly double[] _mVector;

        public static List<string> Labels = CountryInfo.GetCountries();
        public static int LabelMultiplicity = 2;

        public double Cost
        {
            get
            {
                if (InvalidCost) throw new ArgumentException("Cost is not updated.");
                return _mCost;
            }
            // Should ONLY be set internally or by JSON deserializer.
            set
            {
                _mCost = value;
                InvalidCost = false;
            }
        }

        public bool InvalidCost { get; set; }

        public double this[int index] 
        {
            get { return _mVector[index]; }
        }

        public int Length
        {
            get { return _mVector.Length; }
        }

        public NodeVec()
        {
            _mVector = new double[Labels.Count * LabelMultiplicity];
            InvalidCost = true;
            EnforceLimits();
        }

        public NodeVec(Func<double> gamma, Func<double> alpha)
        {
            _mVector = new double[Labels.Count * LabelMultiplicity];
            for (int i = 0; i < Labels.Count; i++)
            {
                _mVector[i] = gamma();
                _mVector[Labels.Count + i] = alpha();
            }
            InvalidCost = true;
            EnforceLimits();
        }

        public NodeVec(double[] vector) 
        {
            _mVector = vector;
            InvalidCost = true;
            EnforceLimits();
        }

        private void EnforceLimits()
        {
            if (GenePool.Renormalize(_mVector)) return;
            // If the solution is renormalizeable, cost is infinite.
            Cost = double.MaxValue;            
            InvalidCost = false;
        }

        public void UpdateCost(Func<ISolution, double> eval)
        {
            Cost = eval(this);
        }

        public NodeVec Add(NodeVec other, double weight)
        {
            return new NodeVec(_mVector.Copy().Add(other._mVector, weight));
        }

        public NodeVec Sub(NodeVec other)
        {
            return Add(other, -1);
        }

        public double[] GetVectorCopy()
        {
            return _mVector.Copy();
        }


    }
}
