using UnityEngine;

namespace Watermelon
{
    public static class IdleProduction
    {
        private static IdleProductionSettings Settings => GameController.Data != null ? GameController.Data.IdleSettings : null;

        public static WorldProductionSnapshot GetSnapshot(string worldId)
        {
            if (string.IsNullOrEmpty(worldId))
                return null;

            return SaveController.GetSaveObject<WorldProductionSnapshot>(worldId, WorldProductionSnapshot.SAVE_KEY);
        }

        public static IdleProductionReport Simulate(string worldId)
        {
            var snapshot = GetSnapshot(worldId);
            if (snapshot == null)
                return null;

            var report = IdleProductionSimulator.Simulate(worldId, snapshot, Settings);

            if (report != null && !report.IsEmpty)
                LogManager.Log($"[Idle Production]: {report}", LogCategory.Systems);

            return report;
        }

        public static void BeginLiveWorld(string worldId)
        {
            var snapshot = GetSnapshot(worldId);
            if (snapshot == null)
                return;

            snapshot.SetLive(true);
        }

        public static void CaptureLiveWorld(BaseWorldBehavior world)
        {
            if (world == null || world.WorldData == null)
                return;

            string worldId = world.WorldData.ID;

            var snapshot = GetSnapshot(worldId);
            if (snapshot == null)
                return;

            WorldProductionSnapshotBuilder.Capture(world, snapshot, worldId);

            snapshot.SetLive(false);

            SaveController.MarkAsSaveIsRequired();
        }

        public static void InvalidateWorld(string worldId)
        {
            var snapshot = GetSnapshot(worldId);
            if (snapshot == null)
                return;

            snapshot.Invalidate();

            SaveController.MarkAsSaveIsRequired();
        }
    }
}
