using UnityEngine;

namespace Watermelon
{
    public sealed class UIComicCutscene : UIPage
    {
        private const float FadeDuration = 0.35f;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform safeAreaRectTransform;
        [SerializeField] private RectTransform comicHolder;

        private ComicPlayer comicPrefab;
        private ComicPlayer comic;
        private SimpleCallback finishedCallback;
        private bool holdsMovementLock;
        private TweenCase fadeTween;

        public override bool IsPopup => true;
        public override bool IsPausePopup => true;

        public static void Play(ComicPlayer comicPrefab, SimpleCallback onFinished = null)
        {
            if (comicPrefab == null)
            {
                onFinished?.Invoke();
                return;
            }

            UIComicCutscene page = UIController.GetPage<UIComicCutscene>();
            if (page == null || page.IsPageDisplayed)
            {
                onFinished?.Invoke();
                return;
            }

            page.comicPrefab = comicPrefab;
            page.finishedCallback = onFinished;

            UIController.ShowPage(page);
        }

        public override void Init()
        {
            SafeAreaAdapter.RegisterRectTransform(safeAreaRectTransform);
        }

        protected override void OnShow()
        {
            holdsMovementLock = true;
            MovementLock.Acquire();

            comic = Instantiate(comicPrefab, comicHolder);

            fadeTween.KillActive();
            canvasGroup.alpha = 0f;
            fadeTween = canvasGroup.DOFade(1f, FadeDuration, unscaledTime: true).OnComplete(() =>
            {
                NotifyOpened();

                comic.Play(OnComicFinished);
            });
        }

        protected override void OnHide()
        {
            fadeTween.KillActive();
            fadeTween = canvasGroup.DOFade(0f, FadeDuration, unscaledTime: true).OnComplete(() =>
            {
                ReleasePage();
                NotifyClosed();
            });
        }

        protected override void OnUnload()
        {
            fadeTween.KillActive();
            ReleasePage();
        }

        private void OnDestroy()
        {
            OnUnload();

            SafeAreaAdapter.UnregisterRectTransform(safeAreaRectTransform);
        }

        private void OnComicFinished()
        {
            SimpleCallback callback = finishedCallback;
            finishedCallback = null;

            UIController.HidePage<UIComicCutscene>(callback);
        }

        private void ReleasePage()
        {
            if (comic != null)
            {
                Destroy(comic.gameObject);
                comic = null;
            }

            if (!holdsMovementLock)
                return;

            holdsMovementLock = false;
            MovementLock.Release();
        }
    }
}
