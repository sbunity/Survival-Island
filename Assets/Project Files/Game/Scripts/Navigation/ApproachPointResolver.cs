using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public readonly struct ApproachRequest
    {
        public readonly Vector3 TargetPosition;
        public readonly Vector3 OriginPosition;

        public readonly float DesiredDistance;

        public readonly float MinDistance;

        public readonly int AreaMask;

        public ApproachRequest(Vector3 targetPosition, Vector3 originPosition, float desiredDistance, float minDistance, int areaMask)
        {
            TargetPosition = targetPosition;
            OriginPosition = originPosition;

            DesiredDistance = Mathf.Max(desiredDistance, 0.01f);
            MinDistance = Mathf.Clamp(minDistance, 0f, DesiredDistance);

            AreaMask = areaMask;
        }
    }

    public static class ApproachPointResolver
    {
        private const int RING_SAMPLES = 8;
        private const float SAMPLE_RADIUS = 0.6f;
        private const float MIN_DIRECTION_SQR = 0.0001f;

        private static readonly float[] DISTANCE_MULTIPLIERS = new float[] { 1f, 1.4f, 2f };

        public static bool TryResolve(in ApproachRequest request, out Vector3 point)
        {
            var preferredDirection = GetPreferredDirection(request);

            var angleStep = 360f / RING_SAMPLES;

            for (var m = 0; m < DISTANCE_MULTIPLIERS.Length; m++)
            {
                var distance = request.DesiredDistance * DISTANCE_MULTIPLIERS[m];

                for (var i = 0; i < RING_SAMPLES; i++)
                {
                    var direction = Quaternion.Euler(0f, GetRingAngle(i, angleStep), 0f) * preferredDirection;

                    if (TryAccept(request.TargetPosition + direction * distance, request, out point))
                        return true;
                }
            }

            point = request.TargetPosition;

            return false;
        }

        public static bool TrySnapToNavMesh(Vector3 position, float searchRadius, int areaMask, out Vector3 point)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, searchRadius, areaMask))
            {
                point = hit.position;

                return true;
            }

            point = position;

            return false;
        }

        private static float GetRingAngle(int index, float angleStep)
        {
            var half = (index + 1) / 2;

            return (index % 2 == 0 ? -half : half) * angleStep;
        }

        private static bool TryAccept(Vector3 candidate, in ApproachRequest request, out Vector3 point)
        {
            point = candidate;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, SAMPLE_RADIUS, request.AreaMask))
                return false;

            var offset = hit.position - request.TargetPosition;
            offset.y = 0f;

            if (offset.sqrMagnitude < request.MinDistance * request.MinDistance)
                return false;

            point = hit.position;

            return true;
        }

        private static Vector3 GetPreferredDirection(in ApproachRequest request)
        {
            var direction = request.OriginPosition - request.TargetPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < MIN_DIRECTION_SQR)
                return Vector3.forward;

            return direction.normalized;
        }
    }
}
