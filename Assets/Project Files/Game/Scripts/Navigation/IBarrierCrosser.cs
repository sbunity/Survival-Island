using UnityEngine;

namespace Watermelon
{
    public readonly struct BarrierCrossing
    {
        public float AlongPath { get; }
        public float SecondsToReach { get; }

        public float Side { get; }

        public BarrierCrossing(float alongPath, float secondsToReach, float side)
        {
            AlongPath = alongPath;
            SecondsToReach = secondsToReach;
            Side = side;
        }
    }

    public interface IBarrierCrosser
    {
        Vector3 Position { get; }

        bool TryGetCrossing(IBarrier barrier, out BarrierCrossing crossing);
    }
}
