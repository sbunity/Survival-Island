using UnityEngine;

namespace Watermelon
{
    public class FenceBuildingBehavior : BuildingBehavior
    {
        [BoxFoldout("Fence", "Fence")]
        [SerializeField] FenceGateController gateController;

        private TweenCase gateRebindCase;

        protected override void RegisterUpgrades()
        {

        }

        public override void FullyUnlock()
        {
            var wasDestroyed = IsDestroyed;

            base.FullyUnlock();

            var hasAppearAnimation = !wasDestroyed && unlockAnimation != null;

            RebindGate(hasAppearAnimation ? unlockAnimation.TotalAnimationDuration : 0f);
        }

        public override void OnWorldUnloaded()
        {
            base.OnWorldUnloaded();

            gateRebindCase.KillActive();
        }

        private void RebindGate(float delay)
        {
            gateRebindCase.KillActive();

            if (gateController == null)
                return;

            if (delay <= 0f)
            {
                gateController.Rebind();

                return;
            }

            gateController.Unbind();

            gateRebindCase = Tween.DelayedCall(delay, () =>
            {
                if (gateController != null)
                    gateController.Rebind();
            });
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            gateRebindCase.KillActive();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            gateRebindCase.KillActive();
        }
    }
}
