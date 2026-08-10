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
                LogDiagnostic($"[Idle Production]: {report}");

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

            WorldProductionSnapshotBuilder.Capture(world, snapshot, worldId, Settings);

            LogCapturedRates(worldId, snapshot);

            snapshot.SetLive(false);

            SaveController.MarkAsSaveIsRequired();
        }

        private static void LogCapturedRates(string worldId, WorldProductionSnapshot snapshot)
        {
            if (snapshot.Producers.IsNullOrEmpty())
                return;

            var builder = new System.Text.StringBuilder();
            builder.Append("[Idle Production]: captured ").Append(worldId);

            foreach (var producer in snapshot.Producers)
            {
                builder.Append(" | ").Append(producer.HelperId.Substring(0, 8))
                    .Append(" authored=").Append(producer.AuthoredUnitsPerMinute.ToString("F1"))
                    .Append(" measured=").Append(producer.MeasuredUnitsPerMinute.ToString("F1"))
                    .Append("/min over ").Append(producer.SampleMinutes.ToString("F1")).Append("min");
            }

            LogDiagnostic(builder.ToString());
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private static void LogDiagnostic(string message)
        {
            Debug.Log(message);
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
