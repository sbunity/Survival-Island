using UnityEngine;

namespace Watermelon
{
    public class ShipwreckLootBehavior : MonoBehaviour, IResourceGiver, IWorldElement, IGroundOpenable
    {
        [UniqueID]
        [SerializeField] private string uniqueSaveID;

        [Header("Loot")]
        [SerializeField] private Resource[] initialResources;

        [Header("References")]
        [SerializeField] private ResourceGivingPointBehavior resourceGivingPoint;
        [SerializeField] private Transform resourceSpawnPoint;

        [Header("Settings")]
        [SerializeField, Min(0)] private float destroyDelay = 0.25f;

        public int InitialisationOrder => 0;
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public Vector3 FlyingResourceSpawnPosition => resourceSpawnPoint != null
            ? resourceSpawnPoint.position
            : transform.position + Vector3.up;

        public float LastTimeResourceGiven { get; private set; }
        public bool IsResourceGivingBlocked => !isInitialised || IsLooted || !isActiveAndEnabled;
        public bool IsPlayer => false;

        public bool IsLooted { get; private set; }
        public int RemainingResourcesAmount => GetRemainingResourcesAmount();

        public event SimpleCallback Looted;

        private ShipwreckLootSave save;
        private TweenCase destroyTweenCase;
        private bool isInitialised;

        private void Awake()
        {
            LinkGivingPoint();
        }

        public void OnWorldLoaded()
        {
            Initialise();
        }

        public void OnWorldUnloaded()
        {
            destroyTweenCase.KillActive();
        }

        private void Initialise()
        {
            if (isInitialised)
                return;

            if (resourceGivingPoint == null)
            {
                Debug.LogError($"[ShipwreckLootBehavior] Resource giving point is not assigned on '{name}'.", this);
                enabled = false;
                return;
            }

            if (string.IsNullOrEmpty(uniqueSaveID))
            {
                Debug.LogError($"[ShipwreckLootBehavior] Unique save ID is not assigned on '{name}'.", this);
                enabled = false;
                return;
            }

            LinkGivingPoint();

            var worldSave = LinkedWorldBehavior != null
                ? LinkedWorldBehavior.WorldSave
                : SaveController.GetFile(WorldController.CurrentWorld.ID);

            save = worldSave.GetSaveObject<ShipwreckLootSave>(uniqueSaveID);
            save.Initialise(CreateInitialResourcesList());

            isInitialised = true;
            IsLooted = save.IsLooted || !HasRemainingResources();

            if (IsLooted)
            {
                save.MarkAsLooted();
                gameObject.SetActive(false);
            }
        }

        private void LinkGivingPoint() 
            => resourceGivingPoint?.SetResourceGiver(this);

        public bool HasResource(Resource resource) 
            => resource.amount > 0 && GetResourceCount(resource.currency) >= resource.amount;

        public int GetResourceCount(CurrencyType currencyType)
        {
            if (save?.Resources == null)
                return 0;

            var amount = 0;

            for (var i = 0; i < save.Resources.Count; i++)
            {
                var resource = save.Resources[i];

                if (resource.currency == currencyType)
                    amount += resource.amount;
            }

            return amount;
        }

        public void GiveResource(Resource resource)
        {
            if (IsResourceGivingBlocked || resource.amount <= 0)
                return;

            var availableAmount = GetResourceCount(resource.currency);
            var amountToGive = Mathf.Min(resource.amount, availableAmount);

            if (amountToGive <= 0)
                return;

            save.SetResources(save.Resources - Resource.Create(resource.currency, amountToGive));
            LastTimeResourceGiven = Time.time;

            if (!HasRemainingResources())
                CompleteLooting();
        }

        public bool HasResources() 
            => isInitialised && !IsLooted && HasRemainingResources();

        private bool HasRemainingResources()
        {
            if (save?.Resources == null)
                return false;

            for (var i = 0; i < save.Resources.Count; i++)
            {
                if (save.Resources[i].amount > 0)
                    return true;
            }

            return false;
        }

        private int GetRemainingResourcesAmount()
        {
            if (save?.Resources == null)
                return 0;

            var amount = 0;

            for (var i = 0; i < save.Resources.Count; i++)
                amount += Mathf.Max(0, save.Resources[i].amount);

            return amount;
        }

        private ResourcesList CreateInitialResourcesList()
        {
            var resources = new ResourcesList();

            if (initialResources == null)
                return resources;

            for (var i = 0; i < initialResources.Length; i++)
            {
                var resource = initialResources[i];

                if (resource.amount > 0)
                    resources += resource;
            }

            return resources;
        }

        private void CompleteLooting()
        {
            if (IsLooted)
                return;

            IsLooted = true;
            save.MarkAsLooted();

            resourceGivingPoint.enabled = false;

            Looted?.Invoke();

            destroyTweenCase.KillActive();
            destroyTweenCase = Tween.DelayedCall(destroyDelay, () =>
            {
                if (gameObject != null)
                    Destroy(gameObject);
            });
        }

        public void OnGroundOpen(bool immediately = false)
        {
            if (!isInitialised && WorldController.CurrentWorld != null)
                Initialise();

            if (!IsLooted)
                gameObject.SetActive(true);
        }

        public void OnGroundHidden(bool immediately = false) 
            => gameObject.SetActive(false);

        private void OnDestroy()
        {
            destroyTweenCase.KillActive();
        }

        private void OnValidate()
        {
            destroyDelay = Mathf.Max(0, destroyDelay);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            Vector3 center = resourceSpawnPoint != null
                ? resourceSpawnPoint.position
                : transform.position + Vector3.up;

            Gizmos.DrawWireSphere(center, 0.25f);
        }
    }
}
