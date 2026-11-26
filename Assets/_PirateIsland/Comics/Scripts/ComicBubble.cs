using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ComicBubble : MonoBehaviour
    {
        [SerializeField] TypewriterText typewriter;

        [Space]
        [SerializeField] float delayBefore = 0.15f;
        [SerializeField] float popDuration = 0.25f;
        [SerializeField] float hiddenScale = 0.6f;

        private CanvasGroup canvasGroup;
        private CanvasGroup CanvasGroup => canvasGroup != null ? canvasGroup : canvasGroup = GetComponent<CanvasGroup>();

        private TweenCase delayTweenCase;
        private TweenCase fadeTweenCase;
        private TweenCase scaleTweenCase;

        private SimpleCallback completedCallback;

        public int CharacterCount => typewriter != null ? typewriter.CharacterCount : 0;

        public void Prepare()
        {
            KillTweens();
            completedCallback = null;

            CanvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * hiddenScale;

            if (typewriter != null)
                typewriter.Clear();
        }

        public void Play(SimpleCallback onCompleted)
        {
            Prepare();
            completedCallback = onCompleted;

            delayTweenCase = Tween.DelayedCall(delayBefore, () =>
            {
                fadeTweenCase = CanvasGroup.DOFade(1f, popDuration * 0.6f, unscaledTime: true);
                scaleTweenCase = transform.DOScale(Vector3.one, popDuration, unscaledTime: true).SetEasing(Ease.Type.BackOut);

                if (typewriter != null)
                {
                    typewriter.Play(NotifyCompleted);
                }
                else
                {
                    scaleTweenCase.OnComplete(NotifyCompleted);
                }
            }, unscaledTime: true);
        }

        public void Complete()
        {
            KillTweens();

            CanvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            if (typewriter != null)
                typewriter.Complete();

            NotifyCompleted();
        }

        private void NotifyCompleted()
        {
            SimpleCallback callback = completedCallback;
            completedCallback = null;
            callback?.Invoke();
        }

        private void KillTweens()
        {
            delayTweenCase.KillActive();
            fadeTweenCase.KillActive();
            scaleTweenCase.KillActive();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
