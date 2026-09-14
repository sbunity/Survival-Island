using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class GuardedSkeletonEncounter : MonoBehaviour, IWorldElement, INavMeshAgent
    {
        private const int CHASE_DESTINATION_UPDATE_RATE = 10;

        [SerializeField] Transform spawnPoint;
        [SerializeField, Min(0.5f)] float stateMachineHandoffDistance = 4.5f;

        [Space]
        [SerializeField] MonoBehaviour rescueTargetBehaviour;

        public int InitialisationOrder => -10;
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public Vector3 Position => (spawnPoint != null ? spawnPoint : transform).position;

        public IGuardedRescueTarget RescueTarget => rescueTarget;

        public bool IsCleared { get; private set; }

        public event SimpleCallback Cleared;

        private IGuardedRescueTarget rescueTarget;

        private SkeletonEnemyBehavior enemy;
        private SkeletonStateMachine stateMachine;
        private NavMeshAgent navMeshAgent;

        private bool isRunning;
        private bool isCombatUnlocked;
        private bool isSpawnAnimationCompleted;
        private bool isLockedAttackLoopActive;
        private bool isChasingPlayer;

        #region World Lifecycle

        public void OnWorldLoaded()
        {
            IsCleared = false;

            rescueTarget = rescueTargetBehaviour as IGuardedRescueTarget;

            if (rescueTarget == null)
            {
                Debug.LogError("[Guarded Skeleton Encounter] Rescue target is missing or does not implement IGuardedRescueTarget.", this);
                return;
            }

            if (!rescueTarget.WaitForExternalRelease)
            {
                Debug.LogError("[Guarded Skeleton Encounter] Enable \"Wait For External Release\" on the linked rescue target.", rescueTargetBehaviour);

                rescueTarget = null;
                return;
            }

            if (!rescueTarget.IsAwaitingRescue)
            {
                SetCleared();
                return;
            }

            rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;
            rescueTarget.RescueAreaUnlocked += OnRescueAreaUnlocked;
        }

        public void OnNavMeshInitialised()
        {
            if (rescueTarget == null || IsCleared)
                return;

            if (!Begin(!rescueTarget.IsRescueAreaUnlocked))
                rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;
        }

        public void OnWorldUnloaded()
        {
            if (rescueTarget != null)
                rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;

            Stop();

            rescueTarget = null;
        }

        #endregion

        private bool Begin(bool startLocked)
        {
            Stop();

            isCombatUnlocked = !startLocked;

            var pooledEnemy = GameController.Data.EnemiesDatabase.GetEnemyBehavior(EnemyType.Skeleton);
            enemy = pooledEnemy as SkeletonEnemyBehavior;

            if (enemy == null)
            {
                Debug.LogError("[Guarded Skeleton Encounter] Skeleton enemy is missing in Enemies Database.", this);
                return false;
            }

            stateMachine = enemy.GetComponent<SkeletonStateMachine>();
            navMeshAgent = enemy.GetComponent<NavMeshAgent>();

            if (stateMachine == null || navMeshAgent == null)
            {
                Debug.LogError("[Guarded Skeleton Encounter] Skeleton prefab has an invalid setup.", enemy);
                ReleaseEnemy();
                return false;
            }

            if (stateMachine.IsPlaying)
                stateMachine.StopMachine();

            enemy.OnDeath += OnEnemyDied;
            enemy.Spawn(spawnPoint != null ? spawnPoint : transform);
            enemy.SetForcedTarget(PlayerBehavior.GetBehavior());
            enemy.SetTargetDamageEnabled(!startLocked);

            SetEnemyHittable(isCombatUnlocked);

            isSpawnAnimationCompleted = false;
            isLockedAttackLoopActive = false;
            isChasingPlayer = false;
            isRunning = true;

            if (!navMeshAgent.isOnNavMesh)
            {
                Debug.LogError("[Guarded Skeleton Encounter] Spawn point must be placed on a baked NavMesh.", this);
            }

            return true;
        }

        private void OnRescueAreaUnlocked()
        {
            if (rescueTarget != null)
                rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;

            UnlockCombat();
        }

        private void UnlockCombat()
        {
            if (isCombatUnlocked)
                return;

            isCombatUnlocked = true;

            if (enemy == null)
                return;

            StopLockedAttackLoop();
            enemy.SetTargetDamageEnabled(true);
            SetEnemyHittable(true);

            if (isSpawnAnimationCompleted)
                BeginAggroHandoff();
        }

        private void Stop()
        {
            isRunning = false;
            isChasingPlayer = false;

            StopLockedAttackLoop();

            if (enemy != null)
            {
                enemy.OnDeath -= OnEnemyDied;
                enemy.ClearForcedTarget();
                enemy.SetTargetDamageEnabled(true);
                SetEnemyHittable(true);

                if (!enemy.IsDead)
                    enemy.Unload();
            }

            ClearEnemyReferences();
        }

        private void Update()
        {
            if (!isRunning || enemy == null || enemy.IsDead)
                return;

            if (!isSpawnAnimationCompleted)
            {
                if (stateMachine.IsPlaying)
                {
                    isSpawnAnimationCompleted = true;

                    if (isCombatUnlocked)
                        BeginAggroHandoff();
                    else
                        StartLockedAttackLoop();
                }

                return;
            }

            if (!isCombatUnlocked)
            {
                if (stateMachine.IsPlaying)
                    stateMachine.StopMachine();

                enemy.StopMoving();
                return;
            }

            if (isChasingPlayer)
                UpdateAggroHandoff();
        }

        private void StartLockedAttackLoop()
        {
            if (stateMachine.IsPlaying)
                stateMachine.StopMachine();

            enemy.StopMoving();
            enemy.SetTargetDamageEnabled(false);

            if (!isLockedAttackLoopActive)
            {
                isLockedAttackLoopActive = true;
                enemy.OnHitEnded += OnLockedAttackEnded;
            }

            enemy.Attack();
        }

        private void StopLockedAttackLoop()
        {
            if (enemy != null && isLockedAttackLoopActive)
                enemy.OnHitEnded -= OnLockedAttackEnded;

            isLockedAttackLoopActive = false;
        }

        private void OnLockedAttackEnded()
        {
            if (isRunning && !isCombatUnlocked && enemy != null && !enemy.IsDead)
                enemy.Attack();
        }

        private void BeginAggroHandoff()
        {
            if (enemy == null || enemy.IsDead)
                return;

            if (stateMachine.IsPlaying)
                stateMachine.StopMachine();

            enemy.StopMoving();
            isChasingPlayer = true;

            UpdateAggroHandoff(true);
        }

        private void UpdateAggroHandoff(bool forceDestinationUpdate = false)
        {
            enemy.RefreshTargetSelection(forceDestinationUpdate);
            if (!enemy.HasAvailableTarget())
            {
                enemy.StopMoving();
                return;
            }

            if (enemy.IsCurrentTargetWithin(stateMachineHandoffDistance))
            {
                isChasingPlayer = false;
                stateMachine.StartMachine();
                return;
            }

            if (!navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
                return;

            if (forceDestinationUpdate || Time.frameCount % CHASE_DESTINATION_UPDATE_RATE == 0)
                enemy.MoveToCurrentTarget();
        }

        private void OnEnemyDied()
        {
            if (enemy != null)
            {
                enemy.OnDeath -= OnEnemyDied;
                StopLockedAttackLoop();
            }

            isRunning = false;
            isChasingPlayer = false;

            ClearEnemyReferences();

            ReleaseRescueTarget();

            SetCleared();
        }

        private void ReleaseRescueTarget()
        {
            if (rescueTarget == null || rescueTarget.IsRescued)
                return;

            if (!rescueTarget.TryRelease())
                Debug.LogError("[Guarded Skeleton Encounter] Rescue target cannot be released before its area is unlocked.", rescueTargetBehaviour);
        }

        private void SetCleared()
        {
            if (IsCleared)
                return;

            IsCleared = true;

            Cleared?.Invoke();
        }

        private void SetEnemyHittable(bool isHittable)
        {
            if (enemy != null && enemy.CharacterCollider != null)
                enemy.CharacterCollider.enabled = isHittable;
        }

        private void ReleaseEnemy()
        {
            if (enemy == null)
                return;

            enemy.OnDeath -= OnEnemyDied;
            enemy.ClearForcedTarget();
            enemy.SetTargetDamageEnabled(true);
            SetEnemyHittable(true);
            enemy.Unload();
            ClearEnemyReferences();
        }

        private void ClearEnemyReferences()
        {
            enemy = null;
            stateMachine = null;
            navMeshAgent = null;
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, 0.5f);
        }

        #region INavMeshAgent

        public void OnNavMeshWaypointChanged(Vector3 targetPoint)
        {

        }

        public void OnNavMeshAgentStartedMovement(Vector3 targetPoint)
        {

        }

        public void OnNavMeshAgentStopped()
        {

        }

        public void OnNavMeshWarpStarted()
        {

        }

        public void OnNavMeshWarpFinished()
        {

        }

        #endregion
    }
}
