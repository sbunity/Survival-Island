using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class MissionPickerSource
    {
        private static string[] ids;
        private static GUIContent[] options;

        static MissionPickerSource()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        public static string[] Ids
        {
            get
            {
                Ensure();

                return ids;
            }
        }

        public static GUIContent[] Options
        {
            get
            {
                Ensure();

                return options;
            }
        }

        public static void Invalidate()
        {
            ids = null;
            options = null;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            Invalidate();
        }

        private static void OnSceneClosed(Scene scene)
        {
            Invalidate();
        }

        private static void Ensure()
        {
            if (ids != null)
                return;

            var idList = new List<string> { string.Empty };
            var optionList = new List<GUIContent> { new GUIContent("None") };

            AppendCatalog(idList, optionList);
            AppendOpenedScenes(idList, optionList);

            ids = idList.ToArray();
            options = optionList.ToArray();
        }

        private static void AppendCatalog(List<string> idList, List<GUIContent> optionList)
        {
            var catalog = FindCatalog();

            if (catalog == null || catalog.Entries.IsNullOrEmpty())
                return;

            for (var i = 0; i < catalog.Entries.Length; i++)
            {
                MissionsCatalog.Entry entry = catalog.Entries[i];

                if (entry == null || string.IsNullOrEmpty(entry.MissionId) || idList.Contains(entry.MissionId))
                    continue;

                idList.Add(entry.MissionId);
                optionList.Add(new GUIContent(FormatLabel(entry.WorldName, entry.Order, entry.MissionName)));
            }
        }

        private static void AppendOpenedScenes(List<string> idList, List<GUIContent> optionList)
        {
            var holders = Object.FindObjectsByType<MissionsHolder>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < holders.Length; i++)
            {
                var holder = holders[i];

                var order = 0;

                for (var j = 0; j < holder.transform.childCount; j++)
                {
                    var mission = holder.transform.GetChild(j).GetComponent<Mission>();

                    if (mission == null)
                        continue;

                    order++;

                    if (string.IsNullOrEmpty(mission.ID) || idList.Contains(mission.ID))
                        continue;

                    idList.Add(mission.ID);
                    optionList.Add(new GUIContent(FormatLabel(holder.gameObject.scene.name, order, mission.name + " (not in catalog)")));
                }
            }
        }

        private static string FormatLabel(string worldName, int order, string missionName)
        {
            var world = string.IsNullOrEmpty(worldName) ? "Unknown World" : worldName.Replace('/', ' ');

            return string.Format("{0}/{1:00} {2}", world, order, missionName.Replace('/', ' '));
        }

        private static MissionsCatalog FindCatalog()
        {
            var guids = AssetDatabase.FindAssets("t:MissionsCatalog");

            for (var i = 0; i < guids.Length; i++)
            {
                var catalog = AssetDatabase.LoadAssetAtPath<MissionsCatalog>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (catalog != null)
                    return catalog;
            }

            return null;
        }
    }
}
