using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public sealed class SkeletonTargetSelector
    {
        public const int PlayerPriority = 300;
        public const int HelperPriority = 200;
        public const int BuildingPriority = 100;

        private readonly NavMeshAgent agent;
        private readonly NavMeshPath path = new NavMeshPath();
        private readonly List<ICombatTarget> registeredTargets = new List<ICombatTarget>(32);
        private readonly float aggroRadius;

        private ICombatTarget currentTarget;
        private ICombatTarget pendingTarget;
        private ICombatTarget forcedTarget;

        private bool hasPendingTarget;
        private bool hasForcedTarget;

        public ICombatTarget CurrentTarget => IsReferenceAlive(currentTarget) ? currentTarget : null;

        public SkeletonTargetSelector(NavMeshAgent agent, float aggroRadius)
        {
            this.agent = agent;
            this.aggroRadius = aggroRadius > 0f ? aggroRadius : float.MaxValue;
        }

        public void Refresh(Vector3 attackerPosition, bool deferSwitch)
        {
            var selectedTarget = SelectTarget(attackerPosition);

            if (ReferenceEquals(selectedTarget, currentTarget))
            {
                ClearPendingTarget();
                return;
            }

            if (deferSwitch)
            {
                pendingTarget = selectedTarget;
                hasPendingTarget = true;
                return;
            }

            SetCurrentTarget(selectedTarget);
        }

        public void SetForcedTarget(ICombatTarget target, Vector3 attackerPosition, bool deferSwitch)
        {
            forcedTarget = target;
            hasForcedTarget = IsReferenceAlive(target);

            Refresh(attackerPosition, deferSwitch);
        }

        public void ClearForcedTarget(Vector3 attackerPosition, bool deferSwitch)
        {
            forcedTarget = null;
            hasForcedTarget = false;

            Refresh(attackerPosition, deferSwitch);
        }

        public void ApplyPendingTarget()
        {
            if (!hasPendingTarget)
                return;

            SetCurrentTarget(pendingTarget);
        }

        public void Reset()
        {
            currentTarget = null;
            forcedTarget = null;
            hasForcedTarget = false;
            ClearPendingTarget();
        }

        public bool IsTargetAvailable(ICombatTarget target, Vector3 attackerPosition) 
            => IsEligible(target) && IsReachable(target, attackerPosition);

        private ICombatTarget SelectTarget(Vector3 attackerPosition)
        {
            if (hasForcedTarget)
                return IsTargetAvailable(forcedTarget, attackerPosition) ? forcedTarget : null;

            CombatTargetRegistry.GetTargetsNonAlloc(registeredTargets);

            var bestPriority = 0;
            var bestDistanceSqr = float.MaxValue;
            var aggroRadiusSqr = aggroRadius >= float.MaxValue ? float.MaxValue : aggroRadius * aggroRadius;
            ICombatTarget bestTarget = null;

            for (var i = 0; i < registeredTargets.Count; i++)
            {
                var target = registeredTargets[i];
                var priority = GetPriority(target);

                if (!IsTargetAvailable(target, attackerPosition))
                    continue;

                var attackPosition = target.GetAttackPosition(attackerPosition);
                var distanceSqr = (attackPosition - attackerPosition).sqrMagnitude;

                if (distanceSqr > aggroRadiusSqr)
                    continue;

                if (CombatSystemLogic.IsBetterTarget(priority, distanceSqr, bestPriority, bestDistanceSqr))
                {
                    bestPriority = priority;
                    bestDistanceSqr = distanceSqr;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private bool IsReachable(ICombatTarget target, Vector3 attackerPosition)
        {
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                return false;

            var attackPosition = target.GetAttackPosition(attackerPosition);
            return agent.CalculatePath(attackPosition, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        private bool IsEligible(ICombatTarget target) 
            => IsReferenceAlive(target) &&
                   target.Faction == CombatFaction.Friendly &&
                   target.CanBeTargeted &&
                   GetPriority(target) > 0;

        private int GetPriority(ICombatTarget target)
        {
            if (!IsReferenceAlive(target))
                return 0;

            return CombatSystemLogic.GetTargetPriority(target.TargetType);

        }

        private bool IsReferenceAlive(ICombatTarget target)
        {
            if (target is null)
                return false;

            if (target is Object unityObject)
                return unityObject != null;

            return true;
        }

        private void SetCurrentTarget(ICombatTarget target)
        {
            currentTarget = target;
            ClearPendingTarget();
        }

        private void ClearPendingTarget()
        {
            pendingTarget = null;
            hasPendingTarget = false;
        }
    }
}
