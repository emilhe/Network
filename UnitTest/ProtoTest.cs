using NUnit.Framework;
using SimpleImporter;

namespace UnitTest
{
    class ProtoTest
    {

        [Test]
        public void ProtoArrayConversionTest()
        {
            var data = new[,,]
            {
                {{1,2},{3,4},{5,6}},
                {{7,8},{9,10},{11,12}},
                {{13,14},{15,16},{17,18}}
            };

            var proto = data.ToProtoArray<int>();
            var data2 = proto.ToArray();

            Assert.AreEqual(data, data2);
        }

        [Test]
        public void ProtoArrayTest()
        {
            var data = new[,]
            {
                {true,false,true},
                {false,true,false},
                {true,false,true}
            };

            ProtoStore.SaveGridResult(data, new[] { 1.0, 2, 3 }, new[] { 1.0, 2, 3 }, "ProtoArrayTest");
            var data2 = ProtoStore.LoadGridResult("ProtoArrayTest").Grid.ToArray();

            Assert.AreEqual(data, data2);
        }

    }
}
