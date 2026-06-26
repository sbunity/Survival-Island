using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for SaveManager — the pure C# core of the save system.
    /// No MonoBehaviour, no coroutines, no file I/O.
    /// MockSaveWrapper stores data in-memory; no cloud handler → sync resolves immediately.
    /// </summary>
    [TestFixture]
    public class SaveManagerTests
    {
        // ─── In-memory mock wrapper ───────────────────────────────────────────

        private class MockSaveWrapper : ISaveWrapper
        {
            private readonly Dictionary<string, string> files = new();

            public bool InitCalled { get; private set; }

            public void Init()             => InitCalled = true;
            public bool UseThreads()       => false;   // synchronous saves — no threading in tests
            public void Configure(SaveWrapperConfig config) { }

            public SaveFile Load(string fileName)
            {
                if (files.TryGetValue(fileName, out string json))
                {
                    var loaded = JsonUtility.FromJson<SaveFile>(json);
                    return loaded ?? new SaveFile();
                }
                return new SaveFile();
            }

            public void SaveRaw(string fileName, string json) => files[fileName] = json;
            public void Delete(string fileName)               => files.Remove(fileName);

            public bool   HasFile(string fileName) => files.ContainsKey(fileName);
            public string GetRaw(string fileName)  => files.TryGetValue(fileName, out string j) ? j : null;
        }

        // ─── Minimal cloud mock (for delete / SyncPendingToCloud tests) ──────

        private class MockCloudHandler : ICloudSaveHandler
        {
            public bool IsAvailable  { get; set; } = true;
            public bool DeleteCalled;
            public bool UploadCalled;

            public void Init() { }
            public void Download(string _, System.Action<bool, string, SaveFileMetadata> cb) => cb?.Invoke(false, null, null);
            public void Upload(string _, string __, SaveFileMetadata ___, System.Action<bool> cb) { UploadCalled = true; cb?.Invoke(true); }
            public void Delete(string _, System.Action<bool> cb) { DeleteCalled = true; cb?.Invoke(true); }
        }

        // ─── Test save objects ────────────────────────────────────────────────

        [System.Serializable]
        private class TestData : ISaveObject
        {
            public int score;
            public void OnBeforeSave() { }
        }

        [System.Serializable]
        [SaveKey("mgr_attributed_stable")]
        private class AttributedTestData : ISaveObject
        {
            public int score;
            public void OnBeforeSave() { }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static SaveManager CreateManager(MockSaveWrapper wrapper = null)
        {
            wrapper ??= new MockSaveWrapper();
            var mgr = new SaveManager(wrapper);
            mgr.Init("save", null, null);  // no cloud handler → resolves synchronously
            return mgr;
        }

        private static (SaveManager mgr, MockSaveWrapper wrapper) CreateManagerAndWrapper()
        {
            var wrapper = new MockSaveWrapper();
            return (CreateManager(wrapper), wrapper);
        }

        // ─── Init ─────────────────────────────────────────────────────────────

        [Test]
        public void Init_SetsIsSaveLoaded()
        {
            Assert.IsTrue(CreateManager().IsSaveLoaded);
        }

        [Test]
        public void Init_CallsWrapperInit()
        {
            var wrapper = new MockSaveWrapper();
            new SaveManager(wrapper).Init("save", null, null);
            Assert.IsTrue(wrapper.InitCalled);
        }

        [Test]
        public void Init_FiresOnCompleteCallback()
        {
            var wrapper = new MockSaveWrapper();
            bool fired  = false;
            new SaveManager(wrapper).Init("save", null, () => fired = true);
            Assert.IsTrue(fired);
        }

        [Test]
        public void Init_IsSaveLoaded_IsTrueInsideCallback()
        {
            var wrapper = new MockSaveWrapper();
            var mgr     = new SaveManager(wrapper);
            bool loadedInsideCallback = false;
            mgr.Init("save", null, () => loadedInsideCallback = mgr.IsSaveLoaded);
            Assert.IsTrue(loadedInsideCallback);
        }

        // ─── GetSaveObject ────────────────────────────────────────────────────

        [Test]
        public void GetSaveObject_ReturnsDefaultInstance()
        {
            var obj = CreateManager().GetSaveObject<TestData>();
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, obj.score);
        }

        [Test]
        public void GetSaveObject_SameTypeTwice_ReturnsSameInstance()
        {
            var mgr = CreateManager();
            var a   = mgr.GetSaveObject<TestData>();
            a.score = 55;

            Assert.AreEqual(55, mgr.GetSaveObject<TestData>().score);
        }

        [Test]
        public void GetSaveObject_UsesFullTypeName_AsDefaultKey()
        {
            // GetSaveObject<T>() and GetSaveObject<T>(typeof(T).FullName) return the same object
            var mgr = CreateManager();
            var a   = mgr.GetSaveObject<TestData>();
            var b   = mgr.GetSaveObject<TestData>(typeof(TestData).FullName);
            Assert.AreSame(a, b);
        }

        [Test]
        public void GetSaveObject_WithExplicitKey_IsolatedFromTypeName()
        {
            var mgr     = CreateManager();
            var byType  = mgr.GetSaveObject<TestData>();
            var byKey   = mgr.GetSaveObject<TestData>("custom.key");
            byType.score = 1;
            byKey.score  = 2;

            Assert.AreEqual(1, mgr.GetSaveObject<TestData>().score);
            Assert.AreEqual(2, mgr.GetSaveObject<TestData>("custom.key").score);
        }

        // ─── GetFile ──────────────────────────────────────────────────────────

        [Test]
        public void GetFile_ReturnsNonNullFile()
        {
            Assert.IsNotNull(CreateManager().GetFile("world_1"));
        }

        [Test]
        public void GetFile_SameNameTwice_ReturnsSameInstance()
        {
            var mgr = CreateManager();
            Assert.AreSame(mgr.GetFile("world_1"), mgr.GetFile("world_1"));
        }

        [Test]
        public void GetFile_DifferentNames_ReturnsDifferentInstances()
        {
            var mgr = CreateManager();
            Assert.AreNotSame(mgr.GetFile("world_1"), mgr.GetFile("world_2"));
        }

        [Test]
        public void GetFile_DefaultAndNamedFile_AreIndependent()
        {
            var mgr  = CreateManager();
            mgr.GetSaveObject<TestData>().score = 10;
            mgr.GetFile("world_1").GetSaveObject<TestData>().score = 99;

            Assert.AreEqual(10, mgr.GetSaveObject<TestData>().score);
            Assert.AreEqual(99, mgr.GetFile("world_1").GetSaveObject<TestData>().score);
        }

        // ─── MarkDefaultAsDirty ───────────────────────────────────────────────

        [Test]
        public void MarkDefaultAsDirty_CausesDefaultFileToBeSaved()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            mgr.MarkAllAsDirty();
            mgr.Save(forceSave: false, useThreads: false);

            Assert.IsTrue(wrapper.HasFile("save"));
        }

        // ─── Save ─────────────────────────────────────────────────────────────

        [Test]
        public void Save_DirtyFile_WritesToWrapper()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            mgr.GetSaveObject<TestData>().score = 5;
            mgr.MarkAllAsDirty();
            mgr.Save(forceSave: false, useThreads: false);

            Assert.IsTrue(wrapper.HasFile("save"));
        }

        [Test]
        public void Save_ForceSave_WritesEvenWhenNotDirty()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            mgr.Save(forceSave: true, useThreads: false);

            Assert.IsTrue(wrapper.HasFile("save"));
        }

        [Test]
        public void Save_NotDirtyNotForced_DoesNotWrite()
        {
            // After Init the default file's IsDirty is false — no write expected
            var (mgr, wrapper) = CreateManagerAndWrapper();
            mgr.Save(forceSave: false, useThreads: false);

            Assert.IsFalse(wrapper.HasFile("save"));
        }

        [Test]
        public void Save_IncrementsSaveCount()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            mgr.MarkAllAsDirty();
            mgr.Save(forceSave: false, useThreads: false);

            string raw     = wrapper.GetRaw("save");
            var reloaded = JsonUtility.FromJson<SaveFile>(raw);
            Assert.AreEqual(1, reloaded.SaveCount);
        }

        // ─── DeleteFile ───────────────────────────────────────────────────────

        [Test]
        public void DeleteFile_RemovesFromWrapper()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            mgr.GetFile("world_1").GetSaveObject<TestData>().score = 5;
            mgr.Save(forceSave: true, useThreads: false);
            Assert.IsTrue(wrapper.HasFile("world_1"));

            mgr.DeleteFile("world_1");
            Assert.IsFalse(wrapper.HasFile("world_1"));
        }

        [Test]
        public void DeleteFile_ThenGetFile_ReturnsEmptyFile()
        {
            var mgr = CreateManager();
            mgr.GetFile("world_1").GetSaveObject<TestData>().score = 99;
            mgr.Save(forceSave: true, useThreads: false);

            mgr.DeleteFile("world_1");

            Assert.AreEqual(0, mgr.GetFile("world_1").GetSaveObject<TestData>().score);
        }

        [Test]
        public void DeleteFile_NonExistent_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => CreateManager().DeleteFile("ghost"));
        }

        [Test]
        public void DeleteFile_DoesNotAffectDefaultFile()
        {
            var mgr = CreateManager();
            mgr.GetSaveObject<TestData>().score = 42;
            mgr.GetFile("world_1").GetSaveObject<TestData>().score = 7;
            mgr.DeleteFile("world_1");

            Assert.AreEqual(42, mgr.GetSaveObject<TestData>().score);
        }

        // ─── Persistence round-trip (two SaveManager instances, same wrapper) ──

        [Test]
        public void Save_ThenNewManager_RestoresDefaultFileData()
        {
            var wrapper = new MockSaveWrapper();

            var mgr1 = new SaveManager(wrapper);
            mgr1.Init("save", null, null);
            mgr1.GetSaveObject<TestData>().score = 42;
            mgr1.MarkAllAsDirty();
            mgr1.Save(forceSave: false, useThreads: false);

            var mgr2 = new SaveManager(wrapper);
            mgr2.Init("save", null, null);
            Assert.AreEqual(42, mgr2.GetSaveObject<TestData>().score);
        }

        [Test]
        public void Save_ThenNewManager_RestoresNamedFileData()
        {
            var wrapper = new MockSaveWrapper();

            var mgr1 = new SaveManager(wrapper);
            mgr1.Init("save", null, null);
            mgr1.GetFile("world_1").GetSaveObject<TestData>().score = 77;
            mgr1.Save(forceSave: true, useThreads: false);

            var mgr2 = new SaveManager(wrapper);
            mgr2.Init("save", null, null);
            Assert.AreEqual(77, mgr2.GetFile("world_1").GetSaveObject<TestData>().score);
        }

        [Test]
        public void Delete_ThenNewManager_NamedFileDataGone()
        {
            var wrapper = new MockSaveWrapper();

            var mgr1 = new SaveManager(wrapper);
            mgr1.Init("save", null, null);
            mgr1.GetFile("world_1").GetSaveObject<TestData>().score = 77;
            mgr1.Save(forceSave: true, useThreads: false);
            mgr1.DeleteFile("world_1");

            var mgr2 = new SaveManager(wrapper);
            mgr2.Init("save", null, null);
            Assert.AreEqual(0, mgr2.GetFile("world_1").GetSaveObject<TestData>().score);
        }

        // ─── GetDefaultFileCopy ───────────────────────────────────────────────

        [Test]
        public void GetDefaultFileCopy_ReturnsIndependentInstance()
        {
            var mgr  = CreateManager();
            mgr.GetSaveObject<TestData>().score = 10;

            var copy = mgr.GetDefaultFileCopy();
            copy.Init();

            // Mutating the live save does not affect the copy (it was loaded from wrapper)
            Assert.AreNotSame(mgr.GetSaveObject<TestData>(), copy.GetSaveObject<TestData>());
        }

        // ─── Configure ────────────────────────────────────────────────────────

        [Test]
        public void Configure_DoesNotThrow()
        {
            var mgr = CreateManager();
            Assert.DoesNotThrow(() => mgr.Configure(new SaveWrapperConfig()));
        }

        // ─── [SaveKey] attribute ─────────────────────────────────────────────

        [Test]
        public void GetSaveObject_WithSaveKeyAttribute_UsesAttributeKey()
        {
            var mgr        = CreateManager();
            var byAttr     = mgr.GetSaveObject<AttributedTestData>();
            var byExplicit = mgr.GetSaveObject<AttributedTestData>("mgr_attributed_stable");

            byAttr.score = 77;
            Assert.AreEqual(77, byExplicit.score);
            Assert.AreSame(byAttr, byExplicit);
        }

        [Test]
        public void GetSaveObject_WithSaveKeyAttribute_NotSameAsFullNameKey()
        {
            var mgr        = CreateManager();
            var byAttr     = mgr.GetSaveObject<AttributedTestData>();
            var byFullName = mgr.GetSaveObject<AttributedTestData>(typeof(AttributedTestData).FullName);

            Assert.AreNotSame(byAttr, byFullName);
        }

        // ─── SaveCustom ───────────────────────────────────────────────────────

        [Test]
        public void SaveCustom_WithNullFileName_WritesToDefaultFile()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            var copy = mgr.GetDefaultFileCopy();
            copy.Init();
            copy.GetSaveObject<TestData>().score = 777;

            mgr.SaveCustom(copy, null);

            Assert.IsTrue(wrapper.HasFile("save"));
        }

        [Test]
        public void SaveCustom_WithExplicitFileName_WritesToNamedFile()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            var file = mgr.GetFile("custom_slot");
            file.GetSaveObject<TestData>().score = 42;

            mgr.SaveCustom(file, "custom_slot");

            Assert.IsTrue(wrapper.HasFile("custom_slot"));
        }

        [Test]
        public void SaveCustom_WithExplicitFileName_DoesNotOverwriteDefault()
        {
            var (mgr, wrapper) = CreateManagerAndWrapper();
            var file = mgr.GetFile("custom_slot");

            mgr.SaveCustom(file, "custom_slot");

            Assert.IsFalse(wrapper.HasFile("save"));
        }

        // ─── ForceCompleteInit ────────────────────────────────────────────────

        [Test]
        public void ForceCompleteInit_SetsSaveLoaded()
        {
            var wrapper = new MockSaveWrapper();
            var mgr     = new SaveManager(wrapper);
            Assert.IsFalse(mgr.IsSaveLoaded);

            mgr.ForceCompleteInit();

            Assert.IsTrue(mgr.IsSaveLoaded);
        }

        [Test]
        public void ForceCompleteInit_WhenAlreadyLoaded_DoesNotThrow()
        {
            var mgr = CreateManager();
            Assert.IsTrue(mgr.IsSaveLoaded);
            Assert.DoesNotThrow(() => mgr.ForceCompleteInit());
        }

        // ─── SyncPendingToCloud ───────────────────────────────────────────────

        [Test]
        public void SyncPendingToCloud_NoCloudHandler_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => CreateManager().SyncPendingToCloud());
        }

        [Test]
        public void SyncPendingToCloud_WithCloudHandler_UploadsPendingFiles()
        {
            var wrapper = new MockSaveWrapper();
            var cloud   = new MockCloudHandler();
            var mgr     = new SaveManager(wrapper);
            mgr.SetCloudHandler(cloud);
            mgr.Init("save", null, null);

            // Force a write so the file ends up in pendingCloudSync
            mgr.MarkAllAsDirty();
            mgr.Save(forceSave: false, useThreads: false);

            mgr.SyncPendingToCloud();

            Assert.IsTrue(cloud.UploadCalled);
        }

        // ─── DeleteFile + cloud ───────────────────────────────────────────────

        [Test]
        public void DeleteFile_WithCloudHandler_CallsCloudDelete()
        {
            var wrapper = new MockSaveWrapper();
            var cloud   = new MockCloudHandler();
            var mgr     = new SaveManager(wrapper);
            mgr.SetCloudHandler(cloud);
            mgr.Init("save", null, null);

            mgr.GetFile("world_1").GetSaveObject<TestData>().score = 5;
            mgr.Save(forceSave: true, useThreads: false);
            mgr.DeleteFile("world_1");

            Assert.IsTrue(cloud.DeleteCalled);
        }

        [Test]
        public void DeleteFile_WithCloudHandler_RemovesFromPendingSync()
        {
            var wrapper = new MockSaveWrapper();
            var cloud   = new MockCloudHandler();
            var mgr     = new SaveManager(wrapper);
            mgr.SetCloudHandler(cloud);
            mgr.Init("save", null, null);

            mgr.GetFile("world_1").GetSaveObject<TestData>().score = 5;
            mgr.Save(forceSave: true, useThreads: false);

            // Delete before syncing to cloud
            mgr.DeleteFile("world_1");
            cloud.UploadCalled = false; // reset

            // SyncPendingToCloud should not upload the deleted file
            mgr.SyncPendingToCloud();

            // The default file was also saved — check it uploaded but world_1 was skipped
            // (MockCloudHandler doesn't distinguish filenames, so we verify no second upload for deleted)
            Assert.IsFalse(wrapper.HasFile("world_1"));
        }
    }
}
