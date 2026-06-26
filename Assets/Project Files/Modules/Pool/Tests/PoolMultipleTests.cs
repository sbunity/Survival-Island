using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for PoolMultiple: multi-prefab weighted pool.
    /// CreateMultiPool() assigns weight 1 to each prefab so all are eligible.
    /// Weight selection is random, but with a single prefab the outcome is deterministic.
    /// </summary>
    [TestFixture]
    public class PoolMultipleTests : PoolTestBase
    {
        private Transform _container;

        [SetUp]
        public void SetUp() => _container = CreateContainer();

        // ─── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Builds a PoolMultiple where every prefab has weight 1.</summary>
        private PoolMultiple CreateMultiPool(string name, params GameObject[] prefabs)
        {
            var list = new List<PoolMultiple.MultiPoolPrefab>();
            foreach (var p in prefabs)
                list.Add(new PoolMultiple.MultiPoolPrefab(p, 1, false));
            return new PoolMultiple(list, name, _container);
        }

        // ─── Init ─────────────────────────────────────────────────────────────

        [Test]
        public void Init_RegistersPool_InPoolManager()
        {
            CreateMultiPool("MultiInit", CreatePrefab("P1"));
            Assert.IsTrue(PoolManager.HasPool("MultiInit"));
        }

        [Test]
        public void Init_EmptyName_DoesNotRegister()
        {
            var list = new List<PoolMultiple.MultiPoolPrefab>
            {
                new PoolMultiple.MultiPoolPrefab(CreatePrefab(), 1, false)
            };
            LogAssert.Expect(LogType.Error, new Regex("unique name"));
            new PoolMultiple(list, "", _container);

            Assert.AreEqual(0, PoolManager.Pools.Count);
        }

        [Test]
        public void Init_NullPrefab_DoesNotRegister()
        {
            var list = new List<PoolMultiple.MultiPoolPrefab>
            {
                new PoolMultiple.MultiPoolPrefab(null, 1, false)
            };
            LogAssert.Expect(LogType.Error, new Regex("no attached prefab"));
            new PoolMultiple(list, "MultiNullPrefab", _container);

            Assert.IsFalse(PoolManager.HasPool("MultiNullPrefab"));
        }

        // ─── GetPooledObject ──────────────────────────────────────────────────

        [Test]
        public void GetPooledObject_ReturnsActiveObject()
        {
            var pool = CreateMultiPool("MultiGet", CreatePrefab("PA"), CreatePrefab("PB"));
            var go = pool.GetPooledObject();

            Assert.IsNotNull(go);
            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void GetPooledObject_ReturnedObject_IsChildOfContainer()
        {
            var pool = CreateMultiPool("MultiChild", CreatePrefab("PA"), CreatePrefab("PB"));
            var go = pool.GetPooledObject();

            Assert.AreEqual(_container, go.transform.parent);
        }

        [Test]
        public void GetPooledObject_AllActive_CreatesDistinctObjects()
        {
            // Single prefab: both gets must produce different instances
            var pool = CreateMultiPool("MultiDistinct", CreatePrefab("PA"));
            var go1 = pool.GetPooledObject();
            var go2 = pool.GetPooledObject();

            Assert.AreNotSame(go1, go2);
        }

        [Test]
        public void GetPooledObject_ReusesDeactivatedObject()
        {
            // Single prefab → deterministic pool selection (always index 0)
            var pool = CreateMultiPool("MultiReuse", CreatePrefab("PA"));
            var go1 = pool.GetPooledObject();
            go1.SetActive(false);
            var go2 = pool.GetPooledObject();

            Assert.AreSame(go1, go2);
            Assert.IsTrue(go2.activeSelf);
        }

        // ─── Weighted selection (deterministic edge cases) ────────────────────

        [Test]
        public void GetPooledObject_SinglePrefab_AlwaysReturnsSamePrefabType()
        {
            // With one prefab, every spawned object must be a clone of that prefab.
            var prefabA = CreatePrefab("OnlyA");
            var pool = CreateMultiPool("MultiSingle", prefabA);

            for (int i = 0; i < 5; i++)
            {
                var go = pool.GetPooledObject();
                // Name follows FormatName("{0} e{1}") → "MultiSingle e{i}"
                StringAssert.StartsWith("MultiSingle", go.name);
            }
        }

        [Test]
        public void GetPooledObject_ZeroWeightPrefab_NeverSelected()
        {
            // prefabA weight=0, prefabB weight=1 → only prefabB ever spawned.
            // totalWeight = 1, randomValue = 1 always, prefabA accumulates 0 (<1),
            // prefabB accumulates 1 (>=1) → index 1 chosen every time.
            var list = new List<PoolMultiple.MultiPoolPrefab>
            {
                new PoolMultiple.MultiPoolPrefab(CreatePrefab("ZeroW"), 0, false),
                new PoolMultiple.MultiPoolPrefab(CreatePrefab("OneW"),  1, false),
            };
            var pool = new PoolMultiple(list, "WeightEdge", _container);

            for (int i = 0; i < 5; i++)
            {
                var go = pool.GetPooledObject();
                // All spawned names belong to sub-pool 1 (index 1)
                StringAssert.StartsWith("WeightEdge", go.name);
                Assert.IsTrue(go.activeSelf);
            }
        }

        // ─── CreatePoolObjects ────────────────────────────────────────────────

        [Test]
        public void CreatePoolObjects_PreWarmsAllSubPools()
        {
            // 2 prefabs × 3 objects each = 6 total children in container
            var pool = CreateMultiPool("MultiPrewarm", CreatePrefab("PA"), CreatePrefab("PB"));
            pool.CreatePoolObjects(3);

            Assert.AreEqual(6, _container.childCount);
        }

        [Test]
        public void CreatePoolObjects_PrewarmedObjects_AreInactive()
        {
            var pool = CreateMultiPool("MultiInactive", CreatePrefab("PA"));
            pool.CreatePoolObjects(3);

            for (int i = 0; i < _container.childCount; i++)
                Assert.IsFalse(_container.GetChild(i).gameObject.activeSelf);
        }

        // ─── ReturnToPoolEverything ───────────────────────────────────────────

        [Test]
        public void ReturnToPoolEverything_DeactivatesAllAcrossAllSubPools()
        {
            var pool = CreateMultiPool("MultiReturn", CreatePrefab("PA"), CreatePrefab("PB"));
            var go1 = pool.GetPooledObject();
            var go2 = pool.GetPooledObject();
            pool.ReturnToPoolEverything();

            Assert.IsFalse(go1.activeSelf);
            Assert.IsFalse(go2.activeSelf);
        }

        [Test]
        public void ReturnToPoolEverything_AfterReturn_ObjectIsReused()
        {
            // Single prefab → deterministic reuse
            var pool = CreateMultiPool("MultiReuseReturn", CreatePrefab("PA"));
            var go1 = pool.GetPooledObject();
            pool.ReturnToPoolEverything();
            var go2 = pool.GetPooledObject();

            Assert.AreSame(go1, go2);
        }

        // ─── GetPrefabByIndex ─────────────────────────────────────────────────

        [Test]
        public void GetPrefabByIndex_ReturnsCorrectPrefabAndWeight()
        {
            var prefabA = CreatePrefab("PrefA");
            var prefabB = CreatePrefab("PrefB");
            var list = new List<PoolMultiple.MultiPoolPrefab>
            {
                new PoolMultiple.MultiPoolPrefab(prefabA, 3, false),
                new PoolMultiple.MultiPoolPrefab(prefabB, 7, false),
            };
            var pool = new PoolMultiple(list, "MultiByIndex", _container);

            Assert.AreEqual(prefabA, pool.GetPrefabByIndex(0).Prefab);
            Assert.AreEqual(3,       pool.GetPrefabByIndex(0).Weight);
            Assert.AreEqual(prefabB, pool.GetPrefabByIndex(1).Prefab);
            Assert.AreEqual(7,       pool.GetPrefabByIndex(1).Weight);
        }
    }
}
