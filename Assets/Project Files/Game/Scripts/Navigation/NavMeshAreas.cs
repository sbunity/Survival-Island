using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public static class NavMeshAreas
    {
        private const string WATER_AREA_NAME = "Water";

        private static bool isWaterMaskResolved;
        private static int waterMask;

        public static int Water
        {
            get
            {
                if (!isWaterMaskResolved)
                {
                    isWaterMaskResolved = true;

                    var area = NavMesh.GetAreaFromName(WATER_AREA_NAME);
                    waterMask = area >= 0 ? 1 << area : 0;
                }

                return waterMask;
            }
        }

        public static int Land => NavMesh.AllAreas & ~Water;

        public static bool IsWater(int areaMask) => (areaMask & Water) != 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            isWaterMaskResolved = false;
            waterMask = 0;
        }
    }
}
