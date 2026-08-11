using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class HelperRoster
    {
        private const string SAVE_KEY = "helperRoster";

        private static readonly List<HelperData> guestsBuffer = new();

        private static HelpersDatabase Database => GameController.Data != null ? GameController.Data.HelpersDatabase : null;

        private static HelperRosterSave GetSave()
        {
            return SaveController.GetSaveObject<HelperRosterSave>(SAVE_KEY);
        }

        public static string GetWorldId(HelperData helperData)
        {
            if (helperData == null)
                return string.Empty;

            var rosterSave = GetSave();
            var entry = rosterSave?.GetEntry(helperData.GlobalId);

            if (entry != null && !string.IsNullOrEmpty(entry.WorldId))
                return entry.WorldId;

            return helperData.HomeWorldId;
        }

        public static bool IsInWorld(HelperData helperData, string worldId)
        {
            if (helperData == null || string.IsNullOrEmpty(worldId))
                return false;

            return GetWorldId(helperData) == worldId;
        }

        public static void Move(HelperData helperData, string toWorldId, bool isOpened)
        {
            if (helperData == null || string.IsNullOrEmpty(toWorldId))
                return;

            var fromWorldId = GetWorldId(helperData);
            if (fromWorldId == toWorldId)
                return;

            var rosterSave = GetSave();
            if (rosterSave == null)
                return;

            rosterSave.GetOrCreateEntry(helperData.GlobalId).WorldId = toWorldId;

            SeedDestinationSave(helperData, toWorldId, isOpened);

            IdleProduction.InvalidateWorld(fromWorldId);
            IdleProduction.InvalidateWorld(toWorldId);

            SaveController.MarkAsSaveIsRequired();
        }

        public static List<HelperData> GetGuests(string worldId)
        {
            guestsBuffer.Clear();

            var database = Database;
            if (database == null || string.IsNullOrEmpty(worldId) || database.Helpers.IsNullOrEmpty())
                return guestsBuffer;

            foreach (var helperData in database.Helpers)
            {
                if (helperData == null || helperData.HomeWorldId == worldId)
                    continue;

                if (IsInWorld(helperData, worldId))
                    guestsBuffer.Add(helperData);
            }

            return guestsBuffer;
        }

        private static void SeedDestinationSave(HelperData helperData, string toWorldId, bool isOpened)
        {
            var helperSave = SaveController.GetSaveObject<HelperSave>(toWorldId, HelperBehavior.GetSaveKey(helperData.GlobalId));
            if (helperSave == null)
                return;

            helperSave.IsOpened = isOpened;
            helperSave.HasHealthData = false;
            helperSave.IsRecovering = false;
        }
    }
}
