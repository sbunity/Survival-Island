using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class MissionProgressSave : ISaveObject
    {
        [SerializeField] List<string> completedMissionIds = new List<string>();

        [System.NonSerialized] private HashSet<string> lookup;

        public bool Contains(string missionId)
        {
            EnsureLookup();

            return lookup.Contains(missionId);
        }

        public bool Add(string missionId)
        {
            EnsureLookup();

            if (!lookup.Add(missionId))
                return false;

            completedMissionIds.Add(missionId);

            return true;
        }

        public void OnBeforeSave() { }

        private void EnsureLookup()
        {
            if (lookup != null)
                return;

            completedMissionIds ??= new List<string>();

            lookup = new HashSet<string>(completedMissionIds);
        }
    }
}
