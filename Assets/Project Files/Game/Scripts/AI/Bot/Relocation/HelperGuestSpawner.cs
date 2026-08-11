using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class HelperGuestSpawner
    {
        public static HelperBehavior[] Spawn(BaseWorldBehavior world, WorldData worldData)
        {
            if (world == null || worldData == null)
                return null;

            var guests = HelperRoster.GetGuests(worldData.ID);
            if (guests.Count == 0)
                return null;

            var spawnedHelpers = new List<HelperBehavior>(guests.Count);

            foreach (var guestData in guests)
            {
                if (guestData.GuestPrefab == null)
                {
                    Debug.LogError($"[Helper Roster]: '{guestData.name}' has no guest prefab, it can't be spawned on {worldData.ID}!", guestData);

                    continue;
                }

                var helper = Object.Instantiate(guestData.GuestPrefab, world.transform);

                helper.name = string.IsNullOrEmpty(guestData.DisplayName) ? guestData.GuestPrefab.name : guestData.DisplayName;
                helper.transform.SetPositionAndRotation(world.GetHelperRestPosition(), Quaternion.identity);

                helper.SetupAsGuest(guestData);

                spawnedHelpers.Add(helper);
            }

            return spawnedHelpers.ToArray();
        }
    }
}
