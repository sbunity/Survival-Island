using System.Collections;
using System.Collections.Generic;
using Watermelon.Enemy.Skeleton;

namespace Watermelon
{
    public class SkeletonStateMachine : AbstractStateMachine<SkeletonStateMachine.State>
    {
        private SkeletonEnemyBehavior enemy;

        private void Awake()
        {
            enemy = GetComponent<SkeletonEnemyBehavior>();

            var idleStateCase = new StateCase
            {
                state = new SkeletonIdleState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new(IdleStateTransition, StateTransitionType.Independent)
                }
            };

            var patrolStateCase = new StateCase
            {
                state = new SkeletonPatrolState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new(PatrolStateTransition, StateTransitionType.Independent)
                }
            };

            var attackingStateCase = new StateCase
            {
                state = new SkeletonAttackState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new(AttackingStateTransition, StateTransitionType.Independent),
                }
            };

            states.Add(State.Idle, idleStateCase);
            states.Add(State.Patrolling, patrolStateCase);
            states.Add(State.Attacking, attackingStateCase);

            startState = State.Patrolling;
        }

        private bool PatrolStateTransition(out State nextState)
        {
            nextState = State.Attacking;

            enemy.RefreshTargetSelection();
            return enemy.IsCurrentTargetWithin(5f);
        }

        private bool IdleStateTransition(out State nextState) 
            => PatrolStateTransition(out nextState);

        private bool AttackingStateTransition(out State nextState)
        {
            nextState = State.Patrolling;

            if (enemy.IsAttackAnimationPlaying)
                return false;

            enemy.RefreshTargetSelection();
            return !enemy.HasAvailableTarget() || !enemy.IsCurrentTargetWithin(10f);
        }

        public enum State
        {
            Patrolling,
            Attacking,
            Idle,
        }
    }
}
