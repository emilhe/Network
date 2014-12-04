using System.Collections.Generic;
using NUnit.Framework;
using Utils.Statistics;

namespace UnitTest
{
    class UtilsTest
    {

        [Test]
        public void PercentileTest()
        {
            var data = new List<double>();
            for (int i = 0; i < 1000; i++) data.Add(i);

            Assert.AreEqual(5,MathUtils.Percentile(data, 0.5));
            Assert.AreEqual(995, MathUtils.Percentile(data, 99.5));
        }

    }
}
