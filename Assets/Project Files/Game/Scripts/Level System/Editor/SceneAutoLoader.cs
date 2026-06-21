using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class SceneAutoLoader
    {
        static SceneAutoLoader()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

            RewriteStartScene();
        }

        private static void RewriteStartScene()
        {
            WorldBehavior world = GameObject.FindAnyObjectByType<WorldBehavior>(FindObjectsInactive.Include);
            if(world != null)
            {
                WorldsDatabase worldsDatabase = EditorUtils.GetAsset<WorldsDatabase>();
                if (worldsDatabase != null)
                {
                    WorldData worldData = worldsDatabase.GetWorldByName(world.gameObject.scene.name);
                    if (worldData != null)
                    {
                        Debug.Log($"[SceneAutoLoader]: World scene is loaded in editor, activating game scene...");

                        var wrapper = SaveController.GetWrapper();

                        SaveManager saveManager = new SaveManager(wrapper);
                        saveManager.Init(SaveController.DEFAULT_FILE_NAME, null, () =>
                        {
                            var file = saveManager.GetFile(SaveController.DEFAULT_FILE_NAME);
                            file.GetSaveObject<WorldGlobalSave>("worldGlobal").worldID = worldData.ID;
                            file.MarkAsDirty();

                            saveManager.Save(true, false);
                        });
                    }
                }

                ActivateGameScene();

                return;
            }

            SubworldBehavior subworld = GameObject.FindAnyObjectByType<SubworldBehavior>();
            if(subworld != null)
            {
                Debug.Log($"[SceneAutoLoader]: Subworld scene is loaded in editor, activating game scene...");

                ActivateGameScene();

                return;
            }
        }

        private static void ActivateGameScene()
        {
            int gameSceneIndex = SceneUtils.GetBuildIndexByName("Game");
            if (gameSceneIndex != -1)
            {
                EditorApplication.delayCall += () =>
                {
                    GameLoading.LoadingSceneBuildIndex = gameSceneIndex;
                };
            }
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            RewriteStartScene();
        }
    }
}
