using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class HelperRosterSave : ISaveObject
    {
        [SerializeField] List<Entry> entries = new();
        public List<Entry> Entries => entries;

        public Entry GetEntry(string globalId)
        {
            if (string.IsNullOrEmpty(globalId) || entries == null)
                return null;

            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].GlobalId == globalId)
                    return entries[i];
            }

            return null;
        }

        public Entry GetOrCreateEntry(string globalId)
        {
            var entry = GetEntry(globalId);
            if (entry != null)
                return entry;

            entries ??= new List<Entry>();

            entry = new Entry { GlobalId = globalId };
            entries.Add(entry);

            return entry;
        }

        public void Reset()
        {
            entries?.Clear();
        }

        public void OnBeforeSave()
        {

        }

        [System.Serializable]
        public class Entry
        {
            public string GlobalId;
            public string WorldId;
        }
    }
}
