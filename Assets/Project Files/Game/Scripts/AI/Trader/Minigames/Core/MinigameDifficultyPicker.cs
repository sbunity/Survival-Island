using UnityEngine;

namespace Watermelon
{
    public static class MinigameDifficultyPicker
    {
        private const int SEED_SALT = 0x5D0C;

        public static T Pick<T>(T[] difficulties, int seed) where T : MinigameDifficulty
        {
            if (difficulties.IsNullOrEmpty())
                return null;

            var random = new System.Random(seed ^ SEED_SALT);

            var totalWeight = 0f;

            for (var i = 0; i < difficulties.Length; i++)
            {
                if (difficulties[i] != null)
                    totalWeight += Mathf.Max(0f, difficulties[i].Weight);
            }

            if (totalWeight <= 0f)
                return FindAny(difficulties, random.Next(0, difficulties.Length));

            var roll = (float)random.NextDouble() * totalWeight;

            for (var i = 0; i < difficulties.Length; i++)
            {
                if (difficulties[i] == null)
                    continue;

                roll -= Mathf.Max(0f, difficulties[i].Weight);

                if (roll <= 0f)
                    return difficulties[i];
            }

            return FindAny(difficulties, difficulties.Length - 1);
        }

        private static T FindAny<T>(T[] difficulties, int preferredIndex) where T : MinigameDifficulty
        {
            for (var i = 0; i < difficulties.Length; i++)
            {
                var index = (preferredIndex + i + difficulties.Length) % difficulties.Length;

                if (difficulties[index] != null)
                    return difficulties[index];
            }

            return null;
        }
    }
}
