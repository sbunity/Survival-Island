using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class MissionsCatalogBuilder
    {
        private const string CATALOG_PATH = "Assets/Project Files/Data/Missions Catalog.asset";

        static MissionsCatalogBuilder()
        {
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        [MenuItem("Tools/Missions/Rebuild Missions Catalog")]
        public static void RebuildAll()
        {
            var database = GetWorldsDatabase();

            if (database == null)
            {
                Debug.LogError("[Missions Catalog]: Worlds Database asset is missing.");

                return;
            }

            var catalog = GetCatalog(true);

            var entries = new List<MissionsCatalog.Entry>();

            if (!database.Worlds.IsNullOrEmpty())
            {
                for (var i = 0; i < database.Worlds.Length; i++)
                {
                    CollectWorld(database.Worlds[i], entries);
                }
            }

            Apply(catalog, entries, database);

            Debug.Log(string.Format("[Missions Catalog]: {0} missions registered.", entries.Count));
        }

        private static void OnSceneSaved(Scene scene)
        {
            var database = GetWorldsDatabase();

            if (database == null || database.Worlds.IsNullOrEmpty())
                return;

            WorldData savedWorld = null;

            for (var i = 0; i < database.Worlds.Length; i++)
            {
                var world = database.Worlds[i];

                if (world != null && world.Scene != null && world.Scene.Path == scene.path)
                {
                    savedWorld = world;

                    break;
                }
            }

            if (savedWorld == null)
                return;

            var catalog = GetCatalog(false);

            if (catalog == null)
                return;

            var entries = new List<MissionsCatalog.Entry>();

            if (!catalog.Entries.IsNullOrEmpty())
            {
                for (var i = 0; i < catalog.Entries.Length; i++)
                {
                    var entry = catalog.Entries[i];

                    if (entry != null && entry.WorldId != savedWorld.ID)
                        entries.Add(entry);
                }
            }

            CollectFromScene(scene, savedWorld, entries);

            Apply(catalog, entries, database);
        }

        private static void Apply(MissionsCatalog catalog, List<MissionsCatalog.Entry> entries, WorldsDatabase database)
        {
            Sort(entries, database);

            catalog.SetEntries(entries.ToArray());

            AssetDatabase.SaveAssetIfDirty(catalog);

            MissionPickerSource.Invalidate();
        }

        private static void CollectWorld(WorldData world, List<MissionsCatalog.Entry> entries)
        {
            if (world == null || world.Scene == null || string.IsNullOrEmpty(world.Scene.Path))
                return;

            var loadedScene = SceneManager.GetSceneByPath(world.Scene.Path);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                CollectFromScene(loadedScene, world, entries);

                return;
            }

            var openedScene = EditorSceneManager.OpenScene(world.Scene.Path, OpenSceneMode.Additive);

            CollectFromScene(openedScene, world, entries);

            EditorSceneManager.CloseScene(openedScene, true);
        }

        private static void CollectFromScene(Scene scene, WorldData world, List<MissionsCatalog.Entry> entries)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            var roots = scene.GetRootGameObjects();

            for (var i = 0; i < roots.Length; i++)
            {
                var holders = roots[i].GetComponentsInChildren<MissionsHolder>(true);

                for (var j = 0; j < holders.Length; j++)
                {
                    CollectFromHolder(holders[j], world, entries);
                }
            }
        }

        private static void CollectFromHolder(MissionsHolder holder, WorldData world, List<MissionsCatalog.Entry> entries)
        {
            var order = 0;

            for (var i = 0; i < holder.transform.childCount; i++)
            {
                var mission = holder.transform.GetChild(i).GetComponent<Mission>();

                if (mission == null)
                    continue;

                order++;

                if (string.IsNullOrEmpty(mission.ID))
                {
                    Debug.LogWarning(string.Format("[Missions Catalog]: Mission {0} has no ID and is skipped.", mission.name), mission);

                    continue;
                }

                var duplicateIndex = entries.FindIndex(x => x.MissionId == mission.ID);

                if (duplicateIndex >= 0)
                {
                    Debug.LogWarning(string.Format("[Missions Catalog]: Duplicated mission ID {0} on {1}, the entry from {2} is kept.", mission.ID, mission.name, entries[duplicateIndex].WorldName), mission);

                    continue;
                }

                entries.Add(new MissionsCatalog.Entry(mission.ID, mission.name, world.ID, world.DisplayName, order));
            }
        }

        private static void Sort(List<MissionsCatalog.Entry> entries, WorldsDatabase database)
        {
            entries.Sort((a, b) =>
            {
                int worldCompare = GetWorldIndex(database, a.WorldId).CompareTo(GetWorldIndex(database, b.WorldId));

                return worldCompare != 0 ? worldCompare : a.Order.CompareTo(b.Order);
            });
        }

        private static int GetWorldIndex(WorldsDatabase database, string worldId)
        {
            if (database.Worlds.IsNullOrEmpty())
                return int.MaxValue;

            for (var i = 0; i < database.Worlds.Length; i++)
            {
                if (database.Worlds[i] != null && database.Worlds[i].ID == worldId)
                    return i;
            }

            return int.MaxValue;
        }

        private static MissionsCatalog GetCatalog(bool createIfMissing)
        {
            var catalog = FindAsset<MissionsCatalog>();

            if (catalog != null || !createIfMissing)
                return catalog;

            catalog = ScriptableObject.CreateInstance<MissionsCatalog>();

            AssetDatabase.CreateAsset(catalog, CATALOG_PATH);
            AssetDatabase.SaveAssets();

            Debug.Log(string.Format("[Missions Catalog]: Catalog asset created at {0}.", CATALOG_PATH));

            return catalog;
        }

        private static WorldsDatabase GetWorldsDatabase()
        {
            return FindAsset<WorldsDatabase>();
        }

        private static T FindAsset<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            for (var i = 0; i < guids.Length; i++)
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (asset != null)
                    return asset;
            }

            return null;
        }
    }
}
