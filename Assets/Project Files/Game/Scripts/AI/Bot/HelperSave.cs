namespace Watermelon
{
    [System.Serializable]
    public class HelperSave : ISaveObject
    {
        public bool IsOpened;
        public bool HasHealthData;
        public float CurrentHealth;
        public bool IsRecovering;

        public void OnBeforeSave()
        {

        }
    }
}
