using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class TreasureHuntDifficulty : MinigameDifficulty
    {
        [SerializeField, Min(TreasureBoard.MIN_SIZE)] int columns = 5;
        public int Columns => Mathf.Max(TreasureBoard.MIN_SIZE, columns);

        [SerializeField, Min(TreasureBoard.MIN_SIZE)] int rows = 5;
        public int Rows => Mathf.Max(TreasureBoard.MIN_SIZE, rows);

        [SerializeField, Min(1)] int digs = 4;
        public int Digs => Mathf.Max(1, digs);

        [Tooltip("Leave empty to use the bands set on the minigame definition.")]
        [SerializeField] TreasureHintBand[] hintBands;

        public TreasureHintBand[] GetHintBands(TreasureHintBand[] fallback)
        {
            return hintBands.IsNullOrEmpty() ? fallback : hintBands;
        }
    }
}
