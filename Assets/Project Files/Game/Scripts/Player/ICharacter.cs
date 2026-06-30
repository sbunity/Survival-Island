using UnityEngine;

namespace Watermelon
{
    public interface ICharacter : IDamageable
    {
        bool IsPlayer { get; }

        Collider CharacterCollider { get; }
    }
}
