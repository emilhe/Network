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
                if (InvalidCost) throw new ArgumentException("Cost is not evaluated.");
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
            //CheckLimits();
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
            //CheckLimits();
        }

        public NodeVec(double[] vector) 
        {
            _mVector = vector;
            InvalidCost = true;
            //CheckLimits();
        }

        //private void CheckLimits()
        //{
        //    if (!Validate(_mVector)) throw new ArgumentException("Non renormalizeable vector");
        //}

        public void UpdateCost(Func<ISolution, double> eval)
        {
            if (!InvalidCost) return;
            Cost = Validate(_mVector) ? eval(this) : 100.0;
            InvalidCost = false;
        }

        public NodeVec Add(double[] vec, double weight)
        {
            var guess = _mVector.Copy().Add(vec, weight);
            while (!Validate(guess))
            {
                weight = weight/2.0;
                guess = _mVector.Copy().Add(vec, weight);
            }
            return new NodeVec(guess);
        }

        public double[] Delta(NodeVec other)
        {
            return _mVector.Copy().Add(other._mVector, -1);
        }

        public double[] GetVectorCopy()
        {
            return _mVector.Copy();
        }

        private static bool Validate(double[] vector)
        {
            var n = Labels.Count;
            // Enforce alpha constraints.
            for (int i = n; i < 2 * n; i++)
            {
                if (vector[i] < GenePool.AlphaMin) vector[i] = GenePool.AlphaMin;
                if (vector[i] > GenePool.AlphaMax) vector[i] = GenePool.AlphaMax;
            }
            // Try to do renormalization.
            return GenePool.Renormalize(vector);
        }

    }
}
