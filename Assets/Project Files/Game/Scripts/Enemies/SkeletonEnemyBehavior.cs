using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class SkeletonEnemyBehavior : BaseEnemyBehavior
    {
        private const int PATROL_POINT_SEARCH_ATTEMPTS = 10;
        private const float PATROL_POINT_REACH_DISTANCE = 0.2f;
        private const float PATROL_RETRY_DELAY = 1f;

        [SerializeField] int damage = 10;
        public int Damage => damage;

        [BoxFoldout("Patrol", label: "Patrol")]
        [SerializeField, Min(0.5f)] float patrolRadius = 4f;
        [BoxFoldout("Patrol", label: "Patrol")]
        [SerializeField] Vector2 patrolWaitDuration = new Vector2(0.5f, 1.5f);

        public float PatrolRetryDelay => PATROL_RETRY_DELAY;

        private NavMeshPath patrolPath;

        protected override void Awake()
        {
            base.Awake();

            patrolPath = new NavMeshPath();

            animationCallbacks.Add(EnemyAnimationEventType.SpawnEnded, OnSpawnAnimationEnded);
            animationCallbacks.Add(EnemyAnimationEventType.Hit, OnHit);
            animationCallbacks.Add(EnemyAnimationEventType.HitEnded, HitEnded);
            animationCallbacks.Add(EnemyAnimationEventType.PlaySpawnParticle, PlaySpawnParticle);
            animationCallbacks.Add(EnemyAnimationEventType.PlayDeathParticle, PlayDeathAnimation);
            animationCallbacks.Add(EnemyAnimationEventType.DropResourcesOnDeath, SpawnDrop);
        }

        public void OnHit()
        {
            if (Vector3.Distance(transform.position, PlayerBehavior.Position) < 1)
            {
                PlayerBehavior.GetBehavior().TakeDamage(new DamageSource(damage, this), transform.position, true);
            }
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
    }
}
