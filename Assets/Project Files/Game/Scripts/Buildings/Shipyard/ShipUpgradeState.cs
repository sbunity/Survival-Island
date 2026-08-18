namespace Watermelon
{
    public static class ShipUpgradeState
    {
        private const string SAVE_KEY = "ship_upgrades";

        private static ShipyardSave Save => SaveController.GetSaveObject<ShipyardSave>(SAVE_KEY);

        public static bool IsCompleted(string stageId)
        {
            if (string.IsNullOrEmpty(stageId))
                return false;

            var save = Save;

            return save != null && save.CompletedStageIds.Contains(stageId);
        }

        public static void MarkCompleted(string stageId)
        {
            if (string.IsNullOrEmpty(stageId))
                return;

            var save = Save;
            if (save == null || save.CompletedStageIds.Contains(stageId))
                return;

            save.CompletedStageIds.Add(stageId);
        }

        public static ShipUpgradeStage GetNextStage(ShipUpgradesDatabase database)
        {
            if (database == null || database.Stages.IsNullOrEmpty())
                return null;

            var stages = database.Stages;

            for (var i = 0; i < stages.Length; i++)
            {
                var stage = stages[i];

                if (stage == null || string.IsNullOrEmpty(stage.ID))
                    continue;

                if (!IsCompleted(stage.ID))
                    return stage;
            }

            return null;
        }
    }
}
