using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Utils;

namespace BusinessLogic.ExportStrategies
{

    class PhaseAngleFlow
    {

        private readonly Matrix<double> _mKtLplus;

        /// <summary>
        /// Construct phase angle calculator from incidence matrix.
        /// </summary>
        /// <param name="k"> incidence matrix </param>
        public PhaseAngleFlow(Matrix<double> k)
        {
            // F = (K^T*L+)*PHI where L = K*K^T, here we preprocess the front factor.
            _mKtLplus = k.Transpose().Multiply((k.Multiply(k.Transpose())).PseudoInverse());
        }

        /// <summary>
        /// Given an injection pattern, the phase angle flows are calcuated.
        /// </summary>
        /// <param name="injectionPattern"> injection pattern </param>
        /// <returns> flows </returns>
        public Vector<double> CalculateFlows(Vector<double> injectionPattern)
        {
            return _mKtLplus.Multiply(injectionPattern);
        }

        /// <summary>
        /// Given an injection pattern, the phase angle flows are calcuated.
        /// </summary>
        /// <param name="injectionPattern"> injection pattern </param>
        /// <returns> flows </returns>
        public double[] CalculateFlows(double[] injectionPattern)
        {
            return _mKtLplus.Multiply(new DenseVector(injectionPattern)).ToArray();
        } 

    }

}
