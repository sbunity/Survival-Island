namespace Watermelon
{
    [System.Serializable]
    public class PeriodicRaidSave : ISaveObject
    {
        public float SelectedInterval;
        public float SecondsElapsed;

        public void OnBeforeSave() { }
    }
}
