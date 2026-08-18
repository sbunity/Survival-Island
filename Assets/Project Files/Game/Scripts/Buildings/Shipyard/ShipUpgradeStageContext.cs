using UnityEngine;

namespace Watermelon
{
    public sealed class ShipUpgradeStageContext : IUnlockableComplex
    {
        private readonly ShipyardBuilding shipyard;
        private readonly ShipUpgradeStage stage;

        public ShipUpgradeStage Stage => stage;

        public string ID { get; }
        public ResourcesList Cost { get; }

        public int ConstructionHitsRequired => stage.ConstructionHitsRequired;

        public Vector3 Position => shipyard.transform.position;

        public bool CanBePurchased { get; private set; }
        public bool CanBeConstructed { get; private set; }

        public ResourcesList CostLeft
        {
            get
            {
                var purchasePoint = shipyard.UpgradePurchasePoint;

                return CanBePurchased && purchasePoint != null && purchasePoint.CostLeft != null
                    ? purchasePoint.CostLeft
                    : Cost;
            }
        }

        public int HitsMade
        {
            get
            {
                var constructionPoint = shipyard.UpgradeConstructionPoint;

                return CanBeConstructed && constructionPoint != null ? constructionPoint.HitsMade : 0;
            }
        }

        public ShipUpgradeStageContext(ShipyardBuilding shipyard, ShipUpgradeStage stage)
        {
            this.shipyard = shipyard;
            this.stage = stage;

            ID = $"{shipyard.ID}_{stage.ID}";
            Cost = stage.CreateCost();
        }

        #region Purchase

        public void Purchase()
        {
            if (!CanBePurchased)
                return;

            CanBePurchased = false;

            shipyard.OnStagePurchased(this);
        }

        public void EnablePurchase()
        {
            if (shipyard.UpgradePurchasePoint == null)
                return;

            CanBePurchased = true;
            shipyard.UpgradePurchasePoint.Enable();
        }

        public void DisablePurchase()
        {
            CanBePurchased = false;

            if (shipyard.UpgradePurchasePoint != null)
                shipyard.UpgradePurchasePoint.Disable();
        }

        #endregion

        #region Construction

        public void Construct()
        {
            if (!CanBeConstructed)
                return;

            CanBeConstructed = false;

            shipyard.OnStageConstructed(this);
        }

        public void EnableConstructing()
        {
            if (shipyard.UpgradeConstructionPoint == null)
                return;

            CanBeConstructed = true;
            shipyard.UpgradeConstructionPoint.Enable();
        }

        public void DisableCounstructing()
        {
            CanBeConstructed = false;

            if (shipyard.UpgradeConstructionPoint != null)
                shipyard.UpgradeConstructionPoint.Disable();
        }

        public Sprite GetConstrutionIcon() 
            => shipyard.UpgradeConstructionPoint != null
                ? shipyard.UpgradeConstructionPoint.GetConstructionIcon()
                : null;

        #endregion

        #region Stage flow

        internal void SetPurchasing(bool value)
        {
            CanBePurchased = value;
        }

        internal void SetConstructing(bool value)
        {
            CanBeConstructed = value;
        }

        #endregion

        #region Events

        public bool SubscribeOnPurchased(SimpleCallback callback)
        {
            if (shipyard.UpgradePurchasePoint == null)
                return false;

            shipyard.UpgradePurchasePoint.OnPurhcased += callback;

            return true;
        }

        public bool UnsubscribeOnPurchased(SimpleCallback callback)
        {
            if (shipyard.UpgradePurchasePoint == null)
                return false;

            shipyard.UpgradePurchasePoint.OnPurhcased -= callback;

            return true;
        }

        public bool SubscribeOnResourcePlaced(SimpleCallback callback)
        {
            if (shipyard.UpgradePurchasePoint == null)
                return false;

            shipyard.UpgradePurchasePoint.OnResourcePlaced += callback;

            return true;
        }

        public bool UnsubscribeOnResourcePlaced(SimpleCallback callback)
        {
            if (shipyard.UpgradePurchasePoint == null)
                return false;

            shipyard.UpgradePurchasePoint.OnResourcePlaced -= callback;

            return true;
        }

        public bool SubscribeOnConstructed(SimpleCallback callback)
        {
            if (shipyard.UpgradeConstructionPoint == null)
                return false;

            shipyard.UpgradeConstructionPoint.OnConstructed += callback;

            return true;
        }

        public bool UnsubscribeOnConstructed(SimpleCallback callback)
        {
            if (shipyard.UpgradeConstructionPoint == null)
                return false;

            shipyard.UpgradeConstructionPoint.OnConstructed -= callback;

            return true;
        }

        public bool SubscribeOnGotHit(SimpleCallback callback)
        {
            if (shipyard.UpgradeConstructionPoint == null)
                return false;

            shipyard.UpgradeConstructionPoint.OnGotHit += callback;

            return true;
        }

        public bool UnsubscribeOnGotHit(SimpleCallback callback)
        {
            if (shipyard.UpgradeConstructionPoint == null)
                return false;

            shipyard.UpgradeConstructionPoint.OnGotHit -= callback;

            return true;
        }

        public bool SubscribeOnFullyUnlocked(SimpleCallback callback)
        {
            return SubscribeOnConstructed(callback) || SubscribeOnPurchased(callback);
        }

        public bool UnsubscribeOnFullyUnlocked(SimpleCallback callback)
        {
            return UnsubscribeOnConstructed(callback) || UnsubscribeOnPurchased(callback);
        }

        #endregion
    }
}
