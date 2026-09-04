using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SudokuDifficulty : MinigameDifficulty
    {
        [SerializeField, Min(SudokuLayout.MIN_SIZE)] int size = 6;
        public int Size => size;

        [SerializeField, Min(0)] int boxWidth;

        [SerializeField, Min(0)] int boxHeight;

        [SerializeField] SudokuRuleFlags rules = SudokuRuleFlags.Row | SudokuRuleFlags.Column;
        public SudokuRuleFlags Rules => rules;

        [SerializeField] DuoFloat holesFraction = new DuoFloat(0.4f, 0.55f);

        [SerializeField, Min(1)] int lives = 3;
        public int Lives => lives;

        public SudokuLayout BuildLayout() => new SudokuLayout(size, boxWidth, boxHeight);

        public SudokuRuleSet BuildRules() => new SudokuRuleSet(BuildLayout(), SudokuRuleFactory.Create(rules));

        public int RollHoles(SudokuLayout layout, System.Random random)
        {
            var min = Mathf.Clamp01(Mathf.Min(holesFraction.firstValue, holesFraction.secondValue));
            var max = Mathf.Clamp01(Mathf.Max(holesFraction.firstValue, holesFraction.secondValue));

            var fraction = min + (float)random.NextDouble() * (max - min);

            return Mathf.Clamp(Mathf.RoundToInt(layout.CellCount * fraction), 1, layout.CellCount - 1);
        }
    }
}
