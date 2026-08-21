using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceTakingPointBehavior : ResourcePointBehavior
    {
        public IResourceTaker ResourceTaker { get; private set; }

        private readonly Dictionary<IResourceGiver, CarrierState> resourceGivers = new Dictionary<IResourceGiver, CarrierState>();

        private readonly List<IResourceGiver> iterationBuffer = new List<IResourceGiver>();
        private readonly List<IResourceGiver> removalBuffer = new List<IResourceGiver>();
        private readonly List<ShuffledResData> availableRequiredResources = new List<ShuffledResData>();

        public int CarriersCount => resourceGivers.Count;

        [Space]
        [SerializeField] protected bool overrideResourceDestination;
        [SerializeField, ShowIf("overrideResourceDestination")] protected Transform resourceDestinationPoint;

        public Vector3 ResourceDestination => overrideResourceDestination ? resourceDestinationPoint.position : (transform.position + Vector3.up);

        public void SetResourceTaker(IResourceTaker resourceTaker)
        {
            ResourceTaker = resourceTaker;
        }

        private void Update()
        {
            if (ResourceTaker == null)
                return;

            RefreshValidation();
            PruneCarriers();

            if (resourceGivers.Count == 0)
                return;

            if (ResourceTaker.RequiredResources.IsNullOrEmpty())
                return;

            iterationBuffer.Clear();
            foreach (var giver in resourceGivers.Keys)
            {
                iterationBuffer.Add(giver);
            }

            for (int i = 0; i < iterationBuffer.Count; i++)
            {
                IResourceGiver giver = iterationBuffer[i];

                if (!resourceGivers.TryGetValue(giver, out CarrierState state))
                    continue;

                if (Time.time - giver.LastTimeResourceGiven < cooldown)
                    continue;
                if (giver.IsResourceGivingBlocked)
                    continue;

                if (ResourceTaker.RequiredResources == null) break;

                availableRequiredResources.Clear();

                foreach (var requiredResourceType in ResourceTaker.RequiredResources)
                {
                    float takingSpeedUpStage = state.TransfersCount / 4;
                    var nextAmount = (int)(2 * takingSpeedUpStage);
                    if (nextAmount < 1)
                        nextAmount = 1;

                    var availableResources = Mathf.Min(giver.GetResourceCount(requiredResourceType), ResourceTaker.RequiredMaxAmount(requiredResourceType));
                    if (availableResources > 0) availableRequiredResources.Add(new ShuffledResData { type = requiredResourceType, availableResources = availableResources, amount = nextAmount });
                }

                availableRequiredResources.Shuffle();

                foreach (var requiredResource in availableRequiredResources)
                {
                    var nextAmount = Mathf.Clamp(requiredResource.amount, 1, requiredResource.availableResources);

                    var one = Resource.Create(requiredResource.type, nextAmount);

                    if (giver.HasResource(one))
                    {
                        giver.GiveResource(one);

                        FlyingResourceBehavior flyingResource = CurrencyController.GetCurrency(requiredResource.type).Data.FlyingResPool.GetPooledComponent();

                        flyingResource.InitAtPosition(giver.FlyingResourceSpawnPosition, nextAmount);

                        ResourceTaker.TakeResource(flyingResource, giver.IsPlayer);

                        state.TransfersCount++;
                        resourceGivers[giver] = state;

                        break;
                    }
                }
            }
        }

        protected override void PruneCarriers()
        {
            if (resourceGivers.Count == 0)
                return;

            removalBuffer.Clear();

            foreach (var pair in resourceGivers)
            {
                if (!IsCarrierValid(pair.Value.Carrier))
                    removalBuffer.Add(pair.Key);
            }

            for (int i = 0; i < removalBuffer.Count; i++)
            {
                resourceGivers.Remove(removalBuffer[i]);
            }
        }

        protected override void ClearCarriers()
        {
            resourceGivers.Clear();
        }

        public override void ForceRemoveCarrier(IResourceCarrier carrier)
        {
            if (carrier == null || resourceGivers.Count == 0)
                return;

            removalBuffer.Clear();

            foreach (var pair in resourceGivers)
            {
                if (ReferenceEquals(pair.Value.Carrier, carrier))
                    removalBuffer.Add(pair.Key);
            }

            for (int i = 0; i < removalBuffer.Count; i++)
            {
                resourceGivers.Remove(removalBuffer[i]);
            }
        }

        protected override void AddResourceCarrier(GameObject carrierObject)
        {
            var resourceGiver = carrierObject.GetComponent<IResourceGiver>();

            if (resourceGiver == null)
            {
                Debug.LogError($"Game Object {carrierObject.name} does not implement IResourceGiver interface", carrierObject);

                return;
            }

            if (resourceGivers.ContainsKey(resourceGiver))
                return;

            IResourceCarrier carrier = ResolveCarrier(carrierObject);

            if (carrier == null)
                return;

            resourceGivers.Add(resourceGiver, new CarrierState(carrier));
        }

        protected override void RemoveResourceCarrier(GameObject carrierObject)
        {
            var resourceGiver = carrierObject.GetComponent<IResourceGiver>();

            if (resourceGiver == null)
            {
                Debug.LogError($"Game Object {carrierObject.name} does not implement IResourceGiver interface", carrierObject);

                return;
            }

            resourceGivers.Remove(resourceGiver);
        }

        private struct ShuffledResData
        {
            public CurrencyType type;
            public int availableResources;
            public int amount;
        }

        #region Editor

        protected bool ShowCreateResourceDestinationPointButton()
        {
            return overrideResourceDestination && resourceDestinationPoint == null;
        }

        [Button(visibilityMethodName: "ShowCreateResourceDestinationPointButton")]
        protected void CreateCustomResourceDestinationPoint()
        {
            Transform newObjectTransform = new GameObject("Resource Destination").transform;
            newObjectTransform.position = transform.position + Vector3.up;
            newObjectTransform.SetParent(transform);

            resourceDestinationPoint = newObjectTransform;

            RuntimeEditorUtils.SetDirty(newObjectTransform);
            RuntimeEditorUtils.SetDirty(this);
        }

        #endregion
    }
}
