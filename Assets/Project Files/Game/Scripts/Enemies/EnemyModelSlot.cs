using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public sealed class EnemyModelSlot
    {
        private readonly Transform holder;
        private readonly BaseEnemyBehavior owner;
        private readonly RuntimeAnimatorController animatorController;
        private readonly Dictionary<PlayerGraphics, PlayerGraphics> instances = new();

        public PlayerGraphics Current { get; private set; }

        public EnemyModelSlot(Transform holder, BaseEnemyBehavior owner, RuntimeAnimatorController animatorController)
        {
            this.holder = holder;
            this.owner = owner;
            this.animatorController = animatorController;
        }

        public PlayerGraphics Wear(PlayerGraphics skin)
        {
            if (skin == null)
                return Current;

            if (!instances.TryGetValue(skin, out PlayerGraphics instance) || instance == null)
            {
                instance = Adopt(Object.Instantiate(skin, holder));
                instances[skin] = instance;
            }

            if (Current != null && Current != instance)
                Current.gameObject.SetActive(false);

            Current = instance;
            Current.gameObject.SetActive(true);

            return Current;
        }

        private PlayerGraphics Adopt(PlayerGraphics instance)
        {
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var animator = instance.Animator;
            animator.runtimeAnimatorController = animatorController;
            animator.applyRootMotion = false;

            if (!animator.TryGetComponent(out EnemyAnimationHandler handler))
                handler = animator.gameObject.AddComponent<EnemyAnimationHandler>();

            handler.Initialise(owner);

            return instance;
        }
    }
}
