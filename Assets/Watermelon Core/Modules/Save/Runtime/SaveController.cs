using System.Collections;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// MonoBehaviour facade over <see cref="SaveManager"/>. Manages Unity lifecycle (auto-save, cloud sync coroutines,
    /// focus/pause hooks) and exposes a static API for game code.
    /// Created automatically by <see cref="SaveInitModule"/> on the Initializer's GameObject.
    /// </summary>
    public class SaveController : MonoBehaviour
    {
        /// <summary>File name used for the primary save file (without extension).</summary>
        public const string DEFAULT_FILE_NAME = "save";

        private const float INIT_TIMEOUT_SECONDS = 15f;
#if UNITY_EDITOR
        private const float CLOUD_SYNC_INTERVAL = 30f;   // shorter for editor testing
#else
        private const float CLOUD_SYNC_INTERVAL = 300f;  // 5 minutes in production
#endif

        private static SaveController instance;
        private static TimeSave timeSave;

        // Created at field-init so Configure() can be called before InitAsync() (see SaveInitModule)
        private readonly SaveManager manager = new SaveManager(GetWrapper());

        /// <summary>Fires once when the save system finishes loading (including cloud sync or timeout fallback).</summary>
        public static event SimpleCallback OnSaveLoaded;

        /// <summary>
        /// Initializes the save system: auto-discovers a cloud handler, loads the default save file,
        /// performs cloud sync (with a timeout fallback), then starts auto-save and cloud sync coroutines.
        /// </summary>
        public IEnumerator InitAsync(float autoSaveDelay, string[] namedFiles = null)
        {
            instance = this;

            manager.Init(DEFAULT_FILE_NAME, namedFiles, null);

            // Wait for cloud sync with a timeout — prevents infinite hang if SDK never fires callback
            float elapsed = 0f;
            while (!manager.IsSaveLoaded && elapsed < INIT_TIMEOUT_SECONDS)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!manager.IsSaveLoaded)
            {
                Debug.LogError($"[Save Controller]: Init timed out after {INIT_TIMEOUT_SECONDS}s. Falling back to local save.");
                manager.ForceCompleteInit();
            }

            timeSave = GetSaveObject<TimeSave>();
            timeSave.BeginSession();

            Debug.Log("[Save Controller]: Save loaded.");
            OnSaveLoaded?.Invoke();

            if (autoSaveDelay > 0f)
                StartCoroutine(AutoSaveCoroutine(autoSaveDelay));
        }

        /// <summary>Applies platform-specific wrapper configuration (e.g. WebGL prefix) before initialization.</summary>
        public void Configure(SaveWrapperConfig config) => manager.Configure(config);

        private void OnDestroy()
        {
#if UNITY_EDITOR
            // Synchronous save on editor stop to avoid lost data
            manager.Save(forceSave: true, useThreads: false);
#endif
            OnSaveLoaded = null;
            instance = null;
        }

#if !UNITY_EDITOR
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                manager.Save();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                manager.Save();
            }
        }
#endif

        private IEnumerator AutoSaveCoroutine(float saveDelay)
        {
            var wait = new WaitForSeconds(saveDelay);

            while (true)
            {
                yield return wait;
                manager.Save();
            }
        }

        // --- Time tracking ---

        /// <summary>The time the game was last saved (approximates last exit time). Returns <see cref="System.DateTime.MinValue"/> if never saved.</summary>
        public static System.DateTime LastExitTime => timeSave?.LastExitTime ?? System.DateTime.MinValue;

        /// <summary>Total accumulated gameplay time in seconds, including the current session.</summary>
        public static float GameTime => timeSave?.GameTime ?? 0f;

        // --- Static API (delegates to manager) ---

        /// <summary>Returns the save object for type T from the default save file, resolving the key automatically.</summary>
        public static T GetSaveObject<T>() where T : ISaveObject, new()
            => instance.manager.GetSaveObject<T>();

        /// <summary>Returns the save object stored under the given explicit key in the default save file.</summary>
        public static T GetSaveObject<T>(string key) where T : ISaveObject, new()
            => instance.manager.GetSaveObject<T>(key);

        /// <summary>Returns the save object from a named save file; key is optional and resolved automatically if omitted.</summary>
        public static T GetSaveObject<T>(string fileName, string key = null) where T : ISaveObject, new()
            => instance != null ? instance.manager.GetSaveObject<T>(fileName, key) : default;

        /// <summary>Marks all loaded save files as dirty so they are written on the next <see cref="Save"/> call.</summary>
        public static void MarkAsSaveIsRequired()
        {
            if (instance != null) instance.manager.MarkAllAsDirty();
        }

        /// <summary>Returns a named save file by its file name, loading it from disk on first access.</summary>
        public static SaveFile GetFile(string fileName)
        {
            if (instance == null) return null;
            return instance.manager.GetFile(fileName);
        }

        /// <summary>Flushes all dirty save files to disk; pass <c>forceSave: true</c> to write regardless of dirty state.</summary>
        public static void Save(bool forceSave = false, bool useThreads = true)
        {
            if (instance != null) instance.manager.Save(forceSave, useThreads);
        }

        /// <summary>Deletes a named save file from disk and cloud.</summary>
        public static void DeleteFile(string fileName)
        {
            if (instance != null) instance.manager.DeleteFile(fileName);
        }

        /// <summary>Deletes the default save file from disk and cloud.</summary>
        public static void DeleteSaveFile()
        {
            if (instance != null) instance.manager.DeleteDefaultFile();
        }

        /// <summary>Returns a deep copy of the current default save file; reflects in-memory changes even before the next flush.</summary>
        public static SaveFile GetSaveFileCopy()
        {
            if (instance == null) return null;
            return instance.manager.GetDefaultFileCopy();
        }

        /// <summary>Writes a custom <see cref="SaveFile"/> to disk without updating its <c>lastSaved</c> or <c>saveCount</c> metadata.</summary>
        public static void SaveCustom(SaveFile saveFile, string fileName = null)
        {
            if (instance != null) instance.manager.SaveCustom(saveFile, fileName);
        }

        /// <summary>Logs save file info (last saved time, container keys) to the Unity console.</summary>
        public static void Info()
        {
            if (instance != null) instance.manager.Info();
        }

        /// <summary>Returns the appropriate <see cref="ISaveWrapper"/> for the current platform: <see cref="WebGLSaveWrapper"/> on WebGL, <see cref="DefaultSaveWrapper"/> otherwise.</summary>
        public static ISaveWrapper GetWrapper()
        {
#if UNITY_EDITOR
            return new DefaultSaveWrapper();
#elif UNITY_WEBGL
            return new WebGLSaveWrapper();
#else
            return new DefaultSaveWrapper();
#endif
        }
    }
}
