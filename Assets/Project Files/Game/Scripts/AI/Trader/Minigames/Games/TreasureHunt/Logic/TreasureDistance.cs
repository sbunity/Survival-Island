using UnityEngine;

namespace Watermelon
{
    public static class TreasureDistance
    {
        public static int Measure(Vector2Int from, Vector2Int to, TreasureDistanceMode mode)
        {
            var dx = Mathf.Abs(from.x - to.x);
            var dy = Mathf.Abs(from.y - to.y);

            return mode switch
            {
                TreasureDistanceMode.Manhattan => dx + dy,
                TreasureDistanceMode.Euclidean => Mathf.RoundToInt(Mathf.Sqrt(dx * dx + dy * dy)),
                _ => Mathf.Max(dx, dy),
            };
        }
    }
}
