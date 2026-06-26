using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for CloudSaveManager conflict-resolution logic.
    /// Uses a synchronous MockCloudHandler to avoid coroutines.
    /// </summary>
    [TestFixture]
    public class CloudSaveManagerTests
    {
        // ─── Mock ─────────────────────────────────────────────────────────────

        private class MockCloudHandler : ICloudSaveHandler
        {
            public bool IsAvailable { get; set; } = true;

            // Recorded calls
            public bool UploadCalled;
            public bool DownloadCalled;
            public bool DeleteCalled;

            // Canned responses
            public bool   DownloadSuccess  = true;
            public string DownloadJson     = null;
            public SaveFileMetadata DownloadMetadata = null;

            public bool UploadSuccess = true;
            public bool DeleteSuccess = true;

            public void Init() { }

            public void Download(string fileName, Action<bool, string, SaveFileMetadata> onComplete)
            {
                DownloadCalled = true;
                onComplete?.Invoke(DownloadSuccess, DownloadJson, DownloadMetadata);
            }

            public void Upload(string fileName, string json, SaveFileMetadata metadata, Action<bool> onComplete)
            {
                UploadCalled = true;
                onComplete?.Invoke(UploadSuccess);
            }

            public void Delete(string fileName, Action<bool> onComplete)
            {
                DeleteCalled = true;
                onComplete?.Invoke(DeleteSuccess);
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Creates a SaveFile with the given number of flushes applied.</summary>
        private static SaveFile MakeSaveFile(int flushes = 0)
        {
            var file = new SaveFile();
            file.Init();
            for (int i = 0; i < flushes; i++)
                file.Flush(updateLastSaved: true);
            return file;
        }

        /// <summary>Serializes a SaveFile and returns its metadata.</summary>
        private static (string json, SaveFileMetadata meta) Serialize(SaveFile file)
        {
            string json = JsonUtility.ToJson(file);
            var    meta = new SaveFileMetadata(file.LastSavedUnix, file.SaveCount);
            return (json, meta);
        }

        // ─── IsConfigured ─────────────────────────────────────────────────────

        [Test]
        public void IsConfigured_NoHandler_ReturnsFalse()
        {
            var mgr = new CloudSaveManager();
            Assert.IsFalse(mgr.IsConfigured);
        }

        [Test]
        public void IsConfigured_UnavailableHandler_ReturnsFalse()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { IsAvailable = false };
            mgr.SetHandler(handler);
            Assert.IsFalse(mgr.IsConfigured);
        }

        [Test]
        public void IsConfigured_AvailableHandler_ReturnsTrue()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { IsAvailable = true };
            mgr.SetHandler(handler);
            Assert.IsTrue(mgr.IsConfigured);
        }

        // ─── SyncToCloud ──────────────────────────────────────────────────────

        [Test]
        public void SyncToCloud_NotConfigured_CallsbackFalse()
        {
            var mgr    = new CloudSaveManager();
            bool? result = null;
            mgr.SyncToCloud("save", MakeSaveFile(), success => result = success);
            Assert.IsFalse(result);
        }

        [Test]
        public void SyncToCloud_NullSaveFile_CallsbackFalse()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler();
            mgr.SetHandler(handler);

            LogAssert.Expect(LogType.Error, "[Cloud Save]: Cannot sync null SaveFile to cloud");
            bool? result = null;
            mgr.SyncToCloud("save", null, success => result = success);

            Assert.IsFalse(result);
            Assert.IsFalse(handler.UploadCalled);
        }

        [Test]
        public void SyncToCloud_Configured_CallsHandlerUpload()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler();
            mgr.SetHandler(handler);

            mgr.SyncToCloud("save", MakeSaveFile(1), _ => { });

            Assert.IsTrue(handler.UploadCalled);
        }

        [Test]
        public void SyncToCloud_UploadSucceeds_CallsbackTrue()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { UploadSuccess = true };
            mgr.SetHandler(handler);

            bool? result = null;
            mgr.SyncToCloud("save", MakeSaveFile(1), success => result = success);

            Assert.IsTrue(result);
        }

        [Test]
        public void SyncToCloud_UploadFails_CallsbackFalse()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { UploadSuccess = false };
            mgr.SetHandler(handler);

            bool? result = null;
            mgr.SyncToCloud("save", MakeSaveFile(1), success => result = success);

            Assert.IsFalse(result);
        }

        // ─── SyncFromCloud — not configured ───────────────────────────────────

        [Test]
        public void SyncFromCloud_NotConfigured_ReturnsLocalUnchanged()
        {
            var mgr   = new CloudSaveManager();
            var local = MakeSaveFile();
            CloudSyncResult result = null;

            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.success);
            Assert.AreEqual(CloudSyncResult.CloudConflictResolution.LocalUnchanged, result.resolution);
            Assert.AreSame(local, result.resolvedSave);
        }

        // ─── SyncFromCloud — no cloud save exists ─────────────────────────────

        [Test]
        public void SyncFromCloud_DownloadFails_ReturnsNoConflict()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { DownloadSuccess = false };
            mgr.SetHandler(handler);

            var local  = MakeSaveFile();
            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.IsTrue(result.success);
            Assert.AreEqual(CloudSyncResult.CloudConflictResolution.NoConflict, result.resolution);
            Assert.AreSame(local, result.resolvedSave);
        }

        [Test]
        public void SyncFromCloud_EmptyCloudJson_ReturnsNoConflict()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = "",
                DownloadMetadata = new SaveFileMetadata(999, 10)
            };
            mgr.SetHandler(handler);

            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", MakeSaveFile(), r => result = r);

            Assert.AreEqual(CloudSyncResult.CloudConflictResolution.NoConflict, result.resolution);
        }

        // ─── SyncFromCloud — cloud preferred ─────────────────────────────────

        [Test]
        public void SyncFromCloud_CloudIsNewer_ResolvesToCloud()
        {
            var local  = MakeSaveFile(flushes: 2);  // saveCount = 2
            var cloud  = MakeSaveFile(flushes: 5);  // saveCount = 5 → newer

            var (cloudJson, cloudMeta) = Serialize(cloud);

            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = cloudJson,
                DownloadMetadata = cloudMeta
            };
            mgr.SetHandler(handler);

            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.AreEqual(CloudSyncResult.CloudConflictResolution.CloudPreferred, result.resolution);
            Assert.IsTrue(result.success);
            Assert.AreEqual(5, result.resolvedSave.SaveCount);
        }

        [Test]
        public void SyncFromCloud_CloudIsNewer_ResolvedSaveIsNotLocalReference()
        {
            var local                  = MakeSaveFile(flushes: 1);
            var cloud                  = MakeSaveFile(flushes: 3);
            var (cloudJson, cloudMeta) = Serialize(cloud);

            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = cloudJson,
                DownloadMetadata = cloudMeta
            };
            mgr.SetHandler(handler);

            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            // Resolved save must be the cloud-loaded instance, not the local one
            Assert.AreNotSame(local, result.resolvedSave);
        }

        // ─── SyncFromCloud — local preferred ─────────────────────────────────

        [Test]
        public void SyncFromCloud_LocalIsNewer_ResolvesToLocal()
        {
            var local  = MakeSaveFile(flushes: 8);  // saveCount = 8
            var cloud  = MakeSaveFile(flushes: 3);  // saveCount = 3 → older

            var (cloudJson, cloudMeta) = Serialize(cloud);

            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = cloudJson,
                DownloadMetadata = cloudMeta
            };
            mgr.SetHandler(handler);

            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.AreEqual(CloudSyncResult.CloudConflictResolution.LocalPreferred, result.resolution);
            Assert.IsTrue(result.success);
            Assert.AreSame(local, result.resolvedSave);
        }

        // ─── SyncFromCloud — bad JSON ─────────────────────────────────────────

        [Test]
        public void SyncFromCloud_InvalidCloudJson_ReturnsSyncFailed()
        {
            // Cloud reports saveCount=100 (newer than local=0) but JSON is garbage
            var local  = MakeSaveFile(flushes: 0);

            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = "NOT_VALID_JSON",
                DownloadMetadata = new SaveFileMetadata(timestamp: 9999, count: 100)
            };
            mgr.SetHandler(handler);

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[Cloud Save\]: Failed to parse cloud save for 'save'"));
            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.AreEqual(CloudSyncResult.CloudConflictResolution.SyncFailed, result.resolution);
            Assert.IsFalse(result.success);
        }

        // ─── SyncFromCloud — metadata populated in result ─────────────────────

        [Test]
        public void SyncFromCloud_ResultContainsLocalMetadata()
        {
            var local  = MakeSaveFile(flushes: 2);
            var cloud  = MakeSaveFile(flushes: 5);
            var (cloudJson, cloudMeta) = Serialize(cloud);

            var mgr     = new CloudSaveManager();
            mgr.SetHandler(new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = cloudJson,
                DownloadMetadata = cloudMeta
            });

            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.IsNotNull(result.localMetadata);
            Assert.AreEqual(2, result.localMetadata.saveCount);
        }

        [Test]
        public void SyncFromCloud_ResultContainsCloudMetadata()
        {
            var local  = MakeSaveFile(flushes: 1);
            var cloud  = MakeSaveFile(flushes: 4);
            var (cloudJson, cloudMeta) = Serialize(cloud);

            var mgr     = new CloudSaveManager();
            mgr.SetHandler(new MockCloudHandler
            {
                DownloadSuccess  = true,
                DownloadJson     = cloudJson,
                DownloadMetadata = cloudMeta
            });

            CloudSyncResult result = null;
            mgr.SyncFromCloud("save", local, r => result = r);

            Assert.IsNotNull(result.cloudMetadata);
            Assert.AreEqual(4, result.cloudMetadata.saveCount);
        }

        // ─── DeleteCloud ──────────────────────────────────────────────────────

        [Test]
        public void DeleteCloud_NotConfigured_CallsbackFalse()
        {
            var mgr    = new CloudSaveManager();
            bool? result = null;
            mgr.DeleteCloud("save", success => result = success);
            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteCloud_Configured_CallsHandlerDelete()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { DeleteSuccess = true };
            mgr.SetHandler(handler);

            bool? result = null;
            mgr.DeleteCloud("save", success => result = success);

            Assert.IsTrue(handler.DeleteCalled);
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteCloud_HandlerFails_CallsbackFalse()
        {
            var mgr     = new CloudSaveManager();
            var handler = new MockCloudHandler { DeleteSuccess = false };
            mgr.SetHandler(handler);

            bool? result = null;
            mgr.DeleteCloud("save", success => result = success);

            Assert.IsFalse(result);
        }

        // ─── CacheLocalMetadata ───────────────────────────────────────────────

        [Test]
        public void CacheLocalMetadata_NullMetadata_DoesNotThrow()
        {
            var mgr = new CloudSaveManager();
            Assert.DoesNotThrow(() => mgr.CacheLocalMetadata("save", null));
        }

        [Test]
        public void CacheLocalMetadata_ValidMetadata_DoesNotThrow()
        {
            var mgr  = new CloudSaveManager();
            var meta = new SaveFileMetadata(1000, 3);
            Assert.DoesNotThrow(() => mgr.CacheLocalMetadata("save", meta));
        }
    }
}
