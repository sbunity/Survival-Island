using System.Collections;
using System.Collections.Generic;
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
        public bool IsAttacking { get; private set; }
        
        public SkeletonAttackState(SkeletonEnemyBehavior skeleton) : base(skeleton)
        {

        }

        public override void OnStart()
        {
            IsAttacking = false;

            if (PlayerBehavior.GetBehavior() != null)
                Target.MoveToPlayer();
        }

        public override void OnUpdate()
        {
            if (IsAttacking)
            {
                Target.transform.rotation = Quaternion.Lerp(Target.transform.rotation, Quaternion.LookRotation((PlayerBehavior.Position - Position).normalized), Time.deltaTime * 5);
                return;
            }

            if (PlayerBehavior.GetBehavior() == null) return;

            if(Vector3.Distance(Position, PlayerBehavior.Position) > 1)
            {
                if(Time.frameCount % 10 == 2)
                {
                    Target.MoveToPlayer();
                }
            } else if(!PlayerBehavior.GetBehavior().IsDead)
            {
                Target.Attack();

                IsAttacking = true;

                Target.OnHitEnded += OnHitEnded;
            }
        }

        private void OnHitEnded()
        {
            Tween.DelayedCall(0.5f, () => IsAttacking = false);
            Target.OnHitEnded -= OnHitEnded;
        }
    }
}
