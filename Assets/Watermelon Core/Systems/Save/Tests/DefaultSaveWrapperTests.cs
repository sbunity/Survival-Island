using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for DefaultSaveWrapper — file I/O, atomic-replace, and .tmp recovery.
    /// Runs in EditMode; uses Application.persistentDataPath for a temp directory.
    /// Each test uses an isolated filename to prevent cross-test interference.
    /// </summary>
    [TestFixture]
    public class DefaultSaveWrapperTests
    {
        private DefaultSaveWrapper wrapper;
        private string dir;

        // Unique prefix to avoid collisions with actual save files
        private const string PREFIX = "__wm_test__";

        [SetUp]
        public void SetUp()
        {
            wrapper = new DefaultSaveWrapper();
            wrapper.Init();
            dir = Application.persistentDataPath;
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any files written during the test
            foreach (string f in Directory.GetFiles(dir, PREFIX + "*.save"))
                File.Delete(f);
            foreach (string f in Directory.GetFiles(dir, PREFIX + "*.save.tmp"))
                File.Delete(f);
            foreach (string f in Directory.GetFiles(dir, PREFIX + "*.json"))
                File.Delete(f);
            foreach (string f in Directory.GetFiles(dir, PREFIX + "*.json.tmp"))
                File.Delete(f);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private string UniqueFile() => PREFIX + System.Guid.NewGuid().ToString("N");

        private string JsonPath(string name)  => Path.Combine(dir, name + ".save");
        private string TmpPath(string name)   => Path.Combine(dir, name + ".save.tmp");
        private string LegacyJsonPath(string name) => Path.Combine(dir, name + ".json");
        private string LegacyTmpPath(string name)  => Path.Combine(dir, name + ".json.tmp");

        // ─── Load ─────────────────────────────────────────────────────────────

        [Test]
        public void Load_NonExistentFile_ReturnsNewSaveFile()
        {
            var result = wrapper.Load(UniqueFile());
            Assert.IsNotNull(result);
        }

        [Test]
        public void Load_NonExistentFile_SaveCountIsZero()
        {
            var result = wrapper.Load(UniqueFile());
            result.Init();
            Assert.AreEqual(0, result.SaveCount);
        }

        [Test]
        public void Load_ValidJson_ReturnsSaveFileWithData()
        {
            string name = UniqueFile();
            var original = new SaveFile();
            original.Init();
            original.GetSaveObject<SimpleBoolSave>("flag").Value = true;
            original.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(original));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual(1, loaded.SaveCount);
            Assert.IsTrue(loaded.GetSaveObject<SimpleBoolSave>("flag").Value);
        }

        [Test]
        public void Load_CorruptJson_ReturnsNewSaveFile()
        {
            string name = UniqueFile();
            File.WriteAllText(JsonPath(name), "THIS IS NOT JSON {{{{");

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[Save\]: Failed to read save"));
            var loaded = wrapper.Load(name);

            // Corrupt file → fallback to fresh SaveFile
            Assert.IsNotNull(loaded);
        }

        // ─── SaveRaw ──────────────────────────────────────────────────────────

        [Test]
        public void SaveRaw_CreatesJsonFile()
        {
            string name = UniqueFile();
            wrapper.SaveRaw(name, "{}");
            Assert.IsTrue(File.Exists(JsonPath(name)));
        }

        [Test]
        public void SaveRaw_NoTmpFileLeftOver()
        {
            string name = UniqueFile();
            wrapper.SaveRaw(name, "{}");
            Assert.IsFalse(File.Exists(TmpPath(name)));
        }

        [Test]
        public void SaveRaw_SecondWrite_OverwritesContent()
        {
            string name = UniqueFile();
            wrapper.SaveRaw(name, "{\"lastSavedUnix\":1}");
            wrapper.SaveRaw(name, "{\"lastSavedUnix\":2}");

            string content = File.ReadAllText(JsonPath(name));
            StringAssert.Contains("\"lastSavedUnix\":2", content);
            StringAssert.DoesNotContain("\"lastSavedUnix\":1", content);
        }

        [Test]
        public void SaveRaw_SecondWrite_NoTmpFileLeftOver()
        {
            string name = UniqueFile();
            wrapper.SaveRaw(name, "{\"lastSavedUnix\":1}");
            wrapper.SaveRaw(name, "{\"lastSavedUnix\":2}");

            Assert.IsFalse(File.Exists(TmpPath(name)));
        }

        // ─── Round-trip ───────────────────────────────────────────────────────

        [Test]
        public void SaveRaw_ThenLoad_PreservesIntValue()
        {
            string name = UniqueFile();

            var file = new SaveFile();
            file.Init();
            file.GetSaveObject<SimpleIntSave>("score").Value = 777;
            file.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual(777, loaded.GetSaveObject<SimpleIntSave>("score").Value);
        }

        [Test]
        public void SaveRaw_ThenLoad_PreservesFloatValue()
        {
            string name = UniqueFile();

            var file = new SaveFile();
            file.Init();
            file.GetSaveObject<SimpleFloatSave>("volume").Value = 0.75f;
            file.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual(0.75f, loaded.GetSaveObject<SimpleFloatSave>("volume").Value, delta: 0.001f);
        }

        [Test]
        public void SaveRaw_ThenLoad_PreservesStringValue()
        {
            string name = UniqueFile();

            var file = new SaveFile();
            file.Init();
            file.GetSaveObject<SimpleStringSave>("lang").Value = "uk";
            file.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual("uk", loaded.GetSaveObject<SimpleStringSave>("lang").Value);
        }

        [Test]
        public void SaveRaw_ThenLoad_PreservesSaveCount()
        {
            string name = UniqueFile();

            var file = new SaveFile();
            file.Init();
            file.Flush(updateLastSaved: true);
            file.Flush(updateLastSaved: true);  // saveCount = 2
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            Assert.AreEqual(2, loaded.SaveCount);
        }

        [Test]
        public void SaveRaw_ThenLoad_PreservesMultipleContainers()
        {
            string name = UniqueFile();

            var file = new SaveFile();
            file.Init();
            file.GetSaveObject<SimpleIntSave>("a").Value  = 1;
            file.GetSaveObject<SimpleIntSave>("b").Value  = 2;
            file.GetSaveObject<SimpleBoolSave>("c").Value = true;
            file.Flush(updateLastSaved: false);
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual(1,    loaded.GetSaveObject<SimpleIntSave>("a").Value);
            Assert.AreEqual(2,    loaded.GetSaveObject<SimpleIntSave>("b").Value);
            Assert.IsTrue(        loaded.GetSaveObject<SimpleBoolSave>("c").Value);
        }

        // ─── Delete ───────────────────────────────────────────────────────────

        [Test]
        public void Delete_RemovesJsonFile()
        {
            string name = UniqueFile();
            wrapper.SaveRaw(name, "{}");
            Assert.IsTrue(File.Exists(JsonPath(name)));

            wrapper.Delete(name);
            Assert.IsFalse(File.Exists(JsonPath(name)));
        }

        [Test]
        public void Delete_NonExistentFile_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => wrapper.Delete(UniqueFile()));
        }

        [Test]
        public void Delete_AlsoRemovesTmpFile()
        {
            string name = UniqueFile();
            // Manually plant a stale .tmp file
            File.WriteAllText(TmpPath(name), "{}");

            wrapper.Delete(name);

            Assert.IsFalse(File.Exists(TmpPath(name)));
        }

        [Test]
        public void Delete_ThenLoad_DataIsGone()
        {
            string name = UniqueFile();

            // Write data to named file (simulates e.g. "world_1")
            var file = new SaveFile();
            file.Init();
            file.GetSaveObject<SimpleIntSave>("progress").Value = 999;
            file.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            // Confirm data persisted
            var before = wrapper.Load(name);
            before.Init();
            Assert.AreEqual(999, before.GetSaveObject<SimpleIntSave>("progress").Value);

            // Delete the file
            wrapper.Delete(name);

            // Reload → must be a fresh empty file
            var after = wrapper.Load(name);
            after.Init();
            Assert.AreEqual(0, after.GetSaveObject<SimpleIntSave>("progress").Value);
            Assert.AreEqual(0, after.SaveCount);
        }

        [Test]
        public void Delete_ThenSave_StartsFromSaveCountOne()
        {
            string name = UniqueFile();

            // Simulate a world save that accumulated 5 flushes
            var file = new SaveFile();
            file.Init();
            for (int i = 0; i < 5; i++)
                file.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(file));

            // Delete and write a fresh file
            wrapper.Delete(name);

            var fresh = new SaveFile();
            fresh.Init();
            fresh.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(fresh));

            // Reloaded file should have saveCount=1, not carry over the old 5
            var reloaded = wrapper.Load(name);
            Assert.AreEqual(1, reloaded.SaveCount);
        }

        // ─── Legacy .json migration ──────────────────────────────────────────

        [Test]
        public void Load_LegacyJsonExists_ReturnsItsData()
        {
            string name = UniqueFile();

            var file = new SaveFile();
            file.Init();
            file.GetSaveObject<SimpleIntSave>("progress").Value = 42;
            file.Flush(updateLastSaved: true);
            File.WriteAllText(LegacyJsonPath(name), JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual(42, loaded.GetSaveObject<SimpleIntSave>("progress").Value);
        }

        [Test]
        public void Load_LegacyJsonExists_DeletesLegacyFileAfterMigration()
        {
            string name = UniqueFile();
            File.WriteAllText(LegacyJsonPath(name), "{}");

            wrapper.Load(name);

            Assert.IsFalse(File.Exists(LegacyJsonPath(name)));
        }

        [Test]
        public void Load_LegacyJsonExists_NewSaveFileNotCreatedUntilNextSaveRaw()
        {
            string name = UniqueFile();
            File.WriteAllText(LegacyJsonPath(name), "{}");

            wrapper.Load(name);

            Assert.IsFalse(File.Exists(JsonPath(name)));
        }

        [Test]
        public void Load_BothNewAndLegacyExist_PrefersNewFile()
        {
            string name = UniqueFile();

            var legacy = new SaveFile();
            legacy.Init();
            legacy.GetSaveObject<SimpleIntSave>("progress").Value = 1;
            legacy.Flush(updateLastSaved: true);
            File.WriteAllText(LegacyJsonPath(name), JsonUtility.ToJson(legacy));

            var current = new SaveFile();
            current.Init();
            current.GetSaveObject<SimpleIntSave>("progress").Value = 2;
            current.Flush(updateLastSaved: true);
            wrapper.SaveRaw(name, JsonUtility.ToJson(current));

            var loaded = wrapper.Load(name);
            loaded.Init();

            Assert.AreEqual(2, loaded.GetSaveObject<SimpleIntSave>("progress").Value);
        }

        [Test]
        public void Delete_AlsoRemovesLegacyJsonFile()
        {
            string name = UniqueFile();
            File.WriteAllText(LegacyJsonPath(name), "{}");
            File.WriteAllText(LegacyTmpPath(name), "{}");

            wrapper.Delete(name);

            Assert.IsFalse(File.Exists(LegacyJsonPath(name)));
            Assert.IsFalse(File.Exists(LegacyTmpPath(name)));
        }

        // ─── .tmp recovery ────────────────────────────────────────────────────

        [Test]
        public void Load_TmpExistsButJsonMissing_RecoversTmpFile()
        {
            string name = UniqueFile();

            // Simulate an interrupted save: only .tmp was written
            var file = new SaveFile();
            file.Init();
            file.Flush(updateLastSaved: true);  // saveCount = 1

            File.WriteAllText(TmpPath(name), JsonUtility.ToJson(file));

            var loaded = wrapper.Load(name);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.SaveCount);
        }

        [Test]
        public void Load_TmpExistsButJsonMissing_JsonFileCreated()
        {
            string name = UniqueFile();
            File.WriteAllText(TmpPath(name), "{}");

            wrapper.Load(name);

            // After recovery the .json file should now exist
            Assert.IsTrue(File.Exists(JsonPath(name)));
        }

        // ─── UseThreads / Configure ───────────────────────────────────────────

        [Test]
        public void UseThreads_ReturnsTrue()
        {
            Assert.IsTrue(wrapper.UseThreads());
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => wrapper.Configure(new SaveWrapperConfig()));
        }
    }
}
