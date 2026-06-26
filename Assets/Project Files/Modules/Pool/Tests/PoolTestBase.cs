using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Watermelon.Tests
{
    /// <summary>
    /// Base class for all Pool module tests.
    /// Creates a real PoolManager MonoBehaviour in [SetUp] and destroys it in [TearDown].
    /// Always pass an explicit container to pool constructors to avoid the #if UNITY_EDITOR
    /// default-container path; containers are tracked and destroyed alongside their children.
    /// </summary>
    public abstract class PoolTestBase
    {
        private GameObject _poolManagerGO;
        private readonly List<GameObject> _ownedObjects = new();

        [SetUp]
        public void PoolBaseSetUp()
        {
            _poolManagerGO = new GameObject("PoolManager");
            // EditMode does not trigger Awake, so Init() must be called explicitly
            // (same pattern as TweenTestBase → tweenGO.AddComponent<Tween>().Init(...))
            _poolManagerGO.AddComponent<PoolManager>().Init();
        }

        [TearDown]
        public void PoolBaseTearDown()
        {
            // Cache before destroying PoolManager (instance → null after OnDestroy)
            Transform defaultContainer = PoolManager.DefaultContainer;

            // Destroy tracked objects — containers bring down all pooled children with them
            foreach (GameObject go in _ownedObjects)
                if (go != null)
                    Object.DestroyImmediate(go);
            _ownedObjects.Clear();

            // Destroy PoolManager (triggers OnDestroy → instance = null)
            if (_poolManagerGO != null)
                Object.DestroyImmediate(_poolManagerGO);

            // Destroy the default container spawned by GetContainer() in Editor mode
            if (defaultContainer != null)
                Object.DestroyImmediate(defaultContainer.gameObject);
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Creates a plain prefab GameObject tracked for TearDown cleanup.</summary>
        protected GameObject CreatePrefab(string name = "TestPrefab")
        {
            var go = new GameObject(name);
            _ownedObjects.Add(go);
            return go;
        }

        /// <summary>Creates a prefab with <typeparamref name="T"/> component, tracked for cleanup.</summary>
        protected GameObject CreatePrefab<T>(string name = "TestPrefab") where T : Component
        {
            var go = CreatePrefab(name);
            go.AddComponent<T>();
            return go;
        }

        /// <summary>
        /// Creates a container Transform tracked for TearDown cleanup.
        /// All pool objects instantiated as children are destroyed with this container.
        /// </summary>
        protected Transform CreateContainer(string name = "PoolContainer")
        {
            var go = new GameObject(name);
            _ownedObjects.Add(go);
            return go.transform;
        }
    }

    // ─── Shared test component ─────────────────────────────────────────────────

    /// <summary>Minimal MonoBehaviour used as a typed component in PoolGeneric tests.</summary>
    internal class TestBehaviour : MonoBehaviour { }
}
