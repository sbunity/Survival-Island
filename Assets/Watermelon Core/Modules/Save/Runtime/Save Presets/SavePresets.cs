using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
using UnityEngine;
using System.IO;

namespace Watermelon
{
    public class SavePresets
    {
        private const string PRESET_FOLDER_PREFIX = "SavePresets/";
        private const string PRESETS_FOLDER_NAME = "SavePresets";
        public static bool saveDataMofied = false;
        private const char SEPARATOR = '/';
        public const string DEFAULT_DIRECTORY = "Custom";
        public const string META_SUFFIX = ".meta";

        public static void DeleteFiles(string path)
        {
            string[] filePaths = Directory.GetFiles(path);

            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }

        private static void LoadSaveFromPath(string presetPath)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated in playmode!");
                return;
            }

            if (EditorApplication.isCompiling)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated during compiling!");
                return;
            }

            string[] presetFilePaths = Directory.GetFiles(presetPath);

            if (presetFilePaths.Length == 0)
            {
                Debug.LogError(string.Format("[Save Presets]: Preset  at path {0} doesn’t  exist!", presetPath));
                return;
            }

            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (currentSceneName.Equals("Init") || (currentSceneName.Equals("Level Editor")))
            {
                EditorSceneManager.OpenScene(Path.Combine(CoreEditor.FOLDER_SCENES, "Game.unity"));
            }

            string[] saveFiles = new string[presetFilePaths.Length];
            DeleteFiles(Application.persistentDataPath); // avoiding situation when new save partially overrides old save that has more files

            for (int i = 0; i < saveFiles.Length; i++)
            {
                saveFiles[i] = Path.Combine(Application.persistentDataPath, GetName(presetFilePaths[i]));
                // Replace current save file with the preset
                File.Copy(presetFilePaths[i], saveFiles[i], true);
            }

            // Start game
            EditorApplication.isPlaying = true;
#endif
        }

        private static void CreateSavePreset(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
                SaveController.Save(true, false);

            if (string.IsNullOrEmpty(saveName))
            {
                Debug.LogError("[Save Presets]: Preset name can't be empty!");
                return;
            }

            if (!Directory.Exists(GetDirectoryPath())) //Creating SavePresets folder
            {
                Directory.CreateDirectory(GetDirectoryPath());
            }

            if (!Directory.Exists(GetDirectoryPath(tabName))) //Creating custom folder
            {
                Directory.CreateDirectory(GetDirectoryPath(tabName));
            }

            string presetFolderPath = GetPresetFolderPath(saveName, tabName);

            if (!Directory.Exists(presetFolderPath)) //Creating custom folder
            {
                Directory.CreateDirectory(presetFolderPath);
            }

            string[] saveFiles = Directory.GetFiles(Application.persistentDataPath);

            if (saveFiles.Length == 0)
            {
                Debug.LogError("[Save Presets]: Save file doesn’t exist!");

                return;
            }

            string[] presetFilePaths = new string[saveFiles.Length];
            DeleteFiles(presetFolderPath); // avoiding situation when new save partially overrides old save that has more files

            for (int i = 0; i < presetFilePaths.Length; i++)
            {
                presetFilePaths[i] = Path.Combine(presetFolderPath, GetName(saveFiles[i]));
                File.Copy(saveFiles[i], presetFilePaths[i], true);
            }

            Directory.SetCreationTime(presetFolderPath, DateTime.Now);
            saveDataMofied = true;
#endif
        }



        public static void LoadSave(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
            string presetPath = GetPresetFolderPath(saveName, tabName);
            LoadSaveFromPath(presetPath);
        }

        public static void CreateSave(string saveName, string tabName = DEFAULT_DIRECTORY, string id = "")
        {
#if UNITY_EDITOR
            if (id.Length == 0)
            {
                id = saveName;
            }

            CreateSavePreset(saveName, tabName);
            SetId(saveName, tabName, id);
#endif
        }

        public static void SetId(string name, string tabName, string id)
        {
            string presetPath = GetPresetFolderPath(name, tabName) + META_SUFFIX;
            File.WriteAllText(presetPath, id);
        }

        private static string GetPresetPathById(string id)
        {
            string directoryPath = SavePresets.GetDirectoryPath();
            string[] directoryEntries = Directory.GetDirectories(directoryPath);
            string[] fileEntries;

            for (int i = 0; i < directoryEntries.Length; i++)
            {
                fileEntries = Directory.GetFiles(directoryEntries[i]);

                for (int j = 0; j < fileEntries.Length; j++)
                {
                    if (fileEntries[j].EndsWith(SavePresets.META_SUFFIX))
                    {
                        if (File.ReadAllText(fileEntries[j]).Equals(id))
                        {
                            return fileEntries[j].Replace(SavePresets.META_SUFFIX,string.Empty);
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static void LoadSaveById(string id)
        {
            string presetPath = GetPresetPathById(id);

            if (presetPath.Length == 0)
            {
                Debug.LogError(string.Format("[Save Presets]: Preset with id {0} doesn’t  exist!", id));
                return;
            }

            LoadSaveFromPath(presetPath);
        }

        public static void RemoveSave(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
            string presetPath = GetPresetFolderPath(saveName, tabName);

            if (Directory.Exists(presetPath))
            {
                Directory.Delete(presetPath, true);
            }

            presetPath += META_SUFFIX;

            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
            }

            saveDataMofied = true;
        }

        public static bool IsSaveExist(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
            string presetPath = GetPresetFolderPath(saveName, tabName);
            return Directory.Exists(presetPath);
        }


        public static bool IsSaveExistById(string id)
        {
            string presetPath = GetPresetPathById(id);

            if(presetPath.Length == 0) // id isn`t found
            {
                return false;
            }

            if (Directory.Exists(presetPath))
            {
                return true;
            }
            else // remove meta file
            {
                File.Delete(presetPath + META_SUFFIX);
                return false;
            }
        }



        public static void RemoveSaveById(string id)
        {
            string presetPath = GetPresetPathById(id);

            if (presetPath.Length == 0) // id isn`t found
            {
                return;
            }

            if (Directory.Exists(presetPath))
            {
                Directory.Delete(presetPath, true);
            }

            presetPath += META_SUFFIX;

            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
            }

            saveDataMofied = true;
        }

        public static string GetPresetFolderPath(string saveName, string tabName)
        {
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME, tabName, saveName);
        }

        public static string GetDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME);
        }

        public static string GetDirectoryPath(string tabName)
        {
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME, tabName);
        }

        public static string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}
