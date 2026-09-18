namespace Watermelon
{
    public static class WorldMissionsProgress
    {
        private const string SAVE_FORMAT = "worldMissions_{0}";

        public static bool AreMissionsCompleted(string worldID)
        {
            if (string.IsNullOrEmpty(worldID))
                return false;

            return GetSave(worldID).AreMissionsCompleted;
        }

        public static void SetMissionsCompleted(string worldID, bool areMissionsCompleted)
        {
            if (string.IsNullOrEmpty(worldID))
                return;

            var save = GetSave(worldID);

            if (save.AreMissionsCompleted == areMissionsCompleted)
                return;

            save.AreMissionsCompleted = areMissionsCompleted;

            SaveController.MarkAsSaveIsRequired();
        }

        public static bool HasPendingMissions(WorldData worldData)
        {
            return worldData != null && worldData.IsUnlocked && !AreMissionsCompleted(worldData.ID);
        }

        public static bool TryGetWorldWithPendingMissions(string excludedWorldID, out WorldData worldData)
        {
            worldData = null;

            var worlds = WorldController.Database.Worlds;

            if (worlds.IsNullOrEmpty())
                return false;

            foreach (var world in worlds)
            {
                if (world == null)
                    continue;

                if (!string.IsNullOrEmpty(excludedWorldID) && world.ID == excludedWorldID)
                    continue;

                if (!HasPendingMissions(world))
                    continue;

                worldData = world;

                return true;
            }

            return false;
        }

        private static WorldMissionsProgressSave GetSave(string worldID)
        {
            return SaveController.GetSaveObject<WorldMissionsProgressSave>(string.Format(SAVE_FORMAT, worldID));
        }
    }
}
