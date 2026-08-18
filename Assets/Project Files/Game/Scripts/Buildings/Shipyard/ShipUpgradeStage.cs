using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ShipUpgradeStage
    {
        [SerializeField, UniqueID] string id;
        public string ID => id;

        [SerializeField] string title;
        public string Title => title;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [Space]
        [SerializeField] List<Resource> cost;
        public List<Resource> RawCost => cost;

        [SerializeField, Min(1)] int constructionHitsRequired = 10;
        public int ConstructionHitsRequired => constructionHitsRequired;

        public ResourcesList CreateCost() 
            => cost != null ? new ResourcesList(cost) : new ResourcesList();

#if UNITY_EDITOR
        public void EditorSetID(string value)
        {
            id = value;
        }
#endif
    }
}
