using NUnit.Framework;
using SimpleNetwork.Interfaces;
using SimpleNetwork.Storages;

namespace UnitTest
{
    [TestFixture]
    class StorageTest
    {

        [Test]
        public void ChargeDischargeTest()
        {
            var storage = new BasicStorage("Test", 1, 10);
            // Dischare empty storage.
            Assert.AreEqual(-2, storage.Inject(0, -2));
            Assert.AreEqual(10, storage.RemainingCapacity(Response.Charge));
            Assert.AreEqual(0, storage.RemainingCapacity(Response.Discharge));
            // Charge empty storage.
            Assert.AreEqual(0, storage.Inject(0, 4));
            Assert.AreEqual(6,storage.RemainingCapacity(Response.Charge));
            Assert.AreEqual(-4, storage.RemainingCapacity(Response.Discharge));
            // Discarge non-empty storage.
            Assert.AreEqual(0, storage.Inject(0, -2));
            Assert.AreEqual(8, storage.RemainingCapacity(Response.Charge));
            Assert.AreEqual(-2, storage.RemainingCapacity(Response.Discharge));
            // Overcharge storage.
            Assert.AreEqual(2, storage.Inject(0, 10));
            Assert.AreEqual(0, storage.RemainingCapacity(Response.Charge));
            Assert.AreEqual(-10, storage.RemainingCapacity(Response.Discharge));
        }

        [Test]
        public void RestoreTest()
        {
            var storage = new BasicStorage("Test", 1, 12);
            Assert.AreEqual(0, storage.Restore(0, Response.Discharge));
            Assert.AreEqual(-12, storage.Restore(0, Response.Charge));
            Assert.AreEqual(12, storage.Restore(0, Response.Discharge));
        }

        [Test]
        public void ChargeEfficiencyTest()
        {
            var storage = new BasicStorage("Test", 0.5, 12);
            // Discharge EMPTY storage (should do nothing).
            Assert.AreEqual(-12, storage.Inject(0, -12));
            // Charge/discharge
            Assert.AreEqual(0, storage.Inject(0, 12));
            Assert.AreEqual(12, storage.RemainingCapacity(Response.Charge));
            Assert.AreEqual(-3, storage.RemainingCapacity(Response.Discharge));
            Assert.AreEqual(0, storage.Inject(0, 12));
            // Charge FULL storage (should do nothing).
            Assert.AreEqual(12, storage.Inject(0, 12));
        }

        [Test]
        public void RestoreEfficiencyTest()
        {
            var storage = new BasicStorage("Test", 0.5, 12);
            // Restore
            Assert.AreEqual(0, storage.Restore(0, Response.Discharge));
            Assert.AreEqual(-24, storage.Restore(0, Response.Charge));
            Assert.AreEqual(0, storage.Restore(0, Response.Charge));
            Assert.AreEqual(6, storage.Restore(0, Response.Discharge));
        }
    }
}
