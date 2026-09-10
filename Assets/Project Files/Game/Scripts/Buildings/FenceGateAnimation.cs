using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public enum FenceGateAnimationType
    {
        None = 0,
        Sink = 1,
        Fade = 2,
        Rotate = 3,
    }

    public readonly struct FenceLogOpenContext
    {
        public float DistanceFromCentre { get; }

        public float CrossingSide { get; }

        public FenceLogOpenContext(float distanceFromCentre, float crossingSide)
        {
            DistanceFromCentre = distanceFromCentre;
            CrossingSide = crossingSide;
        }
    }

    [Serializable]
    public abstract class FenceGateAnimation
    {
        public abstract float Duration { get; }

        public abstract void Initialise(IReadOnlyList<Transform> logs);

        public abstract void SetLogOpen(int logIndex, bool isOpen, in FenceLogOpenContext context);

        public abstract void SnapAllClosed();
    }
}
