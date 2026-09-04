using UnityEngine;

namespace Watermelon
{
    public class TraderMinigameLauncher
    {
        private readonly MinigameStageDirector director = new MinigameStageDirector();

        private WanderingTraderBehavior trader;
        private TraderMinigameSlot slot;

        private bool holdsMovementLock;
        private bool hidTradePanel;
        private bool hidGameHud;

        private bool isRunning;
        public bool IsRunning => isRunning;

        public event SimpleCallback Finished;

        public bool TryLaunch(WanderingTraderBehavior trader)
        {
            if (isRunning || trader == null)
                return false;

            var slot = trader.MinigameSlot;
            if (slot == null || !slot.HasGame)
                return false;

            if (!UIController.HasPage<UIMinigameHost>())
            {
                Debug.LogError("[Trader Minigames]: UIMinigameHost is not registered in the UIController cached pages.");

                return false;
            }

            var stageType = ResolveStage(slot.Definition, trader, out var anchors);

            if (!slot.TryBeginPlay(out var context, out var blockReason))
            {
                if (!string.IsNullOrEmpty(blockReason))
                    Debug.Log($"[Trader Minigames]: Cannot start - {blockReason}");

                return false;
            }

            this.trader = trader;
            this.slot = slot;

            isRunning = true;

            AcquireLock();

            if (stageType != MinigameStageType.None)
            {
                hidTradePanel = true;

                if (UIController.HasPage<UITrader>())
                    UIController.HidePage<UITrader>();

                hidGameHud = true;

                SetGameHudVisible(false);
            }

            director.Enter(stageType, anchors, () => OpenHost(context));

            return true;
        }

        public void Abort()
        {
            director.Abort();

            isRunning = false;
            hidTradePanel = false;

            trader = null;
            slot = null;

            RestoreGameHud();
            ReleaseLock();
        }

        private static MinigameStageType ResolveStage(TraderMinigameDefinition definition, IMinigameStageProvider provider, out MinigameStageAnchors anchors)
        {
            anchors = null;

            var stageType = definition != null ? definition.Stage : MinigameStageType.None;

            if (stageType == MinigameStageType.None)
                return MinigameStageType.None;

            if (provider != null && provider.TryGetStage(stageType, out anchors))
                return stageType;

            Debug.LogWarning($"[Trader Minigames]: \"{(definition != null ? definition.name : "null")}\" asks for the {stageType} stage, but its anchors are not set up. Playing on screen only.", definition);

            return MinigameStageType.None;
        }

        private void OpenHost(MinigameContext context)
        {
            if (!isRunning || slot == null)
                return;

            var host = UIController.GetPage<UIMinigameHost>();

            host.Play(slot.Definition, slot.StakeRule, context, slot.CompletePlay, OnHostClosed);
        }

        private void OnHostClosed()
        {
            director.Exit(OnStageLeft);
        }

        private void OnStageLeft()
        {
            var trader = this.trader;
            var reopenPanel = hidTradePanel;

            isRunning = false;
            hidTradePanel = false;

            this.trader = null;
            slot = null;

            RestoreGameHud();

            Finished?.Invoke();

            if (reopenPanel)
                ReopenTradePanel(trader);

            ReleaseLock();
        }

        private void RestoreGameHud()
        {
            if (!hidGameHud)
                return;

            hidGameHud = false;

            SetGameHudVisible(true);
        }

        private static void SetGameHudVisible(bool isVisible)
        {
            if (!UIController.HasPage<UIGame>())
                return;

            UIController.GetPage<UIGame>().SetHudVisible(isVisible);
        }

        private static void ReopenTradePanel(WanderingTraderBehavior trader)
        {
            if (trader == null || !trader.CanInteract)
                return;

            if (!UIController.HasPage<UITrader>())
                return;

            var page = UIController.GetPage<UITrader>();
            page.SetTrader(trader);

            UIController.ShowPage<UITrader>();
        }

        private void AcquireLock()
        {
            if (holdsMovementLock)
                return;

            holdsMovementLock = true;

            MovementLock.Acquire();
            RaidSuppression.Acquire();
        }

        private void ReleaseLock()
        {
            if (!holdsMovementLock)
                return;

            holdsMovementLock = false;

            MovementLock.Release();
            RaidSuppression.Release();
        }
    }
}
