using UnityEngine;

namespace Watermelon
{
    public readonly struct BarrierCrossing
    {
        public float AlongPath { get; }
        public float SecondsToReach { get; }

        public BarrierCrossing(float alongPath, float secondsToReach)
        {
            AlongPath = alongPath;
            SecondsToReach = secondsToReach;
        }
    }

    public interface IBarrierCrosser
    {
        Vector3 Position { get; }

        bool TryGetCrossing(IBarrier barrier, out BarrierCrossing crossing);
    }
}
