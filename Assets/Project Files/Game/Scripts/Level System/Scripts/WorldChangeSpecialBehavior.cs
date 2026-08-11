using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class WorldChangeSpecialBehavior : MonoBehaviour
    {
        public abstract void OnGroundTileOpened(bool immediately);
        public abstract void OnWorldChanged(SimpleCallback worldChangeCallback);

        public virtual void SetPassengers(IReadOnlyList<IRaftPassenger> passengers)
        {

        }
    }
}
