using UnityEngine;

namespace Watermelon.Enemy.Skeleton
{
    public class SkeletonIdleState : StateBehavior<SkeletonEnemyBehavior>
    {
        public SkeletonIdleState(SkeletonEnemyBehavior skeleton) : base(skeleton)
        {

        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Position, Target.SpawnPoint.position) > 0.2f)
            {
                if (Time.frameCount % 10 == 1)
                {
                    Target.MoveToSpawn();
                }
            }
            else
            {
                Target.transform.rotation = Quaternion.Lerp(Target.transform.rotation, Target.SpawnPoint.rotation, Time.deltaTime * 5);
            }
        }
    }

    public class SkeletonPatrolState : StateBehavior<SkeletonEnemyBehavior>
    {
        private bool isMoving;
        private int destinationSetFrame;
        private float nextDestinationTime;

        public SkeletonPatrolState(SkeletonEnemyBehavior skeleton) : base(skeleton)
        {

        }

        public override void OnStart()
        {
            Target.StopMoving();
            TryStartMoving();
        }

        public override void OnUpdate()
        {
            if (isMoving)
            {
                if (Time.frameCount == destinationSetFrame || !Target.IsPatrolPointReached())
                    return;

                isMoving = false;
                nextDestinationTime = Time.time + Target.GetPatrolWaitDuration();
            }

            if (Time.time >= nextDestinationTime)
                TryStartMoving();
        }

        public override void OnEnd()
        {
            Target.StopMoving();
        }

        private void TryStartMoving()
        {
            isMoving = Target.TryMoveToRandomPatrolPoint();
            destinationSetFrame = Time.frameCount;

            if (!isMoving)
                nextDestinationTime = Time.time + Target.PatrolRetryDelay;
        }
    }

    public class SkeletonAttackState : StateBehavior<SkeletonEnemyBehavior>
    {
        private const float ATTACK_COOLDOWN = 0.5f;

        private float nextAttackTime;
        private bool isWaitingForHitEnded;
        
        public SkeletonAttackState(SkeletonEnemyBehavior skeleton) : base(skeleton)
        {

        }

        public override void OnStart()
        {
            nextAttackTime = 0f;
            isWaitingForHitEnded = false;

            Target.RefreshTargetSelection(true);
            Target.MoveToCurrentTarget();
        }

        public override void OnUpdate()
        {
            Target.RefreshTargetSelection();

            if (Target.IsAttackAnimationPlaying)
            {
                Target.RotateTowardsCurrentTarget(5f);
                return;
            }

            if (!Target.HasAvailableTarget())
                return;

            if (!Target.IsCurrentTargetWithin(Target.AttackRange))
            {
                if (Time.frameCount % 10 == 2)
                    Target.MoveToCurrentTarget();

                return;
            }

            Target.RotateTowardsCurrentTarget(5f);

            if (Time.time >= nextAttackTime)
            {
                Target.Attack();

                if (!isWaitingForHitEnded)
                {
                    isWaitingForHitEnded = true;
                    Target.OnHitEnded += OnHitEnded;
                }
            }
        }

        public override void OnEnd()
        {
            Target.OnHitEnded -= OnHitEnded;
            isWaitingForHitEnded = false;
            Target.StopMoving();
        }

        private void OnHitEnded()
        {
            Target.OnHitEnded -= OnHitEnded;
            isWaitingForHitEnded = false;
            nextAttackTime = Time.time + ATTACK_COOLDOWN;
        }
    }
}
