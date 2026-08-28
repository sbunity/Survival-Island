using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIMinigameHost : UIPage
    {
        private readonly Vector2 DEFAULT_POSITION = new Vector2(0, 0);
        private readonly Vector2 HIDE_POSITION = new Vector2(0, -2000);

        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] RectTransform contentRoot;
        [SerializeField] Button closeButton;

        [SerializeField, Range(0f, 1f)] float fadeAlpha = 0f;

        [Space]
        [SerializeField] MinigameBackground background;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] TMP_Text stakeText;

        [BoxGroup("Result Popup", "Result Popup")]
        [SerializeField] GameObject resultPopup;
        [BoxGroup("Result Popup")]
        [SerializeField] Image resultFadeImage;
        [BoxGroup("Result Popup")]
        [SerializeField] RectTransform resultPanelRectTransform;
        [BoxGroup("Result Popup")]
        [SerializeField] TMP_Text resultTitleText;
        [BoxGroup("Result Popup")]
        [SerializeField] TMP_Text resultRewardText;
        [BoxGroup("Result Popup")]
        [SerializeField] Button resultButton;
        [BoxGroup("Result Popup")]
        [SerializeField] TMP_Text resultButtonText;
        [BoxGroup("Result Popup")]
        [SerializeField, Min(0.01f)] float resultAnimationDuration = 0.4f;
        [BoxGroup("Result Popup")]
        [SerializeField, Range(0f, 1f)] float resultFadeAlpha = 0.5f;

        [BoxGroup("Captions", "Captions")]
        [SerializeField] string stakeCaptionFormat = "Bet: {0}";
        [BoxGroup("Captions")]
        [SerializeField] string prizeCaptionFormat = "Prize: {0}";
        [BoxGroup("Captions")]
        [SerializeField] string winCaption = "You won!";
        [BoxGroup("Captions")]
        [SerializeField] string loseCaption = "You lost";
        [BoxGroup("Captions")]
        [SerializeField] string winButtonCaption = "Collect";
        [BoxGroup("Captions")]
        [SerializeField] string loseButtonCaption = "Good luck next time";

        private TraderMinigameDefinition definition;
        private IMinigameStakeRule stakeRule;
        private MinigameContext context;

        private MinigameFinishedCallback settledCallback;
        private SimpleCallback closedCallback;

        private MinigameView activeView;

        private bool isSettled;

        private TweenCase resultFadeCase;
        private TweenCase resultPanelCase;

        public override void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            if (resultButton != null)
                resultButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public void Play(TraderMinigameDefinition definition, IMinigameStakeRule stakeRule, MinigameContext context, MinigameFinishedCallback onSettled, SimpleCallback onClosed)
        {
            this.definition = definition;
            this.stakeRule = stakeRule;
            this.context = context;

            settledCallback = onSettled;
            closedCallback = onClosed;

            UIController.ShowPage<UIMinigameHost>();
        }

        protected override void OnShow()
        {
            isSettled = false;

            fadeImage.color = fadeImage.color.SetAlpha(0.0f);
            fadeImage.DOFade(fadeAlpha, 0.3f);

            panelRectTransform.anchoredPosition = HIDE_POSITION;
            panelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, 0.3f).SetEasing(Ease.Type.CircOut);

            BuildHeader();

            HideResult();

            closeButton.gameObject.SetActive(true);

            SpawnView();

            NotifyOpened();
        }

        protected override void OnHide()
        {
            SettleIfNeeded(MinigameResult.Abandoned);

            resultFadeCase.KillActive();
            resultPanelCase.KillActive();

            fadeImage.DOFade(0, 0.3f);
            panelRectTransform.DOAnchoredPosition(HIDE_POSITION, 0.3f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
            {
                ClearView();

                var callback = closedCallback;
                closedCallback = null;

                callback?.Invoke();

                NotifyClosed();
            });
        }

        protected override void OnUnload()
        {
            StopView();
            SettleIfNeeded(MinigameResult.Abandoned);
            ClearView();

            closedCallback = null;
            settledCallback = null;
        }

        private void BuildHeader()
        {
            if (definition == null)
                return;

            if (background != null)
                background.SetSprite(definition.Background);

            if (iconImage != null)
            {
                iconImage.sprite = definition.Icon;
                iconImage.gameObject.SetActive(definition.Icon != null);
            }

            if (titleText != null)
                titleText.text = definition.Title;

            if (descriptionText != null)
                descriptionText.text = definition.Description;

            if (stakeText != null)
            {
                stakeText.text = stakeRule != null && stakeRule.Stake.amount > 0
                    ? string.Format(stakeCaptionFormat, TraderResourceFormat.Format(stakeRule.Stake))
                    : string.Format(prizeCaptionFormat, TraderResourceFormat.Format(stakeRule?.Prize));
            }
        }

        private void SpawnView()
        {
            ClearView();

            activeView = definition != null ? definition.CreateView(contentRoot) : null;

            if (activeView == null)
            {
                Debug.LogError($"[Trader Minigames]: \"{(definition != null ? definition.name : "null")}\" failed to create its view.", definition);

                SettleIfNeeded(MinigameResult.Abandoned);
                Close();

                return;
            }

            activeView.gameObject.SetActive(true);
            activeView.Finished += OnGameFinished;
            activeView.Run(context);
        }

        private void OnGameFinished(MinigameResult result)
        {
            SettleIfNeeded(result);

            ShowResult(result);
        }

        private void ShowResult(MinigameResult result)
        {
            if (resultPopup == null)
            {
                Close();

                return;
            }

            resultPopup.SetActive(true);

            closeButton.gameObject.SetActive(false);

            if (resultTitleText != null)
                resultTitleText.text = result.IsWin ? winCaption : loseCaption;

            if (resultRewardText != null)
            {
                resultRewardText.text = result.IsWin && stakeRule != null ? TraderResourceFormat.Format(stakeRule.Prize) : string.Empty;
                resultRewardText.gameObject.SetActive(result.IsWin);
            }

            if (resultButtonText != null)
                resultButtonText.text = result.IsWin ? winButtonCaption : loseButtonCaption;

            PlayResultAppearance();

            AudioController.PlaySound(AudioController.GetClip(result.IsWin ? "reward" : "button_sound"), 0.7f);
        }

        private void PlayResultAppearance()
        {
            resultFadeCase.KillActive();
            resultPanelCase.KillActive();

            if (resultPanelRectTransform != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(resultPanelRectTransform);

            if (resultFadeImage != null)
            {
                resultFadeImage.color = resultFadeImage.color.SetAlpha(0f);
                resultFadeCase = resultFadeImage.DOFade(resultFadeAlpha, resultAnimationDuration);
            }

            if (resultPanelRectTransform != null)
            {
                resultPanelRectTransform.anchoredPosition = HIDE_POSITION;
                resultPanelCase = resultPanelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, resultAnimationDuration).SetEasing(Ease.Type.CircOut);
            }
        }

        private void HideResult()
        {
            resultFadeCase.KillActive();
            resultPanelCase.KillActive();

            if (resultFadeImage != null)
                resultFadeImage.color = resultFadeImage.color.SetAlpha(0f);

            if (resultPanelRectTransform != null)
                resultPanelRectTransform.anchoredPosition = HIDE_POSITION;

            if (resultPopup != null)
                resultPopup.SetActive(false);
        }

        private void SettleIfNeeded(MinigameResult result)
        {
            if (isSettled)
                return;

            isSettled = true;

            var callback = settledCallback;
            settledCallback = null;

            callback?.Invoke(result);
        }

        private void StopView()
        {
            if (activeView == null)
                return;

            activeView.Finished -= OnGameFinished;
            activeView.Stop();
        }

        private void ClearView()
        {
            if (activeView == null)
                return;

            activeView.Finished -= OnGameFinished;

            Destroy(activeView.gameObject);

            activeView = null;
        }

        private void Close()
        {
            UIController.HidePage<UIMinigameHost>();
        }

        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            StopView();

            Close();
        }
    }
}
