using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for the Pool class: initialization, object retrieval, reuse, pre-warming,
    /// global return, and post-destroy re-init.
    /// </summary>
    [TestFixture]
    public class PoolTests : PoolTestBase
    {
        private Transform _container;

        [SetUp]
        public void SetUp() => _container = CreateContainer();

        // ─── Init ─────────────────────────────────────────────────────────────

        [Test]
        public void Init_RegistersPool_InPoolManager()
        {
            new Pool(CreatePrefab(), "InitOk", _container);
            Assert.IsTrue(PoolManager.HasPool("InitOk"));
        }

        [Test]
        public void Init_EmptyName_DoesNotRegister()
        {
            LogAssert.Expect(LogType.Error, new Regex("unique name"));
            new Pool(CreatePrefab(), "", _container);
            Assert.AreEqual(0, PoolManager.Pools.Count);
        }

        [Test]
        public void Init_NullPrefab_DoesNotRegister()
        {
            LogAssert.Expect(LogType.Error, new Regex("no attached prefab"));
            new Pool(null, "NullPrefab", _container);
            Assert.IsFalse(PoolManager.HasPool("NullPrefab"));
        }

        [Test]
        public void Init_CalledTwice_IsIdempotent()
        {
            var pool = new Pool(CreatePrefab(), "IdempotentInit", _container);
            pool.Init(); // second call — must be a no-op
            Assert.AreEqual(1, PoolManager.Pools.Count);
        }

        // ─── GetPooledObject ──────────────────────────────────────────────────

        [Test]
        public void GetPooledObject_EmptyPool_ReturnsNotNull()
        {
            var pool = new Pool(CreatePrefab(), "GetNotNull", _container);
            Assert.IsNotNull(pool.GetPooledObject());
        }

        [Test]
        public void GetPooledObject_EmptyPool_ReturnsActiveObject()
        {
            var pool = new Pool(CreatePrefab(), "GetActive", _container);
            Assert.IsTrue(pool.GetPooledObject().activeSelf);
        }

        [Test]
        public void GetPooledObject_AllActive_CreatesDistinctObject()
        {
            var pool = new Pool(CreatePrefab(), "GetDistinct", _container);
            var go1 = pool.GetPooledObject();
            var go2 = pool.GetPooledObject();

            Assert.IsNotNull(go2);
            Assert.AreNotSame(go1, go2);
        }

        [Test]
        public void GetPooledObject_ReusesDeactivatedObject()
        {
            var pool = new Pool(CreatePrefab(), "Reuse", _container);
            var go1 = pool.GetPooledObject();
            go1.SetActive(false);
            var go2 = pool.GetPooledObject();

            Assert.AreSame(go1, go2);
        }

        [Test]
        public void GetPooledObject_ReusedObject_IsActive()
        {
            var pool = new Pool(CreatePrefab(), "ReuseActive", _container);
            var go1 = pool.GetPooledObject();
            go1.SetActive(false);
            var go2 = pool.GetPooledObject();

            Assert.IsTrue(go2.activeSelf);
        }

        [Test]
        public void GetPooledObject_ReturnedObject_IsChildOfContainer()
        {
            var pool = new Pool(CreatePrefab(), "ChildOf", _container);
            var go = pool.GetPooledObject();

            Assert.AreEqual(_container, go.transform.parent);
        }

        [Test]
        public void GetPooledObject_ReturnedObject_HasFormattedName()
        {
            // PoolManager.FormatName("{0} e{1}", name, index) → "NamedPool e0"
            var pool = new Pool(CreatePrefab(), "NamedPool", _container);
            var go = pool.GetPooledObject();

            Assert.AreEqual("NamedPool e0", go.name);
        }

        // ─── GetPooledComponent ───────────────────────────────────────────────

        [Test]
        public void GetPooledComponent_ReturnsRequestedComponent()
        {
            var pool = new Pool(CreatePrefab<TestBehaviour>(), "CompPool", _container);
            var comp = pool.GetPooledComponent<TestBehaviour>();

            Assert.IsNotNull(comp);
            Assert.IsInstanceOf<TestBehaviour>(comp);
        }

        // ─── CreatePoolObjects ────────────────────────────────────────────────

        [Test]
        public void CreatePoolObjects_InactivatesPrewarmedObjects()
        {
            var pool = new Pool(CreatePrefab(), "Prewarm", _container);
            pool.CreatePoolObjects(4);

            // All 4 should be inactive children of the container
            Assert.AreEqual(4, _container.childCount);
            for (int i = 0; i < _container.childCount; i++)
                Assert.IsFalse(_container.GetChild(i).gameObject.activeSelf);
        }

        [Test]
        public void CreatePoolObjects_SmallerCount_DoesNotShrink()
        {
            var pool = new Pool(CreatePrefab(), "NoShrink", _container);
            pool.CreatePoolObjects(3);
            pool.CreatePoolObjects(1); // already have 3

            Assert.AreEqual(3, _container.childCount);
        }

        [Test]
        public void CreatePoolObjects_PrewarmedObjects_AreReused_NotCreatedAgain()
        {
            var pool = new Pool(CreatePrefab(), "ReusePrewarm", _container);
            pool.CreatePoolObjects(3);

            var go = pool.GetPooledObject(); // should reuse existing
            Assert.AreEqual(3, _container.childCount); // no new object created
            Assert.IsTrue(go.activeSelf);
        }

        // ─── ReturnToPoolEverything ───────────────────────────────────────────

        [Test]
        public void ReturnToPoolEverything_DeactivatesAllActive()
        {
            var pool = new Pool(CreatePrefab(), "Return", _container);
            var go1 = pool.GetPooledObject();
            var go2 = pool.GetPooledObject();
            pool.ReturnToPoolEverything();

            Assert.IsFalse(go1.activeSelf);
            Assert.IsFalse(go2.activeSelf);
        }

        [Test]
        public void ReturnToPoolEverything_AfterReturn_NextGet_ReusesObject()
        {
            var pool = new Pool(CreatePrefab(), "ReturnReuse", _container);
            var go1 = pool.GetPooledObject();
            pool.ReturnToPoolEverything();
            var go2 = pool.GetPooledObject();

            Assert.AreSame(go1, go2);
            Assert.IsTrue(go2.activeSelf);
        }

        // ─── DestroyPool + re-init ────────────────────────────────────────────

        [Test]
        public void AfterDestroyPool_HasPool_ReturnsFalse()
        {
            var pool = new Pool(CreatePrefab(), "AfterDestroy", _container);
            PoolManager.DestroyPool(pool);

            Assert.IsFalse(PoolManager.HasPool("AfterDestroy"));
        }

        [Test]
        public void AfterDestroyPool_Init_ReRegistersPool()
        {
            var pool = new Pool(CreatePrefab(), "Reinit", _container);
            PoolManager.DestroyPool(pool);
            pool.Init(); // pool is unregistered, so Init() can register again

            Assert.IsTrue(PoolManager.HasPool("Reinit"));
        }
    }
}
