using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using BusinessLogic;
using BusinessLogic.Utils;

namespace UnitTest
{
    [TestFixture]
    class GridEvaluatorTest
    {

        private bool[,] _mGrid;
        private int[] _mDims;
        private int _mNumberOfEvals;

        [TestFixtureSetUp]
        public void Init()
        {
            //_mGrid = new[,]
            //{
            //    {false, false, false, true},
            //    {false, false, true, true},
            //    {false, true, true, true},
            //    {true, true, true, true},
            //    {true, true, true, true},
            //    {false, true, true, true},
            //    {false, false, true, true},
            //    {false, false, false, true}
            //};

            _mGrid = new[,]
            {
                {false, false, false, true, true, false, false , false},
                {false, false, true, true, true, true, false , false},
                {false, true, true, true, true, true, true , false},
                {true, true, true, true, true, true, true , true},
            };

            _mDims = new[]{_mGrid.GetLength(0), _mGrid.GetLength(1)};
        }

        private bool ReadGrid(int[] idxs)
        {
            _mNumberOfEvals++;
            return _mGrid[idxs[0], idxs[1]];
        }

        [Test]
        public void DenseGridEvalTest()
        {
            _mNumberOfEvals = 0;
            var grid = GridEvaluator.EvalDense(ReadGrid, _mDims);
            Assert.AreEqual(_mGrid, grid);
            Assert.AreEqual(_mNumberOfEvals, grid.Length);
        }

        [Test]
        public void SparseGridEvalTest()
        {
            _mNumberOfEvals = 0;
            var grid = GridEvaluator.EvalSparse(ReadGrid, _mDims);
            Assert.AreEqual(_mGrid, grid);
            Assert.AreEqual(_mNumberOfEvals, 17); // Less than grid.Length
        }

    }
}
