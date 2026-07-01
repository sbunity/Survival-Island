using UnityEngine;

namespace Watermelon
{
    public enum BuildingRebuildStage
    {
        Purchasing,
        Constructing,
        Complete,
    }

    public static class CombatSystemLogic
    {
        public static int GetTargetPriority(CombatTargetType targetType)
        {
            return targetType switch
            {
                CombatTargetType.Player => SkeletonTargetSelector.PlayerPriority,
                CombatTargetType.Helper => SkeletonTargetSelector.HelperPriority,
                CombatTargetType.Building => SkeletonTargetSelector.BuildingPriority,
                _ => 0,
            };
        }

        public static bool IsBetterTarget(int candidatePriority, float candidateDistanceSqr, int bestPriority, float bestDistanceSqr)
        {
            return candidatePriority > bestPriority ||
                   candidatePriority == bestPriority && candidateDistanceSqr < bestDistanceSqr;
        }

        public static bool ShouldShowHealthbar(bool visibilityAllowed, float currentHealth, float maxHealth)
        {
            return visibilityAllowed && currentHealth > 0f && currentHealth < maxHealth;
        }

        public static bool CanRegenerate(bool isInitialised, bool regenerationEnabled, float regenerationPerSecond,
            float currentHealth, float maxHealth, float currentTime, float regenerationAllowedTime)
        {
            return isInitialised && regenerationEnabled && regenerationPerSecond > 0f &&
                   currentHealth > 0f && currentHealth < maxHealth && currentTime >= regenerationAllowedTime;
        }

        public static BuildingRebuildStage GetRebuildStage(bool purchasePending, bool constructionPending)
        {
            if (purchasePending)
                return BuildingRebuildStage.Purchasing;

            return constructionPending ? BuildingRebuildStage.Constructing : BuildingRebuildStage.Complete;
        }

        public static Vector3 ClampInsideRadius(Vector3 center, Vector3 position, float radius, float inset)
        {
            var offset = position - center;
            var height = offset.y;
            offset.y = 0f;

            var allowedRadius = Mathf.Max(0f, radius - Mathf.Max(0f, inset));
            if (offset.sqrMagnitude > allowedRadius * allowedRadius && offset.sqrMagnitude > Mathf.Epsilon)
                offset = offset.normalized * allowedRadius;

            offset.y = height;
            return center + offset;
        }
    }
}
