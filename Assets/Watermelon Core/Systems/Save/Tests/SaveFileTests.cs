using NUnit.Framework;
using UnityEngine;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for SaveFile and SaveFileContainer — core save data model.
    /// All tests are EditMode (no MonoBehaviour / coroutines required).
    /// </summary>
    [TestFixture]
    public class SaveFileTests
    {
        // ─── Helpers ──────────────────────────────────────────────────────────

        [System.Serializable]
        private class PlayerData : ISaveObject
        {
            public int score;
            public string playerName;
            public void OnBeforeSave() { }
        }

        [System.Serializable]
        private class LevelData : ISaveObject
        {
            public int level;
            public bool completed;
            public void OnBeforeSave() { }
        }

        [System.Serializable]
        [SaveKey("attributed_data_stable_key")]
        private class AttributedData : ISaveObject
        {
            public int value;
            public void OnBeforeSave() { }
        }

        private static SaveFile FreshFile()
        {
            var file = new SaveFile();
            file.Init();
            return file;
        }

        // ─── Init ─────────────────────────────────────────────────────────────

        [Test]
        public void Init_FreshFile_IsNotDirty()
        {
            var file = FreshFile();
            Assert.IsFalse(file.IsDirty);
        }

        [Test]
        public void Init_FreshFile_SaveCountIsZero()
        {
            var file = FreshFile();
            Assert.AreEqual(0, file.SaveCount);
        }

        [Test]
        public void Init_FreshFile_LastSavedAtIsMinValue()
        {
            var file = FreshFile();
            Assert.AreEqual(System.DateTime.MinValue, file.LastSavedAt);
        }

        // ─── GetSaveObject ────────────────────────────────────────────────────

        [Test]
        public void GetSaveObject_NewKey_ReturnsDefaultInstance()
        {
            var obj = FreshFile().GetSaveObject<PlayerData>("player");
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, obj.score);
        }

        [Test]
        public void GetSaveObject_SameKeyTwice_ReturnsSameInstance()
        {
            var file = FreshFile();
            var first = file.GetSaveObject<PlayerData>("player");
            first.score = 42;

            var second = file.GetSaveObject<PlayerData>("player");

            Assert.AreSame(first, second);
            Assert.AreEqual(42, second.score);
        }

        [Test]
        public void GetSaveObject_DifferentKeys_ReturnsDifferentInstances()
        {
            var file = FreshFile();
            var a = file.GetSaveObject<PlayerData>("key.a");
            var b = file.GetSaveObject<PlayerData>("key.b");

            Assert.AreNotSame(a, b);
        }

        [Test]
        public void GetSaveObject_DifferentTypes_SeparateContainers()
        {
            var file = FreshFile();
            var player = file.GetSaveObject<PlayerData>("player");
            var level  = file.GetSaveObject<LevelData>("level");

            player.score = 100;
            level.level  = 5;

            Assert.AreEqual(100, file.GetSaveObject<PlayerData>("player").score);
            Assert.AreEqual(5,   file.GetSaveObject<LevelData>("level").level);
        }

        // ─── [SaveKey] attribute ─────────────────────────────────────────────

        [Test]
        public void GetSaveObject_WithSaveKeyAttribute_UsesAttributeKey()
        {
            var file = FreshFile();
            var byAttr     = file.GetSaveObject<AttributedData>();
            var byExplicit = file.GetSaveObject<AttributedData>("attributed_data_stable_key");

            // Both paths resolve to the same container
            Assert.AreSame(byAttr, byExplicit);
        }

        [Test]
        public void GetSaveObject_WithSaveKeyAttribute_NotSameAsFullNameKey()
        {
            // SaveKey overrides FullName — accessing via FullName is a separate container
            var file = FreshFile();
            var byAttr     = file.GetSaveObject<AttributedData>();
            var byFullName = file.GetSaveObject<AttributedData>(typeof(AttributedData).FullName);

            Assert.AreNotSame(byAttr, byFullName);
        }

        [Test]
        public void GetSaveObject_WithSaveKeyAttribute_DataRoundTrips()
        {
            var file = FreshFile();
            file.GetSaveObject<AttributedData>().value = 99;
            file.Flush(updateLastSaved: false);

            var restored = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(file));
            restored.Init();

            Assert.AreEqual(99, restored.GetSaveObject<AttributedData>().value);
        }

        // ─── Flush ────────────────────────────────────────────────────────────

        [Test]
        public void Flush_WithUpdateLastSaved_IncrementsCount()
        {
            var file = FreshFile();
            file.Flush(updateLastSaved: true);
            Assert.AreEqual(1, file.SaveCount);
        }

        [Test]
        public void Flush_WithoutUpdateLastSaved_DoesNotChangeCount()
        {
            var file = FreshFile();
            file.Flush(updateLastSaved: false);
            Assert.AreEqual(0, file.SaveCount);
            Assert.AreEqual(0, file.LastSavedUnix);
        }

        [Test]
        public void Flush_MultipleTimesWithUpdate_CountAccumulates()
        {
            var file = FreshFile();
            file.Flush(updateLastSaved: true);
            file.Flush(updateLastSaved: true);
            file.Flush(updateLastSaved: true);
            Assert.AreEqual(3, file.SaveCount);
        }

        [Test]
        public void Flush_WithUpdateLastSaved_SetsTimestamp()
        {
            var file = FreshFile();
            long before = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            file.Flush(updateLastSaved: true);
            long after  = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Assert.GreaterOrEqual(file.LastSavedUnix, before);
            Assert.LessOrEqual(file.LastSavedUnix, after);
        }

        [Test]
        public void Flush_ClearsDirtyFlag()
        {
            var file = FreshFile();
            file.MarkAsDirty();
            Assert.IsTrue(file.IsDirty);

            file.Flush(updateLastSaved: false);
            Assert.IsFalse(file.IsDirty);
        }

        // ─── IsDirty / MarkAsDirty ────────────────────────────────────────────

        [Test]
        public void MarkAsDirty_SetsDirtyFlag()
        {
            var file = FreshFile();
            Assert.IsFalse(file.IsDirty);
            file.MarkAsDirty();
            Assert.IsTrue(file.IsDirty);
        }

        // ─── RemoveContainer ─────────────────────────────────────────────────

        [Test]
        public void RemoveContainer_ExistingKey_ObjectIsResetOnNextGet()
        {
            var file = FreshFile();
            var obj = file.GetSaveObject<PlayerData>("player");
            obj.score = 999;

            file.RemoveContainer("player");

            // Requesting the same key after removal creates a fresh default
            var fresh = file.GetSaveObject<PlayerData>("player");
            Assert.AreEqual(0, fresh.score);
        }

        [Test]
        public void RemoveContainer_SetsDirtyFlag()
        {
            var file = FreshFile();
            file.GetSaveObject<PlayerData>("player");
            file.Flush(updateLastSaved: false);
            Assert.IsFalse(file.IsDirty);

            file.RemoveContainer("player");
            Assert.IsTrue(file.IsDirty);
        }

        [Test]
        public void RemoveContainer_NonExistentKey_DoesNotThrow()
        {
            var file = FreshFile();
            Assert.DoesNotThrow(() => file.RemoveContainer("ghost.key"));
        }

        // ─── Clear ───────────────────────────────────────────────────────────

        [Test]
        public void Clear_SetsDirtyFlag()
        {
            var file = FreshFile();
            file.Flush(updateLastSaved: false);
            Assert.IsFalse(file.IsDirty);

            file.Clear();
            Assert.IsTrue(file.IsDirty);
        }

        [Test]
        public void Clear_AllKeysReturnDefaultOnNextGet()
        {
            var file = FreshFile();
            file.GetSaveObject<PlayerData>("p1").score = 10;
            file.GetSaveObject<PlayerData>("p2").score = 20;
            file.Clear();

            Assert.AreEqual(0, file.GetSaveObject<PlayerData>("p1").score);
            Assert.AreEqual(0, file.GetSaveObject<PlayerData>("p2").score);
        }

        // ─── JSON round-trip ─────────────────────────────────────────────────

        [Test]
        public void JsonRoundTrip_PreservesIntField()
        {
            var file = FreshFile();
            file.GetSaveObject<PlayerData>("player").score = 777;
            file.Flush(updateLastSaved: true);

            var restored = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(file));
            restored.Init();

            Assert.AreEqual(777, restored.GetSaveObject<PlayerData>("player").score);
        }

        [Test]
        public void JsonRoundTrip_PreservesStringField()
        {
            var file = FreshFile();
            file.GetSaveObject<PlayerData>("player").playerName = "Hero";
            file.Flush(updateLastSaved: true);

            var restored = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(file));
            restored.Init();

            Assert.AreEqual("Hero", restored.GetSaveObject<PlayerData>("player").playerName);
        }

        [Test]
        public void JsonRoundTrip_PreservesSaveCount()
        {
            var file = FreshFile();
            file.Flush(updateLastSaved: true);
            file.Flush(updateLastSaved: true);

            var restored = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(file));
            restored.Init();

            Assert.AreEqual(2, restored.SaveCount);
        }

        [Test]
        public void JsonRoundTrip_MultipleContainers_AllRestored()
        {
            var file = FreshFile();
            file.GetSaveObject<PlayerData>("player").score = 5;
            file.GetSaveObject<LevelData>("level").level   = 3;
            file.Flush(updateLastSaved: false);

            var restored = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(file));
            restored.Init();

            Assert.AreEqual(5, restored.GetSaveObject<PlayerData>("player").score);
            Assert.AreEqual(3, restored.GetSaveObject<LevelData>("level").level);
        }

        [Test]
        public void JsonRoundTrip_AfterClear_ContainersEmpty()
        {
            var file = FreshFile();
            file.GetSaveObject<PlayerData>("player").score = 99;
            file.Clear();
            file.Flush(updateLastSaved: false);

            var restored = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(file));
            restored.Init();

            // After clear + serialize + restore, key should yield fresh default
            Assert.AreEqual(0, restored.GetSaveObject<PlayerData>("player").score);
        }

        // ─── SaveFileContainer (Flush / Restore) ──────────────────────────────

        [Test]
        public void Container_Constructor_RestoredIsTrue()
        {
            var data      = new PlayerData { score = 1 };
            var container = new SaveFileContainer("key", data);
            Assert.IsTrue(container.Restored);
        }

        [Test]
        public void Container_FlushThenRestore_PreservesData()
        {
            var data      = new PlayerData { score = 42, playerName = "Alice" };
            var container = new SaveFileContainer("key", data);
            container.Flush();

            // Mutate the original — should not affect the stored JSON
            data.score      = 0;
            data.playerName = "changed";

            container.Restore<PlayerData>();
            var restored = (PlayerData)container.SaveObject;
            Assert.AreEqual(42,      restored.score);
            Assert.AreEqual("Alice", restored.playerName);
        }

        [Test]
        public void Container_Key_MatchesConstructorArg()
        {
            var container = new SaveFileContainer("my.unique.key", new PlayerData());
            Assert.AreEqual("my.unique.key", container.Key);
        }
    }
}
