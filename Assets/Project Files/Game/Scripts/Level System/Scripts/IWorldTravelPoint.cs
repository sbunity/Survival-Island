using UnityEngine;

namespace Watermelon
{
    public interface IWorldTravelPoint
    {
        Vector3 TravelPointPosition { get; }

        bool IsTravelPointAvailable { get; }
    }
}
