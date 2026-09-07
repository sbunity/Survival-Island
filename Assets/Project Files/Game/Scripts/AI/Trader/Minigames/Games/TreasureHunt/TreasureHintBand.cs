using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class TreasureHintBand
    {
        [SerializeField] string caption = "Cold";
        public string Caption => caption;

        [SerializeField, Min(0)] int maxDistance = 1;
        public int MaxDistance => maxDistance;

        [SerializeField] Color color = Color.white;
        public Color Color => color;

        public TreasureHintBand() { }

        public TreasureHintBand(string caption, int maxDistance, Color color)
        {
            this.caption = caption;
            this.maxDistance = maxDistance;
            this.color = color;
        }

        public static int[] ToDistances(TreasureHintBand[] bands)
        {
            if (bands.IsNullOrEmpty())
                return new int[0];

            var distances = new int[bands.Length];

            for (var i = 0; i < bands.Length; i++)
                distances[i] = bands[i] != null ? bands[i].MaxDistance : int.MaxValue;

            return distances;
        }
    }
}
