using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Trader Minigames Database", menuName = "Data/Trader/Trader Minigames Database")]
    public class TraderMinigamesDatabase : ScriptableObject
    {
        [SerializeField] TraderMinigameDefinition[] minigames;
        public TraderMinigameDefinition[] Minigames => minigames;

        private Dictionary<string, TraderMinigameDefinition> minigamesLink;

        public TraderMinigameDefinition GetRandom()
        {
            if (minigames.IsNullOrEmpty())
                return null;

            var totalWeight = 0f;
            for (var i = 0; i < minigames.Length; i++)
            {
                if (minigames[i] != null)
                    totalWeight += minigames[i].Weight;
            }

            if (totalWeight <= 0f)
                return null;

            var roll = Random.Range(0f, totalWeight);

            for (var i = 0; i < minigames.Length; i++)
            {
                if (minigames[i] == null)
                    continue;

                roll -= minigames[i].Weight;

                if (roll <= 0f)
                    return minigames[i];
            }

            return null;
        }

        public TraderMinigameDefinition GetByID(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            if (minigamesLink == null)
                BuildLink();

            minigamesLink.TryGetValue(id, out TraderMinigameDefinition definition);

            return definition;
        }

        private void BuildLink()
        {
            minigamesLink = new Dictionary<string, TraderMinigameDefinition>();

            if (minigames.IsNullOrEmpty())
                return;

            for (var i = 0; i < minigames.Length; i++)
            {
                var definition = minigames[i];

                if (definition == null || string.IsNullOrEmpty(definition.ID))
                    continue;

                if (!minigamesLink.TryAdd(definition.ID, definition))
                    Debug.LogWarning($"[Trader Minigames]: Duplicated minigame ID \"{definition.ID}\" on asset \"{definition.name}\".", definition);
            }
        }

        private void OnDisable()
        {
            minigamesLink = null;
        }
    }
}
