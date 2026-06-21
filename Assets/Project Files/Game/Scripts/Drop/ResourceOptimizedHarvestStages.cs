using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceOptimizedHarvestStages : MonoBehaviour
    {
        [SerializeField] List<ResourceVisualStage> harvestStages = new List<ResourceVisualStage>();
        public List<ResourceVisualStage> HarvestStages => harvestStages;

        public void Init()
        {

        }

        public void Unload()
        {

        }

        [Button]
        public void CacheData()
        {
            GetComponentsInChildren(harvestStages);

            harvestStages.Sort((first, second) => second.Id - first.Id);

            if (harvestStages.Count == 0)
            {
                GameObject emptyStage = new GameObject("Empty Stage");
                emptyStage.transform.SetParent(transform);
                emptyStage.transform.localPosition = Vector3.zero;

                harvestStages.Add(emptyStage.AddComponent<ResourceVisualStage>());
            }
        }
    }
}