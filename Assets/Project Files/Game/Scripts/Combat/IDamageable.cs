using UnityEngine;

namespace Watermelon
{
    public interface IDamageable
    {
        Transform Transform { get; }
        bool IsDead { get; }

        void TakeDamage(DamageSource source, Vector3 position, bool shouldFlash = false);
    }
}
