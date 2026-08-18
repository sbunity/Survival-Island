using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ShipyardBuilding : BuildingBehavior
    {
        [BoxFoldout("Ship Upgrades", "Ship Upgrades")]
        [SerializeField] ShipUpgradesDatabase upgradesDatabase;
        [BoxFoldout("Ship Upgrades", "Ship Upgrades")]
        [SerializeField] PurchasePoint upgradePurchasePoint;
        [BoxFoldout("Ship Upgrades", "Ship Upgrades")]
        [SerializeField] ConstructionPointBehavior upgradeConstructionPoint;
        [BoxFoldout("Ship Upgrades", "Ship Upgrades")]
        [SerializeField] ShipStageVisualBinder visualBinder;
        [BoxFoldout("Ship Upgrades", "Ship Upgrades")]
        [SerializeField] TMP_Text stageTitleText;

        public PurchasePoint UpgradePurchasePoint => upgradePurchasePoint;
        public ConstructionPointBehavior UpgradeConstructionPoint => upgradeConstructionPoint;

        public ShipUpgradeStage CurrentStage => activeStage?.Stage;

        public event System.Action<ShipUpgradeStage> StageApplied;

        private ShipUpgradeStageContext activeStage;
        private bool hasStartedUpgrades;

        protected override void RegisterUpgrades() { }

        public override void SpanwNotUnlocked()
        {
            base.SpanwNotUnlocked();

            CloseUpgradePoints();
        }

        protected override void OnOperationalStateChanged(bool isOperational)
        {
            if (isOperational)
                ResumeUpgrades();
            else
                SuspendUpgrades();
        }

        public override void OnWorldUnloaded()
        {
            base.OnWorldUnloaded();

            SuspendUpgrades();

            activeStage = null;
            hasStartedUpgrades = false;
        }

        #region Stage flow

        private void ResumeUpgrades()
        {
            hasStartedUpgrades = true;

            BeginNextStage();
        }

        private void SuspendUpgrades()
        {
            if (!hasStartedUpgrades)
                return;

            activeStage = null;

            if (upgradePurchasePoint != null)
                upgradePurchasePoint.Disable();

            if (upgradeConstructionPoint != null)
                upgradeConstructionPoint.Disable();

            SetStageTitle(null);
        }

        private void BeginNextStage()
        {
            var stage = ShipUpgradeState.GetNextStage(upgradesDatabase);

            if (stage == null)
            {
                activeStage = null;

                CloseUpgradePoints();

                return;
            }

            activeStage = new ShipUpgradeStageContext(this, stage);

            SetStageTitle(stage);

            if (upgradePurchasePoint != null)
            {
                activeStage.SetPurchasing(true);

                if (upgradePurchasePoint.Init(activeStage))
                {
                    if (upgradeConstructionPoint != null)
                        upgradeConstructionPoint.Disable();

                    return;
                }

                activeStage.SetPurchasing(false);
            }

            BeginStageConstruction();
        }

        private void BeginStageConstruction()
        {
            if (upgradeConstructionPoint == null)
            {
                CompleteStage();

                return;
            }

            activeStage.SetConstructing(true);

            upgradeConstructionPoint.Enable();

            if (!upgradeConstructionPoint.Init(activeStage))
            {
                activeStage.SetConstructing(false);

                CompleteStage();
            }
        }

        internal void OnStagePurchased(ShipUpgradeStageContext context)
        {
            if (context != activeStage)
                return;

            if (upgradePurchasePoint != null)
                upgradePurchasePoint.Complete();

            BeginStageConstruction();
        }

        internal void OnStageConstructed(ShipUpgradeStageContext context)
        {
            if (context != activeStage)
                return;

            if (upgradeConstructionPoint != null)
                upgradeConstructionPoint.Complete();

            CompleteStage();
        }

        private void CompleteStage()
        {
            var stage = activeStage.Stage;

            activeStage = null;

            bool wasAlreadyCompleted = ShipUpgradeState.IsCompleted(stage.ID);

            ShipUpgradeState.MarkCompleted(stage.ID);

            if (visualBinder != null)
                visualBinder.ApplyStage(stage, wasAlreadyCompleted);

            if (!wasAlreadyCompleted)
            {
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif

                AudioController.PlaySound(AudioController.GetClip("appear"));
            }

            StageApplied?.Invoke(stage);

            BeginNextStage();
        }

        private void CloseUpgradePoints()
        {
            if (upgradePurchasePoint != null)
                upgradePurchasePoint.Complete();

            if (upgradeConstructionPoint != null)
                upgradeConstructionPoint.Complete();

            SetStageTitle(null);
        }

        private void SetStageTitle(ShipUpgradeStage stage)
        {
            if (stageTitleText == null)
                return;

            var hasTitle = stage != null && !string.IsNullOrEmpty(stage.Title);

            stageTitleText.gameObject.SetActive(hasTitle);

            if (hasTitle)
                stageTitleText.text = stage.Title;
        }

        #endregion

        #region Editor

        public void UpdateUpgradeCostInEditor()
        {
            if (upgradesDatabase == null || upgradesDatabase.Stages.IsNullOrEmpty())
                return;

            var stage = upgradesDatabase.Stages[0];
            if (stage == null)
                return;

            if (upgradePurchasePoint != null)
                upgradePurchasePoint.UpdateCostInEditor(stage.RawCost);

            if (upgradeConstructionPoint != null)
                upgradeConstructionPoint.UpdateCostInEditor(stage.ConstructionHitsRequired);

            if (stageTitleText != null)
                stageTitleText.text = stage.Title;
        }

        #endregion
    }
}
