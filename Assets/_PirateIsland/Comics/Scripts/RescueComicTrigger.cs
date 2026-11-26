using UnityEngine;

namespace Watermelon
{
    public class RescueComicTrigger : MonoBehaviour, IWorldElement
    {
        [SerializeField] HelperBehavior helper;
        [SerializeField] ComicPlayer comicPrefab;
        [SerializeField, Min(0f)] float delay = 1.5f;

        public int InitialisationOrder => -20;
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private bool isSubscribed;
        private TweenCase delayTweenCase;

        public void OnWorldLoaded()
        {
            if (helper == null || comicPrefab == null || helper.IsOpened)
                return;

            helper.HelperUnlocked += OnHelperUnlocked;
            isSubscribed = true;
        }

        public void OnWorldUnloaded()
        {
            Unsubscribe();

            delayTweenCase.KillActive();
        }

        private void OnHelperUnlocked()
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

            helper.HelperUnlocked -= OnHelperUnlocked;
            isSubscribed = false;
        }

        private void OnDestroy()
        {
            Unsubscribe();

            delayTweenCase.KillActive();
        }
    }
}
