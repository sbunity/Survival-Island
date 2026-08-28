using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class MinigameRandom
    {
        public static int Range(this System.Random random, DuoInt range)
        {
            var min = Mathf.Min(range.firstValue, range.secondValue);
            var max = Mathf.Max(range.firstValue, range.secondValue);

            return random.Next(min, max + 1);
        }

        public static T Pick<T>(this System.Random random, IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            return list[random.Next(0, list.Count)];
        }

        public static void Shuffle<T>(this System.Random random, IList<T> list)
        {
            if (list == null)
                return;

            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static List<T> PickDistinct<T>(this System.Random random, IList<T> source, int count, IList<T> required = null)
        {
            var result = new List<T>();
            if (source == null || source.Count == 0 || count <= 0)
                return result;

            if (required != null)
            {
                for (var i = 0; i < required.Count && result.Count < count; i++)
                {
                    if (!result.Contains(required[i]))
                        result.Add(required[i]);
                }
            }

            var pool = new List<T>(source);
            for (var i = 0; i < result.Count; i++)
                pool.Remove(result[i]);

            random.Shuffle(pool);

            for (var i = 0; i < pool.Count && result.Count < count; i++)
                result.Add(pool[i]);

            return result;
        }
    }
}
