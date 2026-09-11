using UnityEngine;

namespace Watermelon
{
    public readonly struct BarrierCrossing
    {
        public float AlongPath { get; }
        public float SecondsToReach { get; }

        public float Side { get; }

        public float SlideDirection { get; }

        public BarrierCrossing(float alongPath, float secondsToReach, float side, float slideDirection = 0f)
        {
            AlongPath = alongPath;
            SecondsToReach = secondsToReach;
            Side = side;
            SlideDirection = slideDirection;
        }
    }

    public interface IBarrierCrosser
    {
        Vector3 Position { get; }

        bool TryGetCrossing(IBarrier barrier, out BarrierCrossing crossing);
    }
}
