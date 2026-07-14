using UnityEngine;

namespace Watermelon
{
    public interface ICombatTarget : IDamageable
    {
        CombatFaction Faction { get; }
        CombatTargetType TargetType { get; }
        bool CanBeTargeted { get; }

        Vector3 GetAttackPosition(Vector3 attackerPosition);
    }

    public enum CombatFaction
    {
        Friendly = 0,
        Hostile = 1,
    }

    public enum CombatTargetType
    {
        Player = 0,
        Helper = 1,
        Building = 2,
        Enemy = 3,
    }
}
