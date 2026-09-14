using UnityEngine;

namespace Watermelon
{
    public interface IGuardedRescueTarget
    {
        Transform Transform { get; }

        bool IsRescued { get; }

        bool IsRescueAreaUnlocked { get; }

        bool WaitForExternalRelease { get; }

        bool IsAwaitingRescue { get; }

        event SimpleCallback RescueAreaUnlocked;

        bool TryRelease();
    }
}
