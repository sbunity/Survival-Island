using UnityEngine;

namespace Watermelon
{
    public class UIBaseAttackBanner : UIHudBanner
    {
        private BaseWorldBehavior subscribedWorld;

        protected override void OnInitialise()
        {
            WorldController.OnWorldLoaded += OnWorldLoaded;

            SubscribeToCurrentWorld();
        }

        protected override void OnUnload()
        {
            WorldController.OnWorldLoaded -= OnWorldLoaded;

            UnsubscribeFromWorld();
        }

        protected override void OnClicked()
        {
            if (subscribedWorld == null)
                return;

            var attackController = subscribedWorld.AttackController;
            if (attackController == null || !attackController.IsAlertActive)
                return;

            var player = PlayerBehavior.GetBehavior();
            if (player == null)
                return;

            FocusCamera(attackController.GetNearestDefensePosition(player.transform.position));
        }

        private void OnWorldLoaded()
        {
            SubscribeToCurrentWorld();

            HideImmediately();
        }

        private void SubscribeToCurrentWorld()
        {
            var world = WorldController.WorldBehavior;
            if (world == subscribedWorld)
                return;

            UnsubscribeFromWorld();

            subscribedWorld = world;

            if (subscribedWorld != null)
            {
                subscribedWorld.BaseUnderAttack += Show;
                subscribedWorld.BaseAttackEnded += Hide;
            }
        }

        private void UnsubscribeFromWorld()
        {
            if (subscribedWorld == null)
                return;

            subscribedWorld.BaseUnderAttack -= Show;
            subscribedWorld.BaseAttackEnded -= Hide;

            subscribedWorld = null;
        }
    }
}
