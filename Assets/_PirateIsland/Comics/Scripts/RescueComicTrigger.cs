using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    public class RescueComicTrigger : ComicTrigger
    {
        [SerializeField, FormerlySerializedAs("helper")] MonoBehaviour rescueTarget;

        private IGuardedRescueTarget Target => rescueTarget as IGuardedRescueTarget;

        protected override bool CanArm() => Target != null && !Target.IsRescued;

        protected override void Subscribe() => Target.Rescued += Fire;

        protected override void Unsubscribe() => Target.Rescued -= Fire;

        private void OnValidate()
        {
            if (rescueTarget != null && !(rescueTarget is IGuardedRescueTarget))
                rescueTarget = rescueTarget.GetComponent<IGuardedRescueTarget>() as MonoBehaviour;
        }
    }
}
