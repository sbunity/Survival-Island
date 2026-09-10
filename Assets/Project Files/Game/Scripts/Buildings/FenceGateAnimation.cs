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
    }

    [Serializable]
    public abstract class FenceGateAnimation
    {
        public abstract float Duration { get; }

        public abstract void Initialise(IReadOnlyList<Transform> logs);

        public abstract void SetLogOpen(int logIndex, bool isOpen, float distanceFromCentre);

        public abstract void SnapAllClosed();
    }
}
