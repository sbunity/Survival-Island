using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for PoolManager registration, lookup, and global operations.
    /// Each test gets a fresh PoolManager MonoBehaviour via PoolTestBase.SetUp.
    /// </summary>
    [TestFixture]
    public class PoolManagerTests : PoolTestBase
    {
        // ─── HasPool ──────────────────────────────────────────────────────────

        [Test]
        public void HasPool_BeforeAnyPool_ReturnsFalse()
        {
            Assert.IsFalse(PoolManager.HasPool("NonExistent"));
        }

        [Test]
        public void HasPool_AfterPoolInit_ReturnsTrue()
        {
            new Pool(CreatePrefab(), "HasPoolTest", CreateContainer());
            Assert.IsTrue(PoolManager.HasPool("HasPoolTest"));
        }

        [Test]
        public void HasPool_AfterDestroyPool_ReturnsFalse()
        {
            var pool = new Pool(CreatePrefab(), "HasPoolDestroy", CreateContainer());
            PoolManager.DestroyPool(pool);
            Assert.IsFalse(PoolManager.HasPool("HasPoolDestroy"));
        }

        // ─── GetPoolByName ────────────────────────────────────────────────────

        [Test]
        public void GetPoolByName_ReturnsCorrectInstance()
        {
            var pool = new Pool(CreatePrefab(), "ByNameTest", CreateContainer());
            Assert.AreSame(pool, PoolManager.GetPoolByName("ByNameTest"));
        }

        [Test]
        public void GetPoolByName_UnknownName_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("Not found pool"));
            Assert.IsNull(PoolManager.GetPoolByName("Missing"));
        }

        // ─── Duplicate name ───────────────────────────────────────────────────

        [Test]
        public void AddPool_DuplicateName_FirstPoolRemains_SecondNotRegistered()
        {
            var pool1 = new Pool(CreatePrefab(), "DupeTest", CreateContainer());
            LogAssert.Expect(LogType.Error, new Regex("already exists"));
            new Pool(CreatePrefab(), "DupeTest", CreateContainer()); // rejected

            Assert.AreSame(pool1, PoolManager.GetPoolByName("DupeTest"));
        }

        [Test]
        public void AddPool_DuplicateName_PoolCountStaysAtOne()
        {
            new Pool(CreatePrefab(), "DupeCount", CreateContainer());
            LogAssert.Expect(LogType.Error, new Regex("already exists"));
            new Pool(CreatePrefab(), "DupeCount", CreateContainer());

            Assert.AreEqual(1, PoolManager.Pools.Count);
        }

        // ─── Pools property ───────────────────────────────────────────────────

        [Test]
        public void Pools_StartsEmpty()
        {
            Assert.AreEqual(0, PoolManager.Pools.Count);
        }

        [Test]
        public void Pools_IncreasesOnRegister()
        {
            new Pool(CreatePrefab(), "PoolsA", CreateContainer());
            new Pool(CreatePrefab(), "PoolsB", CreateContainer());

            Assert.AreEqual(2, PoolManager.Pools.Count);
        }

        [Test]
        public void Pools_DecreasesAfterDestroyPool()
        {
            var pool = new Pool(CreatePrefab(), "PoolsDec", CreateContainer());
            Assert.AreEqual(1, PoolManager.Pools.Count);

            PoolManager.DestroyPool(pool);
            Assert.AreEqual(0, PoolManager.Pools.Count);
        }

        // ─── DestroyPool ──────────────────────────────────────────────────────

        [Test]
        public void DestroyPool_Null_DoesNotThrow()
        {
            LogAssert.Expect(LogType.Error, new Regex("null pool reference"));
            Assert.DoesNotThrow(() => PoolManager.DestroyPool(null));
        }

        // ─── ReturnToPool ─────────────────────────────────────────────────────

        [Test]
        public void ReturnToPool_DeactivatesAllActiveObjectsAcrossAllPools()
        {
            var pool1 = new Pool(CreatePrefab(), "RP1", CreateContainer());
            var pool2 = new Pool(CreatePrefab(), "RP2", CreateContainer());

            var go1 = pool1.GetPooledObject();
            var go2 = pool2.GetPooledObject();

            PoolManager.ReturnToPool();

            Assert.IsFalse(go1.activeSelf);
            Assert.IsFalse(go2.activeSelf);
        }
    }
}
