using System;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    public class ShipwreckLootSave : ISaveObject
    {
        public bool IsInitialised;
        public bool IsLooted;

        [SerializeField] private Resource[] savedResources;

        [NonSerialized] public ResourcesList Resources;

        public void Initialise(ResourcesList initialResources)
        {
            if (!IsInitialised)
            {
                IsInitialised = true;
                Resources = initialResources != null ? new ResourcesList(initialResources) : new ResourcesList();
                IsLooted = Resources.Count == 0;

                OnBeforeSave();
                return;
            }

            Resources = savedResources != null ? new ResourcesList(savedResources) : new ResourcesList();

            if (Resources.Count == 0)
                IsLooted = true;
        }

        public void SetResources(ResourcesList resources)
        {
            Resources = resources ?? new ResourcesList();
            OnBeforeSave();
        }

        public void MarkAsLooted()
        {
            IsLooted = true;
            Resources = new ResourcesList();
            OnBeforeSave();
        }

        public void OnBeforeSave() 
            => savedResources = Resources != null ? Resources.ToArray() : Array.Empty<Resource>();
    }
}
