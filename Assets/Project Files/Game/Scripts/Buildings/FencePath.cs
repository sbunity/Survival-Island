using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public struct FencePath
    {
        private const float CURVATURE_EPSILON = 0.000001f;
        private const float MIN_LENGTH = 0.01f;

        [SerializeField] float startX;
        [SerializeField] float startZ;
        [SerializeField] float straightLength;
        [SerializeField] float curvature;
        [SerializeField] float arcLength;

        public float StartX => startX;
        public float StartZ => startZ;
        public float StraightLength => straightLength;
        public float Curvature => curvature;
        public float ArcLength => arcLength;

        public float TotalLength => straightLength + arcLength;
        public float Radius => Mathf.Abs(curvature) > CURVATURE_EPSILON ? 1f / curvature : 0f;
        public float SweepDegrees => curvature * arcLength * Mathf.Rad2Deg;

        public bool IsValid => TotalLength > MIN_LENGTH;

        private bool IsStraightOnly => Mathf.Abs(curvature) < CURVATURE_EPSILON;

        public static bool TryCreate(Vector3 start, Vector3 end, float straight, out FencePath path)
        {
            path = default;
            path.startX = start.x;
            path.startZ = start.z;
            path.straightLength = Mathf.Max(0f, straight);

            var deltaX = end.x - (path.startX + path.straightLength);
            var deltaZ = end.z - path.startZ;
            var chordSqr = deltaX * deltaX + deltaZ * deltaZ;

            if (chordSqr < CURVATURE_EPSILON)
                return false;

            path.curvature = -2f * deltaZ / chordSqr;

            if (Mathf.Abs(path.curvature) < CURVATURE_EPSILON)
            {
                path.curvature = 0f;
                path.arcLength = deltaX;
            }
            else
            {
                var angle = Mathf.Atan2(path.curvature * deltaX, 1f + path.curvature * deltaZ);

                if (angle * path.curvature < 0f)
                    angle += Mathf.Sign(path.curvature) * 2f * Mathf.PI;

                path.arcLength = angle / path.curvature;
            }

            return path.IsValid;
        }

        public void Sample(float distance, out float x, out float z, out float yawDegrees)
        {
            if (distance <= straightLength || IsStraightOnly)
            {
                x = startX + distance;
                z = startZ;
                yawDegrees = 0f;
                return;
            }

            var angle = curvature * (distance - straightLength);

            x = startX + straightLength + Mathf.Sin(angle) / curvature;
            z = startZ + (Mathf.Cos(angle) - 1f) / curvature;
            yawDegrees = angle * Mathf.Rad2Deg;
        }

        public Vector3 SamplePosition(float distance, float height)
        {
            Sample(distance, out var x, out var z, out _);
            return new Vector3(x, height, z);
        }

        public bool Project(Vector3 point, out float alongPath, out float sideOffset)
        {
            alongPath = 0f;
            sideOffset = 0f;

            if (!IsValid)
                return false;

            if (IsStraightOnly)
            {
                alongPath = Mathf.Clamp(point.x - startX, 0f, TotalLength);
                sideOffset = point.z - startZ;
                return true;
            }

            var radius = 1f / curvature;
            var toPointX = point.x - (startX + straightLength);
            var toPointZ = point.z - (startZ - radius);

            var angle = Mathf.Atan2(toPointX, toPointZ);
            var sweep = arcLength * curvature;

            if (angle * Mathf.Sign(sweep) <= 0f)
            {
                alongPath = Mathf.Clamp(point.x - startX, 0f, straightLength);
                sideOffset = point.z - startZ;
                return true;
            }

            var distanceToCentre = Mathf.Sqrt(toPointX * toPointX + toPointZ * toPointZ);

            alongPath = straightLength + Mathf.Clamp(angle * radius, 0f, arcLength);
            sideOffset = (distanceToCentre - Mathf.Abs(radius)) * Mathf.Sign(radius);

            return true;
        }

        public Vector3 GetNormal(float alongPath)
        {
            if (IsStraightOnly || alongPath <= straightLength)
                return new Vector3(0f, 0f, 1f);

            var angle = curvature * Mathf.Min(alongPath - straightLength, arcLength);

            return new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        }
    }
}
