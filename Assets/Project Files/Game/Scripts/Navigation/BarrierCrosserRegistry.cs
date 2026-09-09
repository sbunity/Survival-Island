using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class BarrierCrosserRegistry
    {
        private static readonly List<IBarrierCrosser> crossers = new(8);

        public static int Count => crossers.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            crossers.Clear();
        }

        public static bool Register(IBarrierCrosser crosser)
        {
            if (!IsReferenceAlive(crosser) || crossers.Contains(crosser))
                return false;

            crossers.Add(crosser);
            return true;
        }

        public static bool Unregister(IBarrierCrosser crosser)
        {
            if (crosser is null)
                return false;

            return crossers.Remove(crosser);
        }

        public static IBarrierCrosser GetCrosser(int index)
        {
            if (index < 0 || index >= crossers.Count)
                return null;

            var crosser = crossers[index];
            return IsReferenceAlive(crosser) ? crosser : null;
        }

        public static void RemoveInvalidCrossers()
        {
            for (var i = crossers.Count - 1; i >= 0; i--)
            {
                if (!IsReferenceAlive(crossers[i]))
                    crossers.RemoveAt(i);
            }
        }

        private static bool IsReferenceAlive(IBarrierCrosser crosser)
        {
            if (crosser is null)
                return false;

            if (crosser is Object unityObject)
                return unityObject != null;

            return true;
        }
    }
}
