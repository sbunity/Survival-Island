using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class SkeletonEnemyBehavior : BaseEnemyBehavior
    {
        private const int PATROL_POINT_SEARCH_ATTEMPTS = 10;
        private const float PATROL_POINT_REACH_DISTANCE = 0.2f;
        private const float PATROL_RETRY_DELAY = 1f;
        private const float ATTACK_RANGE = 1f;
        private const int TARGET_SELECTION_UPDATE_RATE = 10;

        [SerializeField] int damage = 10;
        public int Damage => damage;

        [BoxFoldout("Combat", label: "Combat")]
        [SerializeField, Min(0f)] float aggroRadius = 10f;

        [BoxFoldout("Patrol", label: "Patrol")]
        [SerializeField, Min(0.5f)] float patrolRadius = 4f;
        [BoxFoldout("Patrol", label: "Patrol")]
        [SerializeField] Vector2 patrolWaitDuration = new(0.5f, 1.5f);

        public float PatrolRetryDelay => PATROL_RETRY_DELAY;
        public float AttackRange => ATTACK_RANGE;
        public bool IsAttackAnimationPlaying { get; private set; }
        public ICombatTarget CurrentTarget => targetSelector?.CurrentTarget;

        private NavMeshPath patrolPath;
        private SkeletonTargetSelector targetSelector;
        private int nextTargetSelectionFrame;
        private bool isTargetDamageEnabled = true;
        private bool currentAttackCanDealDamage;

        protected override void Awake()
        {
            base.Awake();

            patrolPath = new NavMeshPath();
            targetSelector = new SkeletonTargetSelector(Agent, aggroRadius);

            animationCallbacks.Add(EnemyAnimationEventType.SpawnEnded, OnSpawnAnimationEnded);
            animationCallbacks.Add(EnemyAnimationEventType.Hit, OnHit);
            animationCallbacks.Add(EnemyAnimationEventType.HitEnded, HitEnded);
            animationCallbacks.Add(EnemyAnimationEventType.PlaySpawnParticle, PlaySpawnParticle);
            animationCallbacks.Add(EnemyAnimationEventType.PlayDeathParticle, PlayDeathAnimation);
            animationCallbacks.Add(EnemyAnimationEventType.DropResourcesOnDeath, SpawnDrop);
        }

        public void OnHit()
        {
            var target = CurrentTarget;
            if (!currentAttackCanDealDamage || !targetSelector.IsTargetAvailable(target, transform.position))
                return;

            var attackPosition = target.GetAttackPosition(transform.position);
            if ((attackPosition - transform.position).sqrMagnitude > ATTACK_RANGE * ATTACK_RANGE)
                return;

            target.TakeDamage(new DamageSource(damage, this), transform.position, true);
        }

        public void RefreshTargetSelection(bool force = false)
        {
            if (!force && Time.frameCount < nextTargetSelectionFrame)
                return;

            nextTargetSelectionFrame = Time.frameCount + TARGET_SELECTION_UPDATE_RATE;
            targetSelector.Refresh(transform.position, IsAttackAnimationPlaying);
        }

        public bool HasAvailableTarget()
        {
            var target = CurrentTarget;
            return target != null && target.CanBeTargeted;
        }

        public bool IsCurrentTargetWithin(float distance)
        {
            var target = CurrentTarget;
            if (target == null || !target.CanBeTargeted)
                return false;

            var attackPosition = target.GetAttackPosition(transform.position);
            return (attackPosition - transform.position).sqrMagnitude <= distance * distance;
        }

        public bool MoveToCurrentTarget()
        {
            var target = CurrentTarget;
            if (target == null || !target.CanBeTargeted || !Agent.isActiveAndEnabled || !Agent.isOnNavMesh)
                return false;

            Agent.stoppingDistance = ATTACK_RANGE;
            return Agent.SetDestination(target.GetAttackPosition(transform.position));
        }

        public void RotateTowardsCurrentTarget(float speed)
        {
            var target = CurrentTarget;
            if (target == null || !target.CanBeTargeted)
                return;

            var direction = target.GetAttackPosition(transform.position) - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction.normalized), Time.deltaTime * speed);
        }

        public void SetForcedTarget(ICombatTarget target)
        {
            targetSelector.SetForcedTarget(target, transform.position, IsAttackAnimationPlaying);
            nextTargetSelectionFrame = Time.frameCount + TARGET_SELECTION_UPDATE_RATE;
        }

        public void ClearForcedTarget()
        {
            targetSelector.ClearForcedTarget(transform.position, IsAttackAnimationPlaying);
            nextTargetSelectionFrame = Time.frameCount;
        }

        public void SetTargetDamageEnabled(bool enabled)
        {
            isTargetDamageEnabled = enabled;
        }

        public override void Attack()
        {
            IsAttackAnimationPlaying = true;
            currentAttackCanDealDamage = isTargetDamageEnabled;
            base.Attack();
        }

        public bool TryMoveToRandomPatrolPoint()
        {
            if (SpawnPoint == null || !Agent.isActiveAndEnabled || !Agent.isOnNavMesh)
                return false;

            var sampleDistance = Mathf.Max(1f, patrolRadius * 0.25f);

            for (var i = 0; i < PATROL_POINT_SEARCH_ATTEMPTS; i++)
            {
                var randomOffset = Random.insideUnitCircle * patrolRadius;
                var point = SpawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

                if (!NavMesh.SamplePosition(point, out NavMeshHit hit, sampleDistance, Agent.areaMask))
                    continue;

                if (!Agent.CalculatePath(hit.position, patrolPath) || patrolPath.status != NavMeshPathStatus.PathComplete)
                    continue;

                Agent.stoppingDistance = 0f;
                return Agent.SetDestination(hit.position);
            }

            return false;
        }

        public bool IsPatrolPointReached()
        {
            if (Agent.pathPending)
                return false;

            return Agent.remainingDistance <= Agent.stoppingDistance + PATROL_POINT_REACH_DISTANCE;
        }

        public float GetPatrolWaitDuration()
        {
            var minDuration = Mathf.Max(0f, Mathf.Min(patrolWaitDuration.x, patrolWaitDuration.y));
            var maxDuration = Mathf.Max(minDuration, Mathf.Max(patrolWaitDuration.x, patrolWaitDuration.y));

            return Random.Range(minDuration, maxDuration);
        }

        public void StopMoving()
        {
            if (Agent.isActiveAndEnabled && Agent.isOnNavMesh)
                Agent.ResetPath();
        }

        protected override void HitEnded()
        {
            IsAttackAnimationPlaying = false;
            currentAttackCanDealDamage = false;
            targetSelector.ApplyPendingTarget();
            nextTargetSelectionFrame = Time.frameCount;

            base.HitEnded();
        }

        protected override void OnSpawned()
        {
            ResetTargeting();
        }

        protected override void OnDeathStarted()
        {
            ResetTargeting();
        }

        protected override void OnUnloaded()
        {
            ResetTargeting();
        }

        protected override void OnReturnedToPool()
        {
            ResetTargeting();
        }

        private void ResetTargeting()
        {
            targetSelector?.Reset();
            IsAttackAnimationPlaying = false;
            isTargetDamageEnabled = true;
            currentAttackCanDealDamage = false;
            nextTargetSelectionFrame = Time.frameCount;
        }
    }
}
