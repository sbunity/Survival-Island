using UnityEngine;

namespace Watermelon
{
    public class RescueComicTrigger : MonoBehaviour, IWorldElement
    {
        [SerializeField] GuardedSkeletonEncounter encounter;
        [SerializeField] ComicPlayer comicPrefab;
        [SerializeField, Min(0f)] float delay = 1.5f;

        public int InitialisationOrder => -20;
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private bool isSubscribed;
        private TweenCase delayTweenCase;

        public void OnWorldLoaded()
        {
            if (encounter == null || comicPrefab == null || encounter.IsCleared)
                return;

            encounter.Cleared += OnEncounterCleared;
            isSubscribed = true;
        }

        public void OnWorldUnloaded()
        {
            Unsubscribe();

            delayTweenCase.KillActive();
        }

        private void OnEncounterCleared()
        {
            Unsubscribe();

            delayTweenCase = Tween.DelayedCall(delay, () =>
            {
                UIComicCutscene.Play(comicPrefab);
            });
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            encounter.Cleared -= OnEncounterCleared;
            isSubscribed = false;
        }

        private void OnDestroy()
        {
            Unsubscribe();

            delayTweenCase.KillActive();
        }
    }
}
