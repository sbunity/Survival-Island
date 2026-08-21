using UnityEngine;

namespace Watermelon
{
    public interface IResourceCarrier
    {
        Transform CarrierTransform { get; }

        bool IsCarrierActive { get; }
    }
}
