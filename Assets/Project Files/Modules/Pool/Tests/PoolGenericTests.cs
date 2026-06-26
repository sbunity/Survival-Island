using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for PoolGeneric&lt;T&gt;: typed component retrieval, lifecycle, and reuse.
    /// Uses TestBehaviour (defined in PoolTestBase) as the component type.
    /// </summary>
    [TestFixture]
    public class PoolGenericTests : PoolTestBase
    {
        private Transform _container;

        [SetUp]
        public void SetUp() => _container = CreateContainer();

        // ─── Init ─────────────────────────────────────────────────────────────

        [Test]
        public void Init_RegistersPool_InPoolManager()
        {
            new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenInit", _container);
            Assert.IsTrue(PoolManager.HasPool("GenInit"));
        }

        [Test]
        public void Init_MissingComponent_DoesNotRegister()
        {
            LogAssert.Expect(LogType.Error, new Regex("no attached component"));
            new PoolGeneric<TestBehaviour>(CreatePrefab("NoBehaviour"), "GenMissing", _container);
            Assert.IsFalse(PoolManager.HasPool("GenMissing"));
        }

        [Test]
        public void Init_NullPrefab_DoesNotRegister()
        {
            LogAssert.Expect(LogType.Error, new Regex("no attached prefab"));
            new PoolGeneric<TestBehaviour>(null, "GenNullPrefab", _container);
            Assert.IsFalse(PoolManager.HasPool("GenNullPrefab"));
        }

        // ─── GetPooledObject ──────────────────────────────────────────────────

        [Test]
        public void GetPooledObject_ReturnsActiveObject()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenGetObj", _container);
            var go = pool.GetPooledObject();

            Assert.IsNotNull(go);
            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void GetPooledObject_ReturnedObject_HasRequiredComponent()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenHasComp", _container);
            var go = pool.GetPooledObject();

            Assert.IsNotNull(go.GetComponent<TestBehaviour>());
        }

        [Test]
        public void GetPooledObject_ReusesDeactivatedObject()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenReuse", _container);
            var go1 = pool.GetPooledObject();
            go1.SetActive(false);
            var go2 = pool.GetPooledObject();

            Assert.AreSame(go1, go2);
        }

        [Test]
        public void GetPooledObject_AllActive_CreatesDistinctObject()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenDistinct", _container);
            var go1 = pool.GetPooledObject();
            var go2 = pool.GetPooledObject();

            Assert.AreNotSame(go1, go2);
        }

        // ─── GetPooledComponent ───────────────────────────────────────────────

        [Test]
        public void GetPooledComponent_ReturnsTypedComponent()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenTyped", _container);
            var comp = pool.GetPooledComponent();

            Assert.IsNotNull(comp);
            Assert.IsInstanceOf<TestBehaviour>(comp);
        }

        [Test]
        public void GetPooledComponent_ComponentIsOnActiveObject()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenActive", _container);
            var comp = pool.GetPooledComponent();

            Assert.IsTrue(comp.gameObject.activeSelf);
        }

        [Test]
        public void GetPooledComponent_ReusesDeactivatedComponent()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenCompReuse", _container);
            var comp1 = pool.GetPooledComponent();
            comp1.gameObject.SetActive(false);
            var comp2 = pool.GetPooledComponent();

            Assert.AreSame(comp1, comp2);
        }

        // ─── CreatePoolObjects ────────────────────────────────────────────────

        [Test]
        public void CreatePoolObjects_PreWarmsCorrectCount()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenPrewarm", _container);
            pool.CreatePoolObjects(4);

            Assert.AreEqual(4, _container.childCount);
        }

        [Test]
        public void CreatePoolObjects_AllPrewarmed_HaveRequiredComponent()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenCompPrewarm", _container);
            pool.CreatePoolObjects(3);

            for (int i = 0; i < _container.childCount; i++)
                Assert.IsNotNull(_container.GetChild(i).GetComponent<TestBehaviour>());
        }

        // ─── ReturnToPoolEverything ───────────────────────────────────────────

        [Test]
        public void ReturnToPoolEverything_DeactivatesAll()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenReturn", _container);
            var go1 = pool.GetPooledObject();
            var go2 = pool.GetPooledObject();
            pool.ReturnToPoolEverything();

            Assert.IsFalse(go1.activeSelf);
            Assert.IsFalse(go2.activeSelf);
        }

        [Test]
        public void ReturnToPoolEverything_AfterReturn_ComponentIsReused()
        {
            var pool = new PoolGeneric<TestBehaviour>(CreatePrefab<TestBehaviour>(), "GenCompReturn", _container);
            var comp1 = pool.GetPooledComponent();
            pool.ReturnToPoolEverything();
            var comp2 = pool.GetPooledComponent();

            Assert.AreSame(comp1, comp2);
        }
    }
}
