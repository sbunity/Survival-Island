namespace Watermelon
{
    [System.Serializable]
    public class BossSkeletonSave : ISaveObject
    {
        public bool IsInitialised;
        public bool IsDead;
        public float CurrentHealth;

        public void Initialise(float maxHealth)
        {
            if (IsInitialised)
                return;

            IsInitialised = true;
            CurrentHealth = maxHealth;
        }

        public void OnBeforeSave()
        {

        }
    }
}
