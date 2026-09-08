using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public static class AttackLineOfSight
    {
        private const float ORIGIN_SAMPLE_DISTANCE = 1f;

        private static readonly string[] BarrierAreaNames = { "Fence" };

        private static int barrierMask;
        private static bool isBarrierMaskResolved;

        public static int BarrierMask
        {
            get
            {
                if (!isBarrierMaskResolved)
                    ResolveBarrierMask();

                return barrierMask;
            }
        }

        public static bool IsClear(Vector3 from, Vector3 to)
        {
            var mask = BarrierMask;

            if (mask == 0)
                return true;

            if (!NavMesh.SamplePosition(from, out var origin, ORIGIN_SAMPLE_DISTANCE, NavMesh.AllAreas))
                return true;

            if (NavMesh.Raycast(origin.position, to, out _, NavMesh.AllAreas))
                return true;

            return !NavMesh.Raycast(origin.position, to, out _, NavMesh.AllAreas & ~mask);
        }

        private static void ResolveBarrierMask()
        {
            isBarrierMaskResolved = true;
            barrierMask = 0;

            for (var i = 0; i < BarrierAreaNames.Length; i++)
            {
                var area = NavMesh.GetAreaFromName(BarrierAreaNames[i]);

                if (area >= 0)
                    barrierMask |= 1 << area;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            isBarrierMaskResolved = false;
            barrierMask = 0;
        }
    }
}
