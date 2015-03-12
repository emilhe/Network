using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using Newtonsoft.Json.Linq;
using SimpleImporter;

namespace BusinessLogic.ExportStrategies
{
    public class EdgeCollection
    {

        public int NodeCount { get { return _mNodes.Length; } }

        public Matrix<double> IncidenceMatrix { get; private set; }
        public List<LinkDataRow> Links { get; private set; }

        private Matrix<double> _mLaplacianMatrix { get; set; }
        private Matrix<double> _mCapacityMatrix { get; set; }

        private readonly string[] _mNodes;

        public EdgeCollection(string[] nodes, List<LinkDataRow> links)
        {
            Links = links;
            _mNodes = nodes;

            IncidenceMatrix = BuildIncidenceMatrix(link => 1);
            _mLaplacianMatrix = IncidenceMatrix.Multiply(IncidenceMatrix.Transpose());
            _mCapacityMatrix = IncidenceMatrix.Multiply(BuildIncidenceMatrix(link => link.LinkCapacity).Transpose());
        }

        Matrix<double> BuildIncidenceMatrix(Func<LinkDataRow, double> weightFunc)
        {
            var matrix = new DenseMatrix(_mNodes.Length, Links.Count);

            for (int i = 0; i < Links.Count; i++)
            {
                // Check of the link already exists.
                var link = Links[i];
                // Set column.
                var col = new double[_mNodes.Length];
                for (int j = 0; j < _mNodes.Length; j++)
                {
                    if (link.From.Equals(_mNodes[j])) col[j] = 1*weightFunc(link);
                    if (link.To.Equals(_mNodes[j])) col[j] = -1 * weightFunc(link);
                }
                matrix.SetColumn(i, col);
            }

            return matrix;
        }

        #region Revise these methods!!

        // Is this correct?
        public bool Connected(int i, int j)
        {
            if (i == j) return false;
            return _mLaplacianMatrix[i, j] != 0;
        }

        // Is this correct?
        public bool EdgeExists(int i, int j)
        {
            if (i > j) return false;
            return Connected(i, j);
        }

        // All costs are 1 at the moment.
        public double GetEdgeCost(int i, int j)
        {
            if (i == j) throw new ArgumentException("Edge cost undefined.");
            return Math.Abs(_mLaplacianMatrix[i, j]);
        }

        // Capacity is currently a one-way-value.
        public double GetEdgeCapacity(int i, int j)
        {
            if (i == j) throw new ArgumentException("Edge capacity undefined.");
            return Math.Abs(_mCapacityMatrix[i, j]);
        }

        #endregion

    }

    public class EdgeBuilder
    {
        
        private readonly string[] _mNodes;
        private readonly List<LinkDataRow> _mLinks;

        public EdgeBuilder(string[] nodes)
        {
            _mNodes = nodes;
            _mLinks = new List<LinkDataRow>();;
        }

        public void Connect(int from, int to)
        {
            _mLinks.Add(new LinkDataRow
            {
                From = _mNodes[from],
                To = _mNodes[to],
                LinkCapacity = double.MaxValue
            });
        }

        public EdgeCollection ToEdges()
        {
            return new EdgeCollection(_mNodes, _mLinks);
        }

    }

}
