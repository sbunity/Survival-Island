using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Trader Offers Database", menuName = "Data/Trader/Trader Offers Database")]
    public class TraderOffersDatabase : ScriptableObject
    {
        [SerializeField] TraderOffer[] offers;
        public TraderOffer[] Offers => offers;

        [SerializeField, Min(1)] int minOffersPerVisit = 3;
        public int MinOffersPerVisit => minOffersPerVisit;

        [SerializeField, Min(1)] int maxOffersPerVisit = 4;
        public int MaxOffersPerVisit => maxOffersPerVisit;

        [BoxGroup("Unlocks", "Resource Unlocks")]
        [SerializeField] ResourceUnlockRule[] unlockRules;
        public ResourceUnlockRule[] UnlockRules => unlockRules;

        public void Initialise()
        {
            ResourceUnlocks.Initialise(unlockRules);
        }

        public List<int> GetRandomOfferIndices()
        {
            var indices = new List<int>();

            if (offers == null || offers.Length == 0)
                return indices;

            for (var i = 0; i < offers.Length; i++)
            {
                if (offers[i] != null && offers[i].IsUnlocked)
                    indices.Add(i);
            }

            if (indices.Count == 0)
                return indices;

            for (var i = indices.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            var min = Mathf.Min(minOffersPerVisit, maxOffersPerVisit);
            var max = Mathf.Max(minOffersPerVisit, maxOffersPerVisit);
            var count = Mathf.Clamp(Random.Range(min, max + 1), 1, indices.Count);

            indices.RemoveRange(count, indices.Count - count);

            return indices;
        }

        public TraderOffer GetOffer(int index)
        {
            if (offers == null || index < 0 || index >= offers.Length)
                return null;

            return offers[index];
        }

#if UNITY_EDITOR
        [Button("Add Missing Resources")]
        private void AddMissingResources()
        {
            var rules = new List<ResourceUnlockRule>();

            if (unlockRules != null)
                rules.AddRange(unlockRules);

            var addedCount = 0;

            foreach (CurrencyType currency in System.Enum.GetValues(typeof(CurrencyType)))
            {
                if (rules.Exists(x => x.Currency == currency))
                    continue;

                rules.Add(new ResourceUnlockRule(currency, ResourceUnlockRule.UnlockMode.FromStart, string.Empty));

                addedCount++;
            }

            if (addedCount == 0)
            {
                Debug.Log("[Trader Offers]: Every resource already has an unlock rule.", this);

                return;
            }

            unlockRules = rules.ToArray();

            UnityEditor.EditorUtility.SetDirty(this);

            Debug.Log(string.Format("[Trader Offers]: {0} resources added to the unlock rules.", addedCount), this);
        }
#endif
    }
}
