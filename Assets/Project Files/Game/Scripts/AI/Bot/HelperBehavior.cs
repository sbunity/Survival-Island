using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Watermelon.AI;

namespace Watermelon
{
    public class HelperBehavior : MonoBehaviour, INavMeshAgent, ICharacterGraphics<HelperGraphics>, IHitter, IResourceGiver, IWorldElement, ICharacter, ICombatTarget
    {
        public static readonly int MOVEMENT_MULTIPLIER_HASH = Animator.StringToHash("Movement Multiplier");

        public static readonly int WAITING_HASH = Animator.StringToHash("Opening");
        public static readonly int SITTING_HASH = Animator.StringToHash("Sitting");

        public int InitialisationOrder => 10;

        [UniqueID, Order(-1)]
        [SerializeField] string id;
        public string ID => id;

        [Order(-1)]
        [SerializeField] HelperGraphics defaultGraphics;

        [BoxGroup("Opening")]
        [SerializeField] Transform customRestPoint;

        [Space]
        [BoxGroup("Opening")]
        [SerializeField] bool specialOpeningLogic = false;

        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] bool disableObjectIfZoneIsLocked = false;

        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] GroundTileComplexBehavior[] linkedTiles;
        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] BuildingComplexBehavior[] linkedBuildings;

        [Space]
        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] AnimationClip openingAnimation;

        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] bool waitForExternalRelease;
        public bool WaitForExternalRelease => waitForExternalRelease;

        [BoxGroup("Settings")]
        [SerializeField] HelperTaskType availableTasks;
        public HelperTaskType AvailableTaskTypes => availableTasks;

        [BoxGroup("Settings")]
        [SerializeField] float tasksDistance = 0;
        public float TasksDistance => tasksDistance;

        [BoxGroup("Health")]
        [SerializeField, Min(1f)] float maxHealth = 100f;
        public float MaxHealth => isHealthInitialised ? healthBehavior.MaxHealth : maxHealth;

        [BoxGroup("Health")]
        [SerializeField] HealthBehavior healthBehavior;
        public HealthBehavior Health => healthBehavior;

        [BoxGroup("Health")]
        [SerializeField, Min(0f)] float regenerationDelay = 5f;

        [BoxGroup("Health")]
        [SerializeField, Min(0f)] float regenerationPerSecond = 10f;

        [BoxGroup("Health")]
        [SerializeField, Min(0f)] float recoveryDuration = 5f;

        [BoxGroup("Combat")]
        [SerializeField, Min(0f)] float combatDamage = 10f;
        public float CombatDamage => combatDamage;

        [BoxGroup("Combat")]
        [SerializeField, Min(0f)] float combatRange = 1.25f;
        public float CombatRange => combatRange;

        [BoxGroup("Combat")]
        [SerializeField, Min(0f)] float combatCooldown = 1f;
        public float CombatCooldown => combatCooldown;

        [Space]
        [BoxGroup("Settings")]
        [SerializeField] SimpleEmoteBehavior emoteBehavior;
        public SimpleEmoteBehavior EmoteBehavior => emoteBehavior;

        [Space]
        [BoxGroup("Settings")]
        [SerializeField] HelperInventory inventory;
        public HelperInventory Inventory => inventory;

        // Components
        protected NavMeshAgentBehaviour navMeshAgentBehaviour;
        public NavMeshAgentBehaviour NavMeshAgentBehaviour => navMeshAgentBehaviour;

        protected NavMeshAgent navMeshAgent;
        protected Rigidbody characterRigidbody;
        protected Animator characterAnimator;

        protected Collider characterCollider;
        public Collider CharacterCollider => characterCollider;

        public Transform Transform => transform;
        public Transform SnappingTransform => transform;
        public bool IsPlayer => false;
        public bool IsDead => healthBehavior != null && healthBehavior.IsDepleted;
        public bool IsRecovering { get; private set; }
        public CombatFaction Faction => CombatFaction.Friendly;
        public CombatTargetType TargetType => CombatTargetType.Helper;
        public bool CanBeTargeted => isInitialised && isOpeningCompleted && !IsDead && !IsRecovering &&
            isActiveAndEnabled && gameObject.activeInHierarchy && (characterCollider == null || characterCollider.enabled);

        // Graphics
        private CharacterGraphicsHolder<HelperGraphics> graphicsHolder;
        public HelperGraphics Graphics => graphicsHolder.CharacterGraphics;

        // State machine
        private HelperStateMachine stateMachine;

        // Gathering
        private AbstractHitableBehavior targetHitableBehavior;
        public AbstractHitableBehavior TtargetHitableBehavior => targetHitableBehavior;

        private ICombatTarget combatTarget;
        public ICombatTarget CombatTarget => combatTarget;

        private ActionContext actionContext;
        private float nextCombatAttackTime;
        private float defaultStoppingDistance;

        private CurrencyType resourceType;
        public CurrencyType ResourceType => resourceType;

        private HelperSave helperSave;

        private bool isStoringResourcesActive;
        public bool IsStoringResourcesActive => isStoringResourcesActive;

        private BaseTask activeTask;
        public BaseTask ActiveTask => activeTask;

        private bool isRunning;
        public bool IsRunning => isRunning;

        public bool IsOpened => helperSave != null && helperSave.IsOpened;

        public Vector3 FlyingResourceSpawnPosition => transform.position + new Vector3(0, 1, 0);

        public float LastTimeResourceGiven { get; protected set; }
        public bool IsResourceGivingBlocked => isRunning;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public bool AutoPickResources => true;

        private Vector3 zoneRestPosition;

        private bool isInitialised;

        private bool isOpeningAreaUnlocked;
        public bool IsOpeningAreaUnlocked => isOpeningAreaUnlocked;

        private bool isOpeningCompleted;
        private bool isHealthInitialised;

        public event SimpleCallback HelperUnlocked;
        public event SimpleCallback OpeningAreaUnlocked;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            characterRigidbody = GetComponent<Rigidbody>();
            characterCollider = GetComponent<Collider>();

            if (healthBehavior == null)
                healthBehavior = GetComponent<HealthBehavior>();

            if (healthBehavior == null)
                healthBehavior = gameObject.AddComponent<HealthBehavior>();

            defaultStoppingDistance = navMeshAgent.stoppingDistance;

            navMeshAgentBehaviour = new NavMeshAgentBehaviour();
            navMeshAgentBehaviour.Initialise(this, navMeshAgent);

            stateMachine = GetComponent<HelperStateMachine>();
            stateMachine.enabled = false;
            stateMachine.Initialise(this, navMeshAgentBehaviour);

            emoteBehavior.Initialise();
        }

        public void OnWorldLoaded()
        {
            isInitialised = true;
            isOpeningAreaUnlocked = false;
            isOpeningCompleted = false;

            graphicsHolder = new CharacterGraphicsHolder<HelperGraphics>();
            graphicsHolder.Initialise(this);
            graphicsHolder.LinkGraphics(defaultGraphics);

            inventory.Initialise(this);

            helperSave = SaveController.GetSaveObject<HelperSave>(LinkedWorldBehavior.WorldData.ID,"helper_" + id);

            isStoringResourcesActive = availableTasks.IsTypeAvailable(HelperTaskType.Storing);

            zoneRestPosition = LinkedWorldBehavior.GetHelperRestPosition();

            InitialiseHealth();
        }

        public void OnNavMeshInitialised()
        {
            if (helperSave.IsOpened)
            {
                isOpeningAreaUnlocked = true;

                navMeshAgentBehaviour.Warp(GetRestPosition());

                CompleteOpening(false);
            }
            else
            {
                if (specialOpeningLogic && (!linkedTiles.IsNullOrEmpty() || !linkedBuildings.IsNullOrEmpty()))
                {
                    if(disableObjectIfZoneIsLocked)
                        gameObject.SetActive(false);

                    ActivateWaitingAnimation();

                    if (!linkedTiles.IsNullOrEmpty())
                    {
                        foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                        {
                            if(linkedTile != null)
                            {
                                linkedTile.SubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                                linkedTile.InvokeOrSubscribe(CheckIfLinkedElementsOpened);
                            }
                        }
                    }

                    if (!linkedBuildings.IsNullOrEmpty())
                    {
                        foreach (BuildingComplexBehavior linkedBuilding in linkedBuildings)
                        {
                            if (linkedBuilding != null)
                            {
                                linkedBuilding.SubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                                linkedBuilding.InvokeOrSubscribe(CheckIfLinkedElementsOpened);
                            }
                        }
                    }

                    CheckIfLinkedElementsOpened();

                    if (isOpeningAreaUnlocked)
                        UnsubscribeFromOpeningSources();
                }
                else
                {
                    OnOpeningAreaUnlocked();
                }
            }
        }

        public void OnWorldUnloaded()
        {
            SaveHealth();
            CombatTargetRegistry.Unregister(this);
            ClearCombatTarget();

            navMeshAgentBehaviour.Unload();

            emoteBehavior.Unload();

            stateMachine.StopMachine();

            if (healthBehavior != null)
            {
                healthBehavior.ConfigureRegeneration(false, regenerationDelay, regenerationPerSecond);
                healthBehavior.HealthChanged -= OnHealthChanged;
                healthBehavior.Depleted -= OnHealthDepleted;
            }

            isHealthInitialised = false;
            isInitialised = false;
        }

        private void OnDestroy()
        {
            CombatTargetRegistry.Unregister(this);
        }

        private void CheckIfLinkedElementsOpened()
        {
            if (!isOpeningAreaUnlocked && IsAnyLinkedElementsOpened())
                OnOpeningAreaUnlocked();
        }

        private bool IsAnyLinkedElementsOpened()
        {
            if (!linkedTiles.IsNullOrEmpty())
            {
                foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                {
                    if(linkedTile != null && linkedTile.IsOpen)
                    {
                        return true;
                    }
                }
            }

            if (!linkedBuildings.IsNullOrEmpty())
            {
                foreach (BuildingComplexBehavior linkedBuildign in linkedBuildings)
                {
                    if (linkedBuildign != null && linkedBuildign.IsOpen)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnOpeningAreaUnlocked()
        {
            if (isOpeningAreaUnlocked)
                return;

            isOpeningAreaUnlocked = true;

            UnsubscribeFromOpeningSources();

            OpeningAreaUnlocked?.Invoke();

            if (!waitForExternalRelease)
                TryRelease();
        }

        public bool TryRelease()
        {
            if (isOpeningCompleted)
                return true;

            if (!isInitialised || helperSave == null || !isOpeningAreaUnlocked)
                return false;

            CompleteOpening(true);

            return true;
        }

        private void CompleteOpening(bool notify)
        {
            if (isOpeningCompleted)
                return;

            isOpeningCompleted = true;

            if (disableObjectIfZoneIsLocked)
            {
                gameObject.SetActive(true);

                graphicsHolder.PlaySpawnAnimation();
            }

            helperSave.IsOpened = true;

            stateMachine.enabled = true;

            if (IsRecovering)
            {
                CombatTargetRegistry.Unregister(this);
                navMeshAgentBehaviour.Warp(GetRestPosition());
                stateMachine.StartMachine(HelperStateMachine.State.RecoveringAtBase);
            }
            else
            {
                CombatTargetRegistry.Register(this);
                stateMachine.StartMachine(HelperStateMachine.State.WaitingForTask);
            }

            DisableWaitingAnimation();

            if (notify)
                HelperUnlocked?.Invoke();
        }

        private void UnsubscribeFromOpeningSources()
        {
            if (!linkedTiles.IsNullOrEmpty())
            {
                foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                {
                    if(linkedTile != null)
                    {
                        linkedTile.UnsubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                    }
                }
            }

            if (!linkedBuildings.IsNullOrEmpty())
            {
                foreach (BuildingComplexBehavior linkedBuilding in linkedBuildings)
                {
                    if(linkedBuilding)
                    {
                        linkedBuilding.UnsubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                    }
                }
            }
        }

        private void InitialiseHealth()
        {
            if (isHealthInitialised)
                return;

            var savedHealth = helperSave.HasHealthData ? helperSave.CurrentHealth : maxHealth;
            var clampedMaxHealth = Mathf.Max(1f, maxHealth);

            IsRecovering = helperSave.HasHealthData &&
                (helperSave.IsRecovering || savedHealth <= 0f) &&
                savedHealth < clampedMaxHealth;

            healthBehavior.HealthChanged -= OnHealthChanged;
            healthBehavior.HealthChanged += OnHealthChanged;
            healthBehavior.Depleted -= OnHealthDepleted;
            healthBehavior.Depleted += OnHealthDepleted;

            healthBehavior.ShowOnChange = true;
            healthBehavior.HideOnFull = true;

            isHealthInitialised = true;

            healthBehavior.Initialise(clampedMaxHealth, savedHealth);
            healthBehavior.ConfigureRegeneration(!IsRecovering, regenerationDelay, regenerationPerSecond);

            if (!healthBehavior.IsDepleted && !healthBehavior.IsFull)
                healthBehavior.Show();
            else if (healthBehavior.IsDepleted)
                healthBehavior.ForceHide();

            SaveHealth();
        }

        private void OnHealthChanged()
        {
            SaveHealth();
        }

        private void SaveHealth()
        {
            if (!isHealthInitialised || helperSave == null || healthBehavior == null)
                return;

            helperSave.HasHealthData = true;
            helperSave.CurrentHealth = healthBehavior.CurrentHealth;
            helperSave.IsRecovering = IsRecovering;
        }

        private void OnHealthDepleted()
        {
            if (IsRecovering)
                return;

            IsRecovering = true;
            healthBehavior.ConfigureRegeneration(false, regenerationDelay, regenerationPerSecond);
            healthBehavior.ForceHide();

            CombatTargetRegistry.Unregister(this);
            ClearCombatTarget();

            stateMachine.StopMachine();
            UnlinkActiveTask();

            targetHitableBehavior = null;
            actionContext = ActionContext.None;

            Graphics.InteractionAnimations.Disable();
            DisableSittingAnimation();
            emoteBehavior.Hide();

            navMeshAgentBehaviour.Stop();
            navMeshAgent.stoppingDistance = defaultStoppingDistance;
            navMeshAgentBehaviour.Warp(GetRestPosition());

            SaveHealth();
            stateMachine.StartMachine(HelperStateMachine.State.RecoveringAtBase);
        }

        public void ShowRecoveryHealthbar()
        {
            if (IsRecovering && healthBehavior.CurrentHealth > 0f)
                healthBehavior.Show();
        }

        public bool UpdateRecovery(float deltaTime)
        {
            if (!IsRecovering)
                return true;

            if (!healthBehavior.IsFull)
            {
                var wasDepleted = healthBehavior.IsDepleted;

                if (recoveryDuration <= 0f)
                    healthBehavior.Restore();
                else if (deltaTime > 0f)
                    healthBehavior.Add(healthBehavior.MaxHealth / recoveryDuration * deltaTime);

                if (wasDepleted && !healthBehavior.IsDepleted)
                    healthBehavior.Show();
            }

            if (!healthBehavior.IsFull)
                return false;

            IsRecovering = false;
            healthBehavior.Hide();
            healthBehavior.ConfigureRegeneration(true, regenerationDelay, regenerationPerSecond);

            SaveHealth();

            if (isOpeningCompleted)
                CombatTargetRegistry.Register(this);

            return true;
        }

        private void Update()
        {
            if (!isInitialised) return;

            navMeshAgentBehaviour.Update();
            emoteBehavior.Update();

            if (isRunning)
            {
                characterAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, navMeshAgent.velocity.magnitude / navMeshAgent.speed);
            }
        }

        public void SnapToHittable(IHitable hitableTarget)
        {
            if (hitableTarget != null)
            {
                Vector3 lookAt = (hitableTarget.SnappingTransform.position - transform.position).SetY(0).normalized;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAt), Time.deltaTime * hitableTarget.SnappingSpeedMultiplier);
                if (hitableTarget.HasSnappingDistance)
                {
                    transform.position = Vector3.Lerp(transform.position, hitableTarget.SnappingTransform.position.SetY(transform.position.y) - lookAt * hitableTarget.SnappingDistance, Time.deltaTime * hitableTarget.SnappingSpeedMultiplier);
                }
            }
        }

        #region Graphics
        public void OnGraphicsUpdated(HelperGraphics characterGraphics)
        {
            characterGraphics.Inititalise(this);

            characterAnimator = characterGraphics.Animator;
        }

        public void OnGraphicsUnloaded(HelperGraphics currentGraphics)
        {

        }
        #endregion

        #region NavMesh
        public void OnNavMeshWaypointChanged(Vector3 targetPoint)
        {

        }

        public void OnNavMeshAgentStartedMovement(Vector3 targetPoint)
        {
            isRunning = true;

            characterAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }

        public void OnNavMeshAgentStopped()
        {
            isRunning = false;

            characterAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, 0);
        }

        public void OnNavMeshWarpStarted()
        {

        }

        public void OnNavMeshWarpFinished()
        {

        }
        #endregion

        #region Combat
        public void TakeDamage(DamageSource source, Vector3 position, bool shouldFlash = false)
        {
            if (!CanBeTargeted || source == null || source.Damage <= 0f)
                return;

            healthBehavior.Subtract(source.Damage);
        }

        public Vector3 GetAttackPosition(Vector3 attackerPosition)
        {
            if (characterCollider != null && characterCollider.enabled)
                return characterCollider.ClosestPoint(attackerPosition);

            return transform.position;
        }

        public bool SetCombatTarget(ICombatTarget target)
        {
            if (!IsCombatTargetValid(target))
            {
                ClearCombatTarget();
                return false;
            }

            combatTarget = target;
            return true;
        }

        public void ClearCombatTarget()
        {
            combatTarget = null;
            navMeshAgent.stoppingDistance = defaultStoppingDistance;

            if (actionContext != ActionContext.Combat)
                return;

            actionContext = ActionContext.None;

            if (Graphics != null)
                Graphics.InteractionAnimations.Disable();
        }

        public bool IsCombatTargetValid(ICombatTarget target)
        {
            if (target == null || ReferenceEquals(target, this) || target.Faction != CombatFaction.Hostile || !target.CanBeTargeted)
                return false;

            if (target is Object unityObject && unityObject == null)
                return false;

            return target.Transform != null && !target.IsDead;
        }

        public bool MoveToCombatTarget()
        {
            if (IsDead || IsRecovering || !IsCombatTargetValid(combatTarget))
            {
                ClearCombatTarget();
                return false;
            }

            var attackPosition = combatTarget.GetAttackPosition(transform.position);
            if ((attackPosition - transform.position).sqrMagnitude <= combatRange * combatRange)
            {
                navMeshAgentBehaviour.Stop();
                return true;
            }

            if (!navMeshAgentBehaviour.PathExists(attackPosition))
                return false;

            navMeshAgent.stoppingDistance = combatRange;
            navMeshAgentBehaviour.SetWaypoints(attackPosition);
            return true;
        }

        public bool MoveToCombatPosition(Vector3 position)
        {
            if (IsDead || IsRecovering || !IsCombatTargetValid(combatTarget))
            {
                ClearCombatTarget();
                return false;
            }

            if (!navMeshAgentBehaviour.PathExists(position))
                return false;

            navMeshAgent.stoppingDistance = 0f;
            navMeshAgentBehaviour.SetWaypoints(position);
            return true;
        }

        public bool TryAttack()
        {
            if (IsDead || IsRecovering || Time.time < nextCombatAttackTime || !IsCombatTargetValid(combatTarget))
            {
                if (!IsCombatTargetValid(combatTarget))
                    ClearCombatTarget();

                return false;
            }

            var attackPosition = combatTarget.GetAttackPosition(transform.position);
            var direction = attackPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > combatRange * combatRange)
                return false;

            navMeshAgentBehaviour.Stop();

            if (direction.sqrMagnitude > Mathf.Epsilon)
                transform.rotation = Quaternion.LookRotation(direction.normalized);

            actionContext = ActionContext.Combat;
            nextCombatAttackTime = Time.time + combatCooldown;

            Graphics.InteractionAnimations.Activate(InteractionAnimationType.Default);

            var interactionsLayer = characterAnimator.GetLayerIndex("Interactions");
            if (interactionsLayer >= 0)
                characterAnimator.Play("Interaction", interactionsLayer, 0f);

            return true;
        }

        public void OnAnimationHit()
        {
            if (actionContext == ActionContext.Combat)
            {
                OnCombatHit();
                return;
            }

            if (actionContext == ActionContext.Resource)
                OnResourceHit();
        }

        private void OnCombatHit()
        {
            if (!IsCombatTargetValid(combatTarget))
            {
                ClearCombatTarget();
                return;
            }

            var attackPosition = combatTarget.GetAttackPosition(transform.position);
            if ((attackPosition - transform.position).sqrMagnitude > combatRange * combatRange)
                return;

            var target = combatTarget;
            target.TakeDamage(new DamageSource(combatDamage, this), transform.position, true);

            if (!IsCombatTargetValid(target))
                ClearCombatTarget();
        }
        #endregion

        #region Animations
        private void ActivateWaitingAnimation()
        {
            defaultGraphics.InteractionAnimations.OverrideAnimation("Opening", openingAnimation);

            characterAnimator.SetBool(WAITING_HASH, true);
        }

        private void DisableWaitingAnimation()
        {
            characterAnimator.SetBool(WAITING_HASH, false);
        }

        public void ActivateSittingAnimation()
        {
            characterAnimator.SetBool(SITTING_HASH, true);
        }

        public void DisableSittingAnimation()
        {
            characterAnimator.SetBool(SITTING_HASH, false);
        }
        #endregion

        #region Task
        public bool TryStartBaseDefense(DefendBaseTask task)
        {
            if (task == null || !task.Validate(this) || IsDead || IsRecovering)
                return false;

            if (activeTask == task && stateMachine.CurrentState == HelperStateMachine.State.DefendingBase)
                return true;

            if (stateMachine.IsPlaying)
                stateMachine.StopMachine();

            UnlinkActiveTask();
            SetActiveTask(task);
            stateMachine.StartMachine(HelperStateMachine.State.DefendingBase);

            return true;
        }

        public void SetActiveTask(BaseTask task)
        {
            UnlinkActiveTask();

            activeTask = task;
            activeTask.Take(this);
        }

        public void UnlinkActiveTask()
        {
            if (activeTask == null) return;

            activeTask.Reset();
            activeTask = null;
        }

        public BaseTask FindAvailableTask()
        {
            return LinkedWorldBehavior.TaskHandler.GetAvailableTask(this);
        }
        #endregion

        #region Gathering
        public void SetTargetHitableObject(AbstractHitableBehavior hitableBehavior)
        {
            targetHitableBehavior = hitableBehavior;

            if (hitableBehavior != null)
                actionContext = ActionContext.Resource;
            else if (actionContext == ActionContext.Resource)
                actionContext = ActionContext.None;
        }

        public void OnResourceHit()
        {
            if (targetHitableBehavior != null)
            {
                if (isStoringResourcesActive)
                {
                    targetHitableBehavior.GetHit(transform.position, true, this);
                }
                else
                {
                    targetHitableBehavior.GetHit(transform.position, true);
                }

                if (!targetHitableBehavior.IsActive)
                {
                    targetHitableBehavior = null;
                    actionContext = ActionContext.None;
                }
            }
        }

        public void OnResourcePickPerformed(ResourceDropBehavior dropBehavior)
        {
            if(!inventory.IsFull)
            {
                inventory.TryToAdd(dropBehavior.CurrencyType, dropBehavior.DropAmount);
            }

            dropBehavior.OnObjectPicked(this, true);
        }

        public bool HasResource(Resource resource)
        {
            return inventory.HasResource(resource.currency, resource.amount);
        }

        public bool HasResources()
        {
            return inventory.CurrentCapacity > 0;
        }

        public int GetResourceCount(CurrencyType currencyType)
        {
            return inventory.GetResourceCount(currencyType);
        }

        public void GiveResource(Resource resource)
        {
            HelperInventory.Slot resourceSlot = inventory.GetResource(resource.currency);
            if(resourceSlot != null)
            {
                resourceSlot.Substract(resource.amount);
            }

            LastTimeResourceGiven = Time.time;
        }
        #endregion

        public Vector3 GetRestPosition()
        {
            if (customRestPoint != null)
                return customRestPoint.position;

            return zoneRestPosition;
        }

        public void SetCustomResetPoint(Transform customRestPoint)
        {
            this.customRestPoint = customRestPoint;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            if (!linkedTiles.IsNullOrEmpty())
            {
                foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                {
                    if(linkedTile != null)
                    {
                        Gizmos.DrawLine(transform.position, linkedTile.transform.position);
                    }
                }
            }

            if (!linkedBuildings.IsNullOrEmpty())
            {
                foreach (BuildingComplexBehavior linkedBuilding in linkedBuildings)
                {
                    if(linkedBuilding != null)
                    {
                        Gizmos.DrawLine(transform.position, linkedBuilding.transform.position);
                    }
                }
            }
        }

        private enum ActionContext
        {
            None = 0,
            Resource = 1,
            Combat = 2,
        }
    }
}
