using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class CombatTargetRegistry
    {
        private static readonly List<ICombatTarget> targets = new(32);

        public static int Count => targets.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            targets.Clear();
        }

        public static bool Register(ICombatTarget target)
        {
            if (!IsReferenceAlive(target) || targets.Contains(target))
                return false;

            targets.Add(target);
            return true;
        }

        public static bool Unregister(ICombatTarget target)
        {
            if (target is null)
                return false;

            return targets.Remove(target);
        }

        public static ICombatTarget GetTarget(int index)
        {
            if (index < 0 || index >= targets.Count)
                return null;

            var target = targets[index];
            return IsReferenceAlive(target) ? target : null;
        }

        public static void GetTargetsNonAlloc(List<ICombatTarget> results)
        {
            if (results == null)
                throw new System.ArgumentNullException(nameof(results));

            results.Clear();
            RemoveInvalidTargets();

            for (var i = 0; i < targets.Count; i++)
                results.Add(targets[i]);
        }

        public static void RemoveInvalidTargets()
        {
            for (var i = targets.Count - 1; i >= 0; i--)
            {
                if (!IsReferenceAlive(targets[i]))
                    targets.RemoveAt(i);
            }
        }

        private static bool IsReferenceAlive(ICombatTarget target)
        {
            if (target is null)
                return false;

            if (target is Object unityObject)
                return unityObject != null;

            return true;
        }
    }
}
