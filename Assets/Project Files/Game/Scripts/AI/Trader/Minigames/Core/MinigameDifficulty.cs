using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public abstract class MinigameDifficulty
    {
        [SerializeField] string title = "Normal";
        public string Title => title;

        [SerializeField, Min(0f)] float weight = 1f;
        public float Weight => weight;

        [SerializeField, Min(0.01f)] float rewardMultiplier = 1f;
        public float RewardMultiplier => rewardMultiplier;
    }
}
