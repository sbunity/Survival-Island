using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Data/Game Data")]
    public class GameData : ScriptableObject
    {
        [Space]
        [SerializeField] EnemiesDatabase enemiesDatabase;
        public EnemiesDatabase EnemiesDatabase => enemiesDatabase;

        [BoxGroup("Storage Sound", "Storage Sound Data")]
        [SerializeField, Range(0, 1)] float storageSoundStartTime = 0.8f;
        [BoxGroup("Storage Sound")]
        [SerializeField] AudioClipHandler storageSoundHandler;

        public AudioClipHandler StorageSoundHandler => storageSoundHandler;
        public float StorageSoundStartTime => storageSoundStartTime;

        [BoxGroup("Steps Sound", "Steps Sound")]
        [SerializeField] DuoFloat stepsVolumeRange = new DuoFloat(0.4f, 0.7f);
        [BoxGroup("Steps Sound")]
        [SerializeField] float minSpeedToTriggerSteps = 0.2f;

        public DuoFloat StepsVolumeRange => stepsVolumeRange;
        public float MinSpeedToTriggerSteps => minSpeedToTriggerSteps;

        public void Init()
        {
            enemiesDatabase.Init();
        }

        public void Unload()
        {
            enemiesDatabase.Unload();
        }
    }
}
