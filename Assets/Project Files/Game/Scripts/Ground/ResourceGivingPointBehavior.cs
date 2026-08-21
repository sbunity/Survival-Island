using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceGivingPointBehavior : ResourcePointBehavior
    {
        public IResourceGiver ResourceGiver { get; private set; }

        private readonly Dictionary<IResourceTaker, CarrierState> resourceTakers = new Dictionary<IResourceTaker, CarrierState>();

        private readonly List<IResourceTaker> iterationBuffer = new List<IResourceTaker>();
        private readonly List<IResourceTaker> removalBuffer = new List<IResourceTaker>();

        public int CarriersCount => resourceTakers.Count;

        [Space]
        [SerializeField] protected bool overrideResourceSpawnPoint;
        [SerializeField, ShowIf("overrideResourceSpawnPoint")] protected Transform resourceSpawnPoint;

        public Vector3 ResourceSpawnPosition => overrideResourceSpawnPoint ? resourceSpawnPoint.position : transform.position;

        public void SetResourceGiver(IResourceGiver resourceGiver)
        {
            ResourceGiver = resourceGiver;
        }

        private CurrencyType GetAvailableResourceType()
        {
            foreach (CurrencyType currencyType in System.Enum.GetValues(typeof(CurrencyType)))
            {
                if (ResourceGiver.GetResourceCount(currencyType) > 0) return currencyType;
            }

            return default;
        }

        private void Update()
        {
            if (ResourceGiver == null)
                return;

            RefreshValidation();
            PruneCarriers();

            if (resourceTakers.Count == 0)
                return;

            if (Time.time - ResourceGiver.LastTimeResourceGiven <= cooldown)
                return;

            if (ResourceGiver.IsResourceGivingBlocked)
                return;

            iterationBuffer.Clear();
            foreach (var taker in resourceTakers.Keys)
            {
                iterationBuffer.Add(taker);
            }

            for (int i = 0; i < iterationBuffer.Count; i++)
            {
                IResourceTaker taker = iterationBuffer[i];

                if (!resourceTakers.TryGetValue(taker, out CarrierState state))
                    continue;

                if (taker.IsResourceTakingBlocked || taker.RequiredResources == null) continue;

                foreach (var requiredResourceType in taker.RequiredResources)
                {
                    float takingSpeedUpStage = state.TransfersCount / 4;
                    int nextAmount = (int)(2 * takingSpeedUpStage);
                    if (nextAmount < 1) nextAmount = 1;

                    int availableResources = Mathf.Min(ResourceGiver.GetResourceCount(requiredResourceType), taker.RequiredMaxAmount(requiredResourceType));
                    if (availableResources <= 0) continue;
                    nextAmount = Mathf.Clamp(nextAmount, 1, availableResources);

                    Resource one = Resource.Create(requiredResourceType, nextAmount);

                    if (ResourceGiver.HasResource(one))
                    {
                        if (!taker.CanTakeResource(ref one)) continue;

                        ResourceGiver.GiveResource(one);

                        FlyingResourceBehavior flyingResource = CurrencyController.GetCurrency(requiredResourceType).Data.FlyingResPool.GetPooledComponent();

                        flyingResource.InitAtPosition(ResourceGiver.FlyingResourceSpawnPosition, nextAmount);

                        taker.TakeResource(flyingResource, ResourceGiver.IsPlayer);

                        state.TransfersCount++;
                        resourceTakers[taker] = state;

                        return;
                    }
                }

                if (ResourceGiver.HasResources()) taker.Rejected(GetAvailableResourceType());
            }
        }

        protected override void PruneCarriers()
        {
            if (resourceTakers.Count == 0)
                return;

            removalBuffer.Clear();

            foreach (var pair in resourceTakers)
            {
                if (!IsCarrierValid(pair.Value.Carrier))
                    removalBuffer.Add(pair.Key);
            }

            for (int i = 0; i < removalBuffer.Count; i++)
            {
                resourceTakers.Remove(removalBuffer[i]);
            }
        }

        protected override void ClearCarriers()
        {
            resourceTakers.Clear();
        }

        public override void ForceRemoveCarrier(IResourceCarrier carrier)
        {
            if (carrier == null || resourceTakers.Count == 0)
                return;

            removalBuffer.Clear();

            foreach (var pair in resourceTakers)
            {
                if (ReferenceEquals(pair.Value.Carrier, carrier))
                    removalBuffer.Add(pair.Key);
            }

            for (int i = 0; i < removalBuffer.Count; i++)
            {
                resourceTakers.Remove(removalBuffer[i]);
            }
        }

        protected override void AddResourceCarrier(GameObject carrierObject)
        {
            var resourceTaker = carrierObject.GetComponent<IResourceTaker>();

            if (resourceTaker == null)
            {
                Debug.LogError($"Game Object {carrierObject.name} does not implement IResourceTaker interface", carrierObject);

                return;
            }

            if (resourceTakers.ContainsKey(resourceTaker))
                return;

            IResourceCarrier carrier = ResolveCarrier(carrierObject);

            if (carrier == null)
                return;

            resourceTakers.Add(resourceTaker, new CarrierState(carrier));
        }

        protected override void RemoveResourceCarrier(GameObject carrierObject)
        {
            var resourceTaker = carrierObject.GetComponent<IResourceTaker>();

            if (resourceTaker == null)
            {
                Debug.LogError($"Game Object {carrierObject.name} does not implement IResourceTaker interface", carrierObject);

                return;
            }

            resourceTakers.Remove(resourceTaker);
        }

        #region Editor

        protected bool ShowCreateCustomGavedResourceSpawnPointButton()
        {
            return overrideResourceSpawnPoint && resourceSpawnPoint == null;
        }

        [Button(visibilityMethodName: "ShowCreateCustomGavedResourceSpawnPointButton")]
        protected void CreateCustomGavedResourceSpawnPoint()
        {
            Transform newObjectTransform = new GameObject("Resource Spawn Position").transform;
            newObjectTransform.position = transform.position + Vector3.up;
            newObjectTransform.SetParent(transform);

            resourceSpawnPoint = newObjectTransform;

            RuntimeEditorUtils.SetDirty(newObjectTransform);
            RuntimeEditorUtils.SetDirty(this);
        }

        #endregion
    }
}
