using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Pure C# save system core. Manages save files, cloud sync, and data access.
    /// Fully testable without MonoBehaviour — inject ISaveWrapper via constructor.
    /// SaveController is the thin MonoBehaviour adapter that drives coroutines and Unity lifecycle.
    /// </summary>
    public class SaveManager
    {
        private readonly ISaveWrapper wrapper;

        private Dictionary<string, SaveFile> loadedFiles;
        private Dictionary<string, object> fileLocks;
        private SaveFile defaultFile;
        private string defaultFileName;

        /// <summary>True once the default save file has been loaded (and cloud sync has resolved or timed out).</summary>
        public bool IsSaveLoaded { get; private set; }
        /// <summary>The platform wrapper used for all file I/O.</summary>
        public ISaveWrapper Wrapper => wrapper;

        public SaveManager(ISaveWrapper wrapper)
        {
            this.wrapper      = wrapper;
        }

        /// <summary>Passes configuration to the underlying <see cref="ISaveWrapper"/> (e.g. WebGL prefix).</summary>
        public void Configure(SaveWrapperConfig config)
        {
            wrapper.Configure(config);
        }

        /// <summary>
        /// Loads the default save file and performs cloud sync.
        /// onComplete fires when IsSaveLoaded becomes true.
        /// With no cloud handler the callback is synchronous — IsSaveLoaded is true before Init returns.
        /// With a real cloud handler the callback fires asynchronously after the network response.
        /// namedFiles: additional file names declared by the project to cloud-sync on startup.
        /// </summary>
        public void Init(string fileName, string[] namedFiles, Action onComplete)
        {
            defaultFileName = fileName;
            loadedFiles     = new Dictionary<string, SaveFile>();
            fileLocks       = new Dictionary<string, object>();

            wrapper.Init();

            defaultFile = wrapper.Load(fileName);
            defaultFile.Init();

            // Register immediately so Save() works even while cloud sync is in flight.
            loadedFiles[fileName] = defaultFile;

            IsSaveLoaded = true;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Forces initialization to complete immediately.
        /// Used when cloud sync times out — local save is already loaded, only cloud result is pending.
        /// </summary>
        public void ForceCompleteInit()
        {
            if (!IsSaveLoaded)
            {
                IsSaveLoaded = true;
                Debug.LogWarning("[Save Manager]: Init forcibly completed — cloud sync may not have finished.");
            }
        }

        // --- Default file accessors ---

        /// <summary>Returns the save object for type T from the default save file, resolving the key via <see cref="SaveKeyMap"/> or the <see cref="SaveKeyAttribute"/>.</summary>
        public T GetSaveObject<T>() where T : ISaveObject, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("[Save Manager]: Not initialized.");
                return default;
            }

            return defaultFile.GetSaveObject<T>(); // SaveKey attribute resolved inside SaveFile
        }

        /// <summary>Returns the save object stored under the given explicit key in the default save file.</summary>
        public T GetSaveObject<T>(string key) where T : ISaveObject, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("[Save Manager]: Not initialized.");
                return default;
            }

            return defaultFile.GetSaveObject<T>(key);
        }

        /// <summary>
        /// Returns a save object from a named save file.
        /// <paramref name="key"/> is optional — if omitted, resolved via [SaveKey] attribute or typeof(T).FullName.
        /// </summary>
        public T GetSaveObject<T>(string fileName, string key = null) where T : ISaveObject, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("[Save Manager]: Not initialized.");
                return default;
            }

            SaveFile file = GetFile(fileName);
            return key != null ? file.GetSaveObject<T>(key) : file.GetSaveObject<T>();
        }

        /// <summary>
        /// Marks all currently loaded save files as dirty so they are included in the next Save() call.
        /// </summary>
        public void MarkAllAsDirty()
        {
            if (loadedFiles == null) return;
            foreach (SaveFile file in loadedFiles.Values)
                file.MarkAsDirty();
        }

        // --- Multiple file support ---

        /// <summary>Returns a named save file; loads it from disk on first access (lazy load).</summary>
        public SaveFile GetFile(string fileName)
        {
            if (loadedFiles == null)
            {
                Debug.LogError("[Save Manager]: Not initialized.");
                return null;
            }

            if (!loadedFiles.TryGetValue(fileName, out SaveFile file))
            {
                file = wrapper.Load(fileName);
                file.Init();

                loadedFiles[fileName] = file;
            }

            return file;
        }

        // --- Save ---

        /// <summary>Flushes and writes all dirty save files to disk; pass <c>forceSave: true</c> to write regardless of dirty state.</summary>
        public void Save(bool forceSave = false, bool useThreads = true)
        {
            if (loadedFiles == null) return;

            foreach (KeyValuePair<string, SaveFile> kvp in loadedFiles)
            {
                if (forceSave || kvp.Value.IsDirty)
                    WriteFile(kvp.Key, kvp.Value, useThreads);
            }
        }

        /// <summary>Flushes and serializes one save file, then writes it to disk — on a background thread if supported.</summary>
        private void WriteFile(string fileName, SaveFile file, bool useThreads)
        {
            file.Flush(updateLastSaved: true);

            string json = JsonUtility.ToJson(file);

            if (useThreads && wrapper.UseThreads())
            {
                if (!fileLocks.TryGetValue(fileName, out object fileLock))
                {
                    fileLock = new object();
                    fileLocks[fileName] = fileLock;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    lock (fileLock)
                    {
                        wrapper.SaveRaw(fileName, json);
                        Debug.Log($"[Save Manager]: Saved '{fileName}'.");
                    }
                });
            }
            else
            {
                wrapper.SaveRaw(fileName, json);
                Debug.Log($"[Save Manager]: Saved '{fileName}'.");
            }
        }

        // --- Delete ---

        /// <summary>Removes the named save file from memory, disk, and cloud storage.</summary>
        public void DeleteFile(string fileName)
        {
            loadedFiles?.Remove(fileName);
            fileLocks?.Remove(fileName);
            wrapper.Delete(fileName);
        }

        /// <summary>Removes the default save file from memory, disk, and cloud storage.</summary>
        public void DeleteDefaultFile()
        {
            loadedFiles?.Remove(defaultFileName);
            fileLocks?.Remove(defaultFileName);
            wrapper.Delete(defaultFileName);
        }

        // --- Utilities ---

        /// <summary>Exports a snapshot of the default save file to an arbitrary absolute path; used for preset authoring in the editor.</summary>
        public void PresetsSave(string fullFileName)
        {
            if (defaultFile == null) return;
            defaultFile.Flush(updateLastSaved: false);
            wrapper.SaveRaw(fullFileName, JsonUtility.ToJson(defaultFile));
        }

        /// <summary>Returns a deep copy of the default save file; reflects in-memory changes even if the file hasn't been flushed to disk yet.</summary>
        public SaveFile GetDefaultFileCopy()
        {
            // If there are unsaved in-memory changes, disk is stale — serialize from memory.
            if (defaultFile != null && defaultFile.IsDirty)
            {
                defaultFile.Flush(updateLastSaved: false);
                string json = JsonUtility.ToJson(defaultFile);
                SaveFile copy = JsonUtility.FromJson<SaveFile>(json);
                copy.Init();
                return copy;
            }

            return wrapper.Load(defaultFileName); // Load() now calls Init() internally
        }

        /// <summary>
        /// Writes saveFile to disk under the given fileName (defaults to the default save file).
        /// Does not update lastSaved or saveCount.
        /// </summary>
        public void SaveCustom(SaveFile saveFile, string fileName = null)
        {
            if (saveFile == null) return;
            string targetFile = fileName ?? defaultFileName;
            saveFile.Flush(updateLastSaved: false);
            wrapper.SaveRaw(targetFile, JsonUtility.ToJson(saveFile));
        }

        /// <summary>Logs default save file info (last saved time, container keys) to the Unity console.</summary>
        public void Info() => defaultFile?.Info();
    }
}
