using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    public static class SaveActions
    {
        [MenuItem("Actions/Remove Save", priority = 1)]
        [MenuItem("Edit/Clear Save", priority = 270)]
        private static void RemoveSave()
        {
            PlayerPrefs.DeleteAll();

            ISaveWrapper wrapper = SaveController.GetWrapper();
            wrapper.Init();

            SaveManager manager = new SaveManager(wrapper);
            manager.DeleteFile(SaveController.DEFAULT_FILE_NAME);

            WorldsDatabase worldsDatabase = EditorUtils.GetAsset<WorldsDatabase>();
            if (worldsDatabase != null)
            {
                foreach (WorldData worldData in worldsDatabase.Worlds)
                {
                    manager.DeleteFile(worldData.ID);
                }
            }

            Debug.Log("Save files are removed!");
        }

        [MenuItem("Actions/Remove Save", true)]
        private static bool RemoveSaveValidation()
        {
            return !Application.isPlaying;
        }
    }
}