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

            snapshot.SetLive(true, () => BankLiveMeasurements(worldId));

            IdleProductionTracker.BeginSession();
        }

        public static void CaptureLiveWorld(BaseWorldBehavior world)
        {
            if (world == null || world.WorldData == null)
                return;

            string worldId = world.WorldData.ID;

            var snapshot = GetSnapshot(worldId);
            if (snapshot == null)
                return;

            CaptureAndReset(world, snapshot, worldId);

            LogCapturedRates(worldId, snapshot);

            snapshot.SetLive(false);

            SaveController.MarkAsSaveIsRequired();
        }

        private static void BankLiveMeasurements(string worldId)
        {
            var world = WorldController.WorldBehavior;

            if (world == null || world.WorldData == null || world.WorldData.ID != worldId)
                return;

            var snapshot = GetSnapshot(worldId);
            if (snapshot == null)
                return;

            CaptureAndReset(world, snapshot, worldId);
        }

        private static void CaptureAndReset(BaseWorldBehavior world, WorldProductionSnapshot snapshot, string worldId)
        {
            if (!WorldProductionSnapshotBuilder.Capture(world, snapshot, worldId, Settings))
                return;

            IdleProductionTracker.BeginSession();

            var helpers = world.GetComponentsInChildren<HelperBehavior>(true);
            foreach (var helper in helpers)
            {
                if (helper != null)
                    helper.ResetIdleMeasurement();
            }
        }

        private static void LogCapturedRates(string worldId, WorldProductionSnapshot snapshot)
        {
            if (snapshot.Producers.IsNullOrEmpty())
                return;

            var builder = new System.Text.StringBuilder();
            builder.Append("[Idle Production]: captured ").Append(worldId);

            foreach (var producer in snapshot.Producers)
            {
                builder.Append(" | ").Append(ShortId(producer.HelperId))
                    .Append(" authored=").Append(producer.AuthoredUnitsPerMinute.ToString("F1"))
                    .Append(" measured=").Append(producer.MeasuredUnitsPerMinute.ToString("F1"))
                    .Append("/min over ").Append(producer.SampleMinutes.ToString("F1")).Append("min");
            }

            LogDiagnostic(builder.ToString());
        }

        private static string ShortId(string helperId)
        {
            if (string.IsNullOrEmpty(helperId))
                return "<no id>";

            return helperId.Length > 8 ? helperId.Substring(0, 8) : helperId;
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

            if (!snapshot.IsLive)
                Simulate(worldId);

            snapshot.Invalidate();

            SaveController.MarkAsSaveIsRequired();
        }
    }
}
