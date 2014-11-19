using NUnit.Framework;
using BusinessLogic.Interfaces;
using BusinessLogic.Storages;

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
            Assert.AreEqual(-2, storage.Inject(-2));
            Assert.AreEqual(10, storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(0, storage.RemainingEnergy(Response.Discharge));
            // Charge empty storage.
            Assert.AreEqual(0, storage.Inject(4));
            Assert.AreEqual(6,storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(-4, storage.RemainingEnergy(Response.Discharge));
            // Discarge non-empty storage.
            Assert.AreEqual(0, storage.Inject(-2));
            Assert.AreEqual(8, storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(-2, storage.RemainingEnergy(Response.Discharge));
            // Overcharge storage.
            Assert.AreEqual(2, storage.Inject(10));
            Assert.AreEqual(0, storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(-10, storage.RemainingEnergy(Response.Discharge));
        }

        [Test]
        public void RestoreTest()
        {
            var storage = new BasicStorage("Test", 1, 12);
            Assert.AreEqual(0, storage.InjectMax(Response.Discharge));
            Assert.AreEqual(-12, storage.InjectMax(Response.Charge));
            Assert.AreEqual(12, storage.InjectMax(Response.Discharge));
        }

        [Test]
        public void ChargeEfficiencyTest()
        {
            var storage = new BasicStorage("Test", 0.5, 12);
            // Discharge EMPTY storage (should do nothing).
            Assert.AreEqual(-12, storage.Inject(-12));
            // Charge/discharge
            Assert.AreEqual(0, storage.Inject(12));
            Assert.AreEqual(12, storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(-3, storage.RemainingEnergy(Response.Discharge));
            Assert.AreEqual(0, storage.Inject(12));
            // Charge FULL storage (should do nothing).
            Assert.AreEqual(12, storage.Inject(12));
        }

        [Test]
        public void RestoreEfficiencyTest()
        {
            var storage = new BasicStorage("Test", 0.5, 12);
            // InjectMax
            Assert.AreEqual(0, storage.InjectMax(Response.Discharge));
            Assert.AreEqual(-24, storage.InjectMax(Response.Charge));
            Assert.AreEqual(0, storage.InjectMax(Response.Charge));
            Assert.AreEqual(6, storage.InjectMax(Response.Discharge));
        }

        [Test]
        public void CompositeStorageTest()
        {
            var master = new BasicStorage("Master", 1, 10);
            var composite = new CompositeStorage(master);
            var slave = new BasicStorage("Slave", 1, 10);
            composite.AddStorage(slave);
            // InjectMax
            Assert.AreEqual(20, composite.NominalEnergy);
            Assert.AreEqual(1, composite.Efficiency);
            Assert.AreEqual(0, composite.InitialEnergy);
        }

        [Test]
        public void ChargeLimitTest()
        {
            var storage = new BasicStorage("Test", 1, 12) { Capacity = 8 };
            // InjectMax
            Assert.AreEqual(12, storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(16, storage.Inject(24));
            Assert.AreEqual(4, storage.RemainingEnergy(Response.Charge));
        }

        [Test]
        public void DischargeLimitTest()
        {
            var storage = new BasicStorage("Test", 1, 12, 12) { Capacity = 3 };
            // InjectMax
            Assert.AreEqual(-12, storage.RemainingEnergy(Response.Discharge));
            Assert.AreEqual(-3, storage.Inject(-6));
            Assert.AreEqual(-9, storage.RemainingEnergy(Response.Discharge));
        }

        [Test]
        public void ChargeLimitEfficiencyTest()
        {
            var storage = new BasicStorage("Test", 0.5, 12) {Capacity = 12};
            // InjectMax
            Assert.AreEqual(24, storage.RemainingEnergy(Response.Charge));
            Assert.AreEqual(12, storage.Inject(24));
            Assert.AreEqual(12, storage.RemainingEnergy(Response.Charge));
        }

        [Test]
        public void DischargeLimitEfficiencyTest()
        {
            var storage = new BasicStorage("Test", 0.5, 12, 12) {Capacity = 3};
            // InjectMax
            Assert.AreEqual(-6, storage.RemainingEnergy(Response.Discharge));
            Assert.AreEqual(-3, storage.Inject(-6));
            Assert.AreEqual(-3, storage.RemainingEnergy(Response.Discharge));
        }

    }
}
