namespace Watermelon
{
    [System.Serializable]
    public class PeriodicRaidSave : ISaveObject
    {
        public float NextRaidAtGameTime;

        public void OnBeforeSave() { }
    }
}
