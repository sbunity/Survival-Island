using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public struct WaterSurfaceProbe
    {
        private const float SAMPLE_DISTANCE = 0.5f;
        private const float SWITCH_DELAY = 0.15f;

        private float pendingTime;

        public bool IsInWater { get; private set; }

        public void Reset(Vector3 position, int areaMask)
        {
            IsInWater = Sample(position, areaMask, false);
            pendingTime = 0f;
        }

        public bool Tick(Vector3 position, int areaMask, float deltaTime)
        {
            if (Sample(position, areaMask, IsInWater) == IsInWater)
            {
                pendingTime = 0f;

                return false;
            }

            pendingTime += deltaTime;

            if (pendingTime < SWITCH_DELAY)
                return false;

            IsInWater = !IsInWater;
            pendingTime = 0f;

            return true;
        }

        private static bool Sample(Vector3 position, int areaMask, bool fallback)
        {
            if (!NavMesh.SamplePosition(position, out var hit, SAMPLE_DISTANCE, areaMask))
                return fallback;

            return NavMeshAreas.IsWater(hit.mask);
        }
    }
}
