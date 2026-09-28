using System.Collections.Generic;

namespace Watermelon
{
    public static class ResourceUnlocks
    {
        private static readonly Dictionary<CurrencyType, ResourceUnlockRule> rules = new Dictionary<CurrencyType, ResourceUnlockRule>();
        private static readonly List<CurrencyType> buffer = new List<CurrencyType>();

        public static void Initialise(ResourceUnlockRule[] source)
        {
            rules.Clear();

            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
            {
                if (!rules.TryAdd(source[i].Currency, source[i]))
                    UnityEngine.Debug.LogWarning($"[Resource Unlocks]: Duplicated rule for \"{source[i].Currency}\", the first one is used.");
            }
        }

        public static bool IsUnlocked(CurrencyType currency)
        {
            if (MissionsActionMenu.AreMissionsDisabled())
                return true;

            return !rules.TryGetValue(currency, out ResourceUnlockRule rule) || rule.IsUnlocked;
        }

        public static bool AreAllUnlocked(Resource[] resources)
        {
            if (resources == null)
                return true;

            for (var i = 0; i < resources.Length; i++)
            {
                if (!IsUnlocked(resources[i].currency))
                    return false;
            }

            return true;
        }

        public static bool AreAllUnlocked(CurrencyType[] currencies)
        {
            if (currencies == null)
                return true;

            for (var i = 0; i < currencies.Length; i++)
            {
                if (!IsUnlocked(currencies[i]))
                    return false;
            }

            return true;
        }

        public static bool HasAnyUnlocked(CurrencyType[] currencies)
        {
            if (currencies.IsNullOrEmpty())
                return false;

            for (var i = 0; i < currencies.Length; i++)
            {
                if (IsUnlocked(currencies[i]))
                    return true;
            }

            return false;
        }

        public static int FilterUnlocked(CurrencyType[] currencies, List<CurrencyType> result)
        {
            result.Clear();

            if (currencies == null)
                return 0;

            for (var i = 0; i < currencies.Length; i++)
            {
                if (IsUnlocked(currencies[i]))
                    result.Add(currencies[i]);
            }

            return result.Count;
        }

        public static bool TryPickUnlocked(CurrencyType[] currencies, out CurrencyType picked)
        {
            picked = default;

            if (FilterUnlocked(currencies, buffer) == 0)
                return false;

            picked = buffer[UnityEngine.Random.Range(0, buffer.Count)];

            return true;
        }
    }
}
