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

        public List<int> GetRandomOfferIndices()
        {
            var indices = new List<int>();

            if (offers == null || offers.Length == 0)
                return indices;

            for (var i = 0; i < offers.Length; i++)
                indices.Add(i);

            for (var i = indices.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            var min = Mathf.Min(minOffersPerVisit, maxOffersPerVisit);
            var max = Mathf.Max(minOffersPerVisit, maxOffersPerVisit);
            var count = Mathf.Clamp(Random.Range(min, max + 1), 1, offers.Length);

            indices.RemoveRange(count, indices.Count - count);

            return indices;
        }

        public TraderOffer GetOffer(int index)
        {
            if (offers == null || index < 0 || index >= offers.Length)
                return null;

            return offers[index];
        }
    }
}
