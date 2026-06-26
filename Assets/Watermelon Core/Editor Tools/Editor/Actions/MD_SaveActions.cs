using System.IO;
using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    public static class SaveActions
    {
        [MenuItem("Actions/Remove Save", priority = -10)]
        [MenuItem("Edit/Clear Save", priority = 270)]
        private static void RemoveSave()
        {
            PlayerPrefs.DeleteAll();

            // Wipe every save file, not just the default one — named/per-world saves
            // (e.g. "world_1.save") never go through SaveController.DeleteSaveFile().
            string dir = Application.persistentDataPath;

            foreach (string file in Directory.GetFiles(dir, "*.save"))
                File.Delete(file);
            foreach (string file in Directory.GetFiles(dir, "*.save.tmp"))
                File.Delete(file);

            Debug.Log("Save files are removed!");
        }

        [MenuItem("Actions/Remove Save", true)]
        private static bool RemoveSaveValidation()
        {
            return !Application.isPlaying;
        }
    }
}