using System;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    [RequireComponent(typeof(HealthBehavior))]
    public abstract class BuildingBehavior : MonoBehaviour, IUnlockable, IWorldElement, ICombatTarget
    {
        public int InitialisationOrder => 2;

        public string ID { get; private set; }

        [BoxFoldout("Health", "Health")]
        [SerializeField, Min(1f)] float maxHealth = 100f;
        [BoxFoldout("Health", "Health")]
        [SerializeField] HealthBehavior healthBehavior;
        [BoxFoldout("Health", "Health")]
        [SerializeField] bool enablePassiveRegeneration = true;
        [BoxFoldout("Health", "Health")]
        [SerializeField, Min(0f)] float regenerationDelay = 5f;
        [BoxFoldout("Health", "Health")]
        [SerializeField, Min(0f)] float regenerationPerSecond = 1f;
        [BoxFoldout("Health", "Health")]
        [SerializeField] Collider targetCollider;
        [BoxFoldout("Health", "Health")]
        [SerializeField] List<Transform> attackPoints = new List<Transform>();

        [BoxFoldout("Upgrades", "Upgrades")]
        [SerializeField] protected List<BuildingUpgradeContainer> buildingUpgrades;
        public List<BuildingUpgradeContainer> BuildingUpgrades => buildingUpgrades;

        [BoxFoldout("Upgrades", "Upgrades")]
        [SerializeField, ShowIf("EditorHaveUpgrades")] protected UpgradesTrigger upgradesTrigger;
        [BoxFoldout("Upgrades", "Upgrades")]
        [SerializeField, ShowIf("EditorHaveUpgrades")] protected bool showUpgradesOnMainPage;

        [BoxFoldout("Visuals", "Visuals")]
        [SerializeField] GameObject openedVisuals;
        [BoxFoldout("Visuals", "Visuals")]
        [SerializeField] GameObject closedVisuals;

        [BoxFoldout("Visuals", "Visuals")]
        [SerializeField] protected AnimationForUnlockable unlockAnimation;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public CombatFaction Faction => CombatFaction.Friendly;
        public CombatTargetType TargetType => CombatTargetType.Building;
        public bool CanBeTargeted => IsOperational && isActiveAndEnabled && gameObject.activeInHierarchy && healthBehavior != null && !healthBehavior.IsDepleted;

        public Transform Transform => transform;
        public bool IsDead => IsDestroyed;
        public bool IsDestroyed { get; private set; }
        public bool IsOperational { get; private set; }

        public float CurrentHealth => healthBehavior != null ? healthBehavior.CurrentHealth : 0f;
        public float MaxHealth => healthBehavior != null ? healthBehavior.MaxHealth : maxHealth;
        public float HealthNormalized => MaxHealth > 0f ? CurrentHealth / MaxHealth : 0f;

        public event SimpleCallback Damaged;
        public event SimpleCallback Destroyed;
        public event SimpleCallback Rebuilt;
        public event SimpleCallback HealthNormalizedChanged;
        public event Action<DamageSource> Attacked;

        private BuildingComplexBehavior complex;
        private ConstructingPointSave buildingSave;
        private Collider[] fallbackTargetColliders;

        private bool isRuntimeInitialised;
        private bool isHealthInitialised;
        private bool isOpen;

        protected abstract void RegisterUpgrades();

        protected virtual void Init()
        {
            if (!buildingUpgrades.IsNullOrEmpty())
            {
                RegisterUpgrades();

                if (upgradesTrigger != null)
                {
                    upgradesTrigger.RegisterUpgrades(buildingUpgrades.ConvertAll((upgrade) => (IUpgrade)upgrade.Upgrade));
                }

                if (showUpgradesOnMainPage)
                {
                    for (int i = 0; i < buildingUpgrades.Count; i++)
                    {
                        GlobalUpgradesController.RegisterSimpleUpgrade(buildingUpgrades[i].Upgrade);
                    }
                }
            }
            else if (upgradesTrigger != null)
            {
                upgradesTrigger.gameObject.SetActive(false);
            }
        }

        public virtual void OnWorldLoaded()
        {
            InitialiseHealth();
        }

        public virtual void OnWorldUnloaded()
        {
            SetOperational(false);
            CombatTargetRegistry.Unregister(this);

            if (healthBehavior != null)
                healthBehavior.ForceHide();
        }

        public virtual void SpawnUnlocked()
        {
            InitialiseHealth();
            EnsureRuntimeInitialised();

            if (IsDestroyed)
            {
                SpawnDestroyed();
                return;
            }

            SetVisuals(true);
            isOpen = true;
            SetOperational(true);
            healthBehavior.Show();
        }

        public virtual void SpanwNotUnlocked()
        {
            InitialiseHealth();

            SetVisuals(false);
            isOpen = false;
            SetOperational(false);
            healthBehavior.ForceHide();
        }

        public void SpawnDestroyed()
        {
            InitialiseHealth();
            EnsureRuntimeInitialised();

            SetVisuals(true);
            isOpen = false;
            SetOperational(false);
            healthBehavior.ForceHide();
        }

        public virtual void FullyUnlock()
        {
            InitialiseHealth();
            EnsureRuntimeInitialised();

            var wasDestroyed = IsDestroyed;

            if (!wasDestroyed && unlockAnimation != null)
                unlockAnimation.RunUnlockedAnimation();

            SetVisuals(true);

            if (wasDestroyed)
            {
                IsDestroyed = false;
                healthBehavior.Restore();

                buildingSave.HasHealthData = true;
                buildingSave.CurrentHealth = healthBehavior.CurrentHealth;
                buildingSave.IsDestroyed = false;
            }

            isOpen = true;
            SetOperational(true);
            healthBehavior.Show();

            if (wasDestroyed)
            {
                Rebuilt?.Invoke();
                return;
            }

            Tween.DelayedCall(unlockAnimation != null ? unlockAnimation.TotalAnimationDuration : 0f, () =>
            {
                NavMeshController.CalculateNavMesh();
            });

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif

            AudioController.PlaySound(AudioController.GetClip("appear"));
        }

        public void TakeDamage(DamageSource source, Vector3 position, bool shouldFlash = false)
        {
            if (!CanBeTargeted || source == null || source.Damage <= 0f)
                return;

            var previousHealth = healthBehavior.CurrentHealth;

            Attacked?.Invoke(source);
            healthBehavior.Subtract(source.Damage);

            if (Mathf.Approximately(previousHealth, healthBehavior.CurrentHealth))
                return;

            Damaged?.Invoke();

            if (healthBehavior.IsDepleted)
                EnterDestroyedState();
        }

        public Vector3 GetAttackPosition(Vector3 attackerPosition)
        {
            Transform nearestPoint = null;
            var nearestDistanceSqr = float.MaxValue;

            for (var i = 0; attackPoints != null && i < attackPoints.Count; i++)
            {
                var point = attackPoints[i];
                if (point == null || !point.gameObject.activeInHierarchy)
                    continue;

                var distanceSqr = (point.position - attackerPosition).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestPoint = point;
                }
            }

            if (nearestPoint != null)
                return nearestPoint.position;

            if (targetCollider != null && targetCollider.enabled)
                return targetCollider.ClosestPoint(attackerPosition);

            var fallbackPosition = transform.position;
            var fallbackDistanceSqr = float.MaxValue;

            if (fallbackTargetColliders != null)
            {
                for (var i = 0; i < fallbackTargetColliders.Length; i++)
                {
                    var collider = fallbackTargetColliders[i];
                    if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
                        continue;

                    var position = collider.ClosestPoint(attackerPosition);
                    var distanceSqr = (position - attackerPosition).sqrMagnitude;
                    if (distanceSqr < fallbackDistanceSqr)
                    {
                        fallbackDistanceSqr = distanceSqr;
                        fallbackPosition = position;
                    }
                }
            }

            return fallbackPosition;
        }

        public void SetID(string id)
        {
            ID = id;
        }

        public void SetComplex(BuildingComplexBehavior buildingComplex)
        {
            complex = buildingComplex;
        }

        protected virtual void OnOperationalStateChanged(bool isOperational) { }

        private void InitialiseHealth()
        {
            if (isHealthInitialised)
                return;

            if (healthBehavior == null)
                healthBehavior = GetComponent<HealthBehavior>();

            if (healthBehavior == null)
                healthBehavior = gameObject.AddComponent<HealthBehavior>();

            fallbackTargetColliders = openedVisuals != null
                ? openedVisuals.GetComponentsInChildren<Collider>(true)
                : Array.Empty<Collider>();

            buildingSave = SaveController.GetSaveObject<ConstructingPointSave>(LinkedWorldBehavior.WorldData.ID, ID + "_building_point");

            var currentHealth = buildingSave.HasHealthData ? buildingSave.CurrentHealth : maxHealth;
            IsDestroyed = buildingSave.HasHealthData && (buildingSave.IsDestroyed || currentHealth <= 0f);

            healthBehavior.Initialise(maxHealth, IsDestroyed ? 0f : currentHealth);
            healthBehavior.ConfigureRegeneration(enablePassiveRegeneration, regenerationDelay, regenerationPerSecond);
            healthBehavior.ShowOnChange = true;
            healthBehavior.HideOnFull = true;
            healthBehavior.HealthChanged -= OnHealthChanged;
            healthBehavior.HealthChanged += OnHealthChanged;

            if (IsDestroyed)
                healthBehavior.ForceHide();

            isHealthInitialised = true;
        }

        private void EnsureRuntimeInitialised()
        {
            if (isRuntimeInitialised)
                return;

            isRuntimeInitialised = true;
            Init();
            OnOperationalStateChanged(IsOperational);
        }

        private void EnterDestroyedState()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;
            isOpen = false;

            buildingSave.HasHealthData = true;
            buildingSave.CurrentHealth = 0f;
            buildingSave.IsDestroyed = true;

            SetOperational(false);
            healthBehavior.ForceHide();
            CombatTargetRegistry.Unregister(this);

            Destroyed?.Invoke();

            if (complex != null)
                complex.BeginReconstruction();
            else
                Debug.LogError($"[Building] '{name}' cannot start reconstruction without BuildingComplexBehavior.", this);
        }

        private void OnHealthChanged()
        {
            if (!isHealthInitialised || buildingSave == null)
                return;

            buildingSave.HasHealthData = true;
            buildingSave.CurrentHealth = healthBehavior.CurrentHealth;
            buildingSave.IsDestroyed = IsDestroyed;

            HealthNormalizedChanged?.Invoke();
        }

        private void SetOperational(bool value)
        {
            var newValue = value && isOpen && !IsDestroyed;
            if (IsOperational == newValue)
            {
                RefreshRegistry();
                return;
            }

            IsOperational = newValue;
            RefreshRegistry();
            OnOperationalStateChanged(IsOperational);
        }

        private void RefreshRegistry()
        {
            if (CanBeTargeted)
                CombatTargetRegistry.Register(this);
            else
                CombatTargetRegistry.Unregister(this);
        }

        private void SetVisuals(bool opened)
        {
            if (openedVisuals != null)
                openedVisuals.SetActive(opened);

            if (closedVisuals != null)
                closedVisuals.SetActive(!opened);
        }

        protected virtual void OnEnable()
        {
            RefreshRegistry();
        }

        protected virtual void OnDisable()
        {
            CombatTargetRegistry.Unregister(this);
        }

        protected virtual void OnDestroy()
        {
            CombatTargetRegistry.Unregister(this);

            if (healthBehavior != null)
                healthBehavior.HealthChanged -= OnHealthChanged;
        }

        #region Editor

        protected bool EditorHaveUpgrades()
        {
            return !buildingUpgrades.IsNullOrEmpty();
        }


        protected bool EditorHaveCapacityUpgrade()
        {
            if (buildingUpgrades == null)
                return false;

            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                if (buildingUpgrades[i].UpgradeType == BuildingUpgradeType.StorageCapacity)
                    return true;
            }

            return false;
        }

        protected bool EditorHaveDurationUpgrade()
        {
            if (buildingUpgrades == null)
                return false;

            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                if (buildingUpgrades[i].UpgradeType == BuildingUpgradeType.ConversionDuration)
                    return true;
            }

            return false;
        }

        protected bool EditorHaveRecipeUpgrade()
        {
            if (buildingUpgrades == null)
                return false;

            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                if (buildingUpgrades[i].UpgradeType == BuildingUpgradeType.Recipe)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
