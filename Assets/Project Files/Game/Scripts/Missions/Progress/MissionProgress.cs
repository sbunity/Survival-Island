namespace Watermelon
{
    public static class MissionProgress
    {
        private const string SAVE_KEY = "missionsProgress";

        public static bool IsCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
                return false;

            return GetSave().Contains(missionId);
        }

        public static void MarkCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
                return;

            if (!GetSave().Add(missionId))
                return;

            SaveController.MarkAsSaveIsRequired();
        }

        private static MissionProgressSave GetSave()
        {
            return SaveController.GetSaveObject<MissionProgressSave>(SAVE_KEY);
        }
    }
}
