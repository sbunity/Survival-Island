using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Missions Catalog", menuName = "Data/Missions Catalog")]
    public class MissionsCatalog : ScriptableObject
    {
        [SerializeField] Entry[] entries;
        public Entry[] Entries => entries;

#if UNITY_EDITOR
        public void SetEntries(Entry[] value)
        {
            entries = value;

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        [System.Serializable]
        public class Entry
        {
            [SerializeField] string missionId;
            public string MissionId => missionId;

            [SerializeField] string missionName;
            public string MissionName => missionName;

            [SerializeField] string worldId;
            public string WorldId => worldId;

            [SerializeField] string worldName;
            public string WorldName => worldName;

            [SerializeField] int order;
            public int Order => order;

            public Entry(string missionId, string missionName, string worldId, string worldName, int order)
            {
                this.missionId = missionId;
                this.missionName = missionName;
                this.worldId = worldId;
                this.worldName = worldName;
                this.order = order;
            }
        }
    }
}
