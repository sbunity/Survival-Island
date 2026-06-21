using System;
using System.IO;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// File system save wrapper for Windows, macOS, Android, and iOS.
    /// Uses atomic write (write to <c>.tmp</c>, then <c>File.Replace</c>) to prevent save corruption on crash.
    /// </summary>
    public sealed class DefaultSaveWrapper : ISaveWrapper
    {
        // Cached on construction (main thread) so background threads can safely use it.
        private string persistentDataPath;

        /// <summary>Caches <see cref="Application.persistentDataPath"/> so background threads can access it safely.</summary>
        public void Init()
        {
            persistentDataPath = Application.persistentDataPath;
        }

        private string JsonPath(string fileName) => Path.Combine(persistentDataPath, fileName + ".json");

        /// <summary>Loads the save file from <c>persistentDataPath/{fileName}.json</c>; auto-recovers from a <c>.tmp</c> file if the primary is missing.</summary>
        public SaveFile Load(string fileName)
        {
            string jsonPath = JsonPath(fileName);
            string tmpPath  = jsonPath + ".tmp";

            // Recovery: if .json is missing but .tmp exists, a previous save was interrupted
            // between WriteAllText and File.Replace — recover from the temp file.
            if (!File.Exists(jsonPath) && File.Exists(tmpPath))
            {
                try { File.Move(tmpPath, jsonPath); }
                catch (Exception ex) { Debug.LogWarning($"[Save]: Could not recover '{fileName}' from .tmp: {ex.Message}"); }
            }

            if (File.Exists(jsonPath))
            {
                try
                {
                    SaveFile loaded = JsonUtility.FromJson<SaveFile>(File.ReadAllText(jsonPath));
                    if (loaded != null)
                    {
                        loaded.Init();
                        return loaded;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Save]: Failed to read save '{fileName}': {ex.Message}");
                }
            }

            SaveFile empty = new();
            empty.Init();
            return empty;
        }

        /// <summary>Writes JSON to a <c>.tmp</c> file then atomically replaces the final file using <c>File.Replace</c>.</summary>
        public void SaveRaw(string fileName, string json)
        {
            string finalPath = JsonPath(fileName);
            string tmpPath   = finalPath + ".tmp";

            try
            {
                File.WriteAllText(tmpPath, json);

                // File.Replace atomically replaces finalPath with tmpPath on Windows (ReplaceFile API).
                // Fall back to Move on first save when finalPath doesn't exist yet.
                if (File.Exists(finalPath))
                    File.Replace(tmpPath, finalPath, destinationBackupFileName: null);
                else
                    File.Move(tmpPath, finalPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Save]: Failed to write save '{fileName}': {ex.Message}");
            }
        }

        /// <summary>Deletes both the <c>.json</c> and <c>.tmp</c> files for the given save file name.</summary>
        public void Delete(string fileName)
        {
            string jsonPath = JsonPath(fileName);
            string tmpPath  = jsonPath + ".tmp";

            if (File.Exists(jsonPath)) File.Delete(jsonPath);
            if (File.Exists(tmpPath))  File.Delete(tmpPath);
        }

        /// <summary>Returns <c>true</c>; file I/O can be safely offloaded to a <see cref="System.Threading.ThreadPool"/> thread.</summary>
        public bool UseThreads() => true;

        public void Configure(SaveWrapperConfig config)
        {
            // DefaultSaveWrapper doesn't require any configuration
        }
    }
}
