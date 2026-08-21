using System.Collections.Generic;

namespace Watermelon
{
    public static class ResourceCarrierRegistry
    {
        private static readonly List<ResourcePointBehavior> points = new List<ResourcePointBehavior>();

        public static void RegisterPoint(ResourcePointBehavior point)
        {
            if (point == null)
                return;

            if (points.Contains(point))
                return;

            points.Add(point);
        }

        public static void UnregisterPoint(ResourcePointBehavior point)
        {
            points.Remove(point);
        }

        public static void Evict(IResourceCarrier carrier)
        {
            if (carrier == null)
                return;

            for (var i = points.Count - 1; i >= 0; i--)
            {
                var point = points[i];

                if (point == null)
                {
                    points.RemoveAt(i);

                    continue;
                }

                point.ForceRemoveCarrier(carrier);
            }
        }

        public static void Clear()
        {
            points.Clear();
        }
    }
}
