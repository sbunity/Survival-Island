using UnityEngine;

namespace Watermelon
{
    public abstract class ComicTrigger : MonoBehaviour, IWorldElement
    {
        [SerializeField] ComicPlayer comicPrefab;
        [SerializeField, Min(0f)] float delay = 1.5f;

        public int InitialisationOrder => -20;
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private bool isArmed;
        private TweenCase delayTweenCase;

        public void OnWorldLoaded()
        {
            if (comicPrefab == null || !CanArm())
                return;

            Subscribe();
            isArmed = true;
        }

        public void OnWorldUnloaded()
        {
            Disarm();

            delayTweenCase.KillActive();
        }

        protected abstract bool CanArm();
        protected abstract void Subscribe();
        protected abstract void Unsubscribe();

        protected void Fire()
        {
            Disarm();

            delayTweenCase.KillActive();
            delayTweenCase = Tween.DelayedCall(delay, () =>
            {
                UIComicCutscene.Play(comicPrefab);
            });
        }

        private void Disarm()
        {
            if (!isArmed)
                return;

            isArmed = false;
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Disarm();

            delayTweenCase.KillActive();
        }
    }
}
