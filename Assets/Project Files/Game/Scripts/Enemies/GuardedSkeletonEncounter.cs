using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class GuardedSkeletonEncounter : MonoBehaviour
    {
        private const int CHASE_DESTINATION_UPDATE_RATE = 10;

        [SerializeField] Transform spawnPoint;
        [SerializeField, Min(0.5f)] float stateMachineHandoffDistance = 4.5f;

        public Vector3 Position => (spawnPoint != null ? spawnPoint : transform).position;
        public bool IsCompleted { get; private set; }

        public event SimpleCallback EnemyDied;

        private SkeletonEnemyBehavior enemy;
        private SkeletonStateMachine stateMachine;
        private NavMeshAgent navMeshAgent;

        private bool isRunning;
        private bool isCombatUnlocked;
        private bool isSpawnAnimationCompleted;
        private bool isLockedAttackLoopActive;
        private bool isChasingPlayer;

        public bool Begin(bool startLocked)
        {
            Stop();

            IsCompleted = false;
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

        public void UnlockCombat()
        {
            if (isCombatUnlocked)
                return;

            isCombatUnlocked = true;

            StopLockedAttackLoop();
            enemy.SetTargetDamageEnabled(true);
            SetEnemyHittable(true);

            if (isSpawnAnimationCompleted)
                BeginAggroHandoff();
        }

        public void Stop()
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
            IsCompleted = true;

            ClearEnemyReferences();

            EnemyDied?.Invoke();
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
    }
}
