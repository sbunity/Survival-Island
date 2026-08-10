using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class IdleProductionTracker
    {
        private static readonly Dictionary<string, ResourcesList> deliveries = new();

        private static float sessionStartGameTime;

        public static float SessionMinutes => Mathf.Max(0f, (IdleClock.Now - sessionStartGameTime) / 60f);

        public static void BeginSession()
        {
            deliveries.Clear();

            sessionStartGameTime = IdleClock.Now;
        }

        public static void RecordDelivery(string storageSaveKey, CurrencyType currency, int amount)
        {
            if (string.IsNullOrEmpty(storageSaveKey) || amount <= 0)
                return;

            if (!deliveries.TryGetValue(storageSaveKey, out var list))
            {
                list = new ResourcesList();

                deliveries[storageSaveKey] = list;
            }

            deliveries[storageSaveKey] = list + new Resource(currency, amount);
        }

        public static ResourcesList GetDeliveries(string storageSaveKey)
        {
            if (string.IsNullOrEmpty(storageSaveKey))
                return null;

            return deliveries.TryGetValue(storageSaveKey, out var list) ? list : null;
        }
    }
}
