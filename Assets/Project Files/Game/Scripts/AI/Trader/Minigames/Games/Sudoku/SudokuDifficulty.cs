using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SudokuDifficulty
    {
        private const int SEED_SALT = 0x5D0C;

        [SerializeField] string title = "Normal";
        public string Title => title;

        [SerializeField, Min(SudokuLayout.MIN_SIZE)] int size = 6;
        public int Size => size;

        [SerializeField, Min(0)] int boxWidth;

        [SerializeField, Min(0)] int boxHeight;

        [SerializeField] SudokuRuleFlags rules = SudokuRuleFlags.Row | SudokuRuleFlags.Column;
        public SudokuRuleFlags Rules => rules;

        [SerializeField] DuoFloat holesFraction = new DuoFloat(0.4f, 0.55f);

        [SerializeField, Min(1)] int lives = 3;
        public int Lives => lives;

        [SerializeField, Min(0f)] float weight = 1f;
        public float Weight => weight;

        [SerializeField, Min(0.01f)] float rewardMultiplier = 1f;
        public float RewardMultiplier => rewardMultiplier;

        public SudokuLayout BuildLayout() => new SudokuLayout(size, boxWidth, boxHeight);

        public SudokuRuleSet BuildRules() => new SudokuRuleSet(BuildLayout(), SudokuRuleFactory.Create(rules));

        public int RollHoles(SudokuLayout layout, System.Random random)
        {
            var min = Mathf.Clamp01(Mathf.Min(holesFraction.firstValue, holesFraction.secondValue));
            var max = Mathf.Clamp01(Mathf.Max(holesFraction.firstValue, holesFraction.secondValue));

            var fraction = min + (float)random.NextDouble() * (max - min);

            return Mathf.Clamp(Mathf.RoundToInt(layout.CellCount * fraction), 1, layout.CellCount - 1);
        }

        public static SudokuDifficulty Pick(SudokuDifficulty[] difficulties, int seed)
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

        private static SudokuDifficulty FindAny(SudokuDifficulty[] difficulties, int preferredIndex)
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
