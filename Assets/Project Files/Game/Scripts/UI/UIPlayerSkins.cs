using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;

namespace Watermelon
{
    public sealed class UIPlayerSkins : UIPage
    {
        private const float TransitionDuration = 0.4f;
        private static readonly Vector2 HiddenPosition = new(0f, -2000f);
        private static readonly Color LockedPreviewColor = new(0.62f, 0.62f, 0.62f);

        [SerializeField] private Image fadeImage;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image previewImage;
        [SerializeField] private Image lockIcon;
        [SerializeField] private TMP_Text pageIndicator;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionLabel;

        private SkinController skinController;
        private PlayerSkinsDatabase database;
        private int skinIndex;
        private bool holdsMovementLock;
        private bool isClosing;
        private TweenCase fadeTween;
        private TweenCase panelTween;

        public override bool IsPopup => true;

        public override void Init()
        {
            skinController = SkinController.Instance;
            database = skinController.GetSelectedSkin<PlayerSkinsDatabase>()?.SkinsProvider as PlayerSkinsDatabase;

            previousButton.onClick.AddListener(() => Browse(-1));
            nextButton.onClick.AddListener(() => Browse(1));
            closeButton.onClick.AddListener(OnCloseClicked);
            actionButton.onClick.AddListener(OnActionClicked);
        }

        protected override void OnShow()
        {
            isClosing = false;
            holdsMovementLock = true;
            MovementLock.Acquire();

            var selectedSkin = skinController.GetSelectedSkin<PlayerSkinsDatabase>();
            skinIndex = database != null ? Mathf.Max(0, System.Array.IndexOf(database.Skins, selectedSkin as PlayerSkinData)) : 0;
            RefreshPreview();
            SkinController.SkinUnlocked += OnSkinUnlocked;

            fadeImage.color = fadeImage.color.SetAlpha(0f);
            panel.anchoredPosition = HiddenPosition;
            Animate(0.25f, Vector2.zero, Ease.Type.CircOut);

            NotifyOpened();
        }

        protected override void OnHide()
        {
            isClosing = true;
            Animate(0f, HiddenPosition, Ease.Type.CircIn);
            panelTween.OnComplete(() =>
            {
                ReleasePage();
                NotifyClosed();
            });
        }

        protected override void OnUnload()
        {
            fadeTween.KillActive();
            panelTween.KillActive();
            ReleasePage();
        }

        private void OnDestroy()
        {
            OnUnload();
        }

        private void ReleasePage()
        {
            SkinController.SkinUnlocked -= OnSkinUnlocked;

            if (!holdsMovementLock)
                return;

            holdsMovementLock = false;
            MovementLock.Release();
        }

        private void Animate(float fade, Vector2 position, Ease.Type easing)
        {
            fadeTween.KillActive();
            panelTween.KillActive();
            fadeTween = fadeImage.DOFade(fade, TransitionDuration, unscaledTime: true);
            panelTween = panel.DOAnchoredPosition(position, TransitionDuration, unscaledTime: true).SetEasing(easing);
        }

        private void Browse(int direction)
        {
            if (isClosing || database == null || database.SkinsCount < 2)
                return;

            PlayButtonFeedback();
            skinIndex = (skinIndex + direction + database.SkinsCount) % database.SkinsCount;
            RefreshPreview();
        }

        private void OnSkinUnlocked(ISkinData skin)
        {
            if (database != null && skin.SkinsProvider == database)
                RefreshPreview();
        }

        private void RefreshPreview()
        {
            int count = database != null ? database.SkinsCount : 0;
            bool hasSkin = count > 0;
            var skin = hasSkin ? database.Skins[skinIndex] : null;
            bool isUnlocked = hasSkin && skin.IsUnlocked;

            previewImage.sprite = skin?.PreviewSprite;
            previewImage.enabled = previewImage.sprite != null;
            previewImage.color = isUnlocked ? Color.white : LockedPreviewColor;
            lockIcon.gameObject.SetActive(hasSkin && !isUnlocked);
            pageIndicator.gameObject.SetActive(count > 1);
            pageIndicator.SetText("{0} / {1}", skinIndex + 1, count);
            previousButton.gameObject.SetActive(count > 1);
            nextButton.gameObject.SetActive(count > 1);
            actionButton.gameObject.SetActive(hasSkin);
            actionLabel.text = isUnlocked ? "Select" : "Go to Store";
        }

        private void OnCloseClicked()
        {
            if (!IsPageDisplayed || isClosing)
                return;

            PlayButtonFeedback();
            UIController.HidePage(this);
        }

        private void OnActionClicked()
        {
            if (!IsPageDisplayed || isClosing || database == null || database.SkinsCount == 0)
                return;

            PlayButtonFeedback();
            var skin = database.Skins[skinIndex];

            if (!skin.IsUnlocked)
            {
                UIController.HidePage(this, UIStore.OpenAsOverlay);
                return;
            }

            if (!skinController.IsSkinSelected(skin))
                skinController.SelectSkin(skin);

            UIController.HidePage(this);
        }

        private static void PlayButtonFeedback()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));
        }
    }
}
