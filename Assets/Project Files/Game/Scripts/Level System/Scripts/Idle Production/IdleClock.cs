namespace Watermelon
{
    public static class IdleClock
    {
        public static float Now
        {
            get
            {
                var timeSave = GetTimeSave();

                return timeSave != null ? timeSave.GameTime : 0f;
            }
        }

        public static TimeSave GetTimeSave()
        {
            var saveFile = SaveController.GetFile(SaveController.DEFAULT_FILE_NAME);

            return saveFile?.GetSaveObject<TimeSave>();
        }
    }
}
