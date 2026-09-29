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

        [BoxGroup("Frame", "Frame")]
        [SerializeField] RectTransform topBar;
        [BoxGroup("Frame")]
        [SerializeField] float topBarTopGap = 138f;
        [BoxGroup("Frame")]
        [SerializeField] float contentBottomGap = 40f;
        [BoxGroup("Frame")]
        [SerializeField] float contentTopGap;
        [BoxGroup("Frame")]
        [SerializeField, Min(1f)] float referencePanelHeight = 1920f;
        [BoxGroup("Frame")]
        [SerializeField, Range(0f, 1f)] float minGapScale = 0.3f;

        [SerializeField, Range(0f, 1f)] float fadeAlpha = 0f;

        [BoxGroup("Intro", "Intro")]
        [SerializeField] MinigameIntroElement topBarIntro;
        [BoxGroup("Intro")]
        [SerializeField, Min(0f)] float introStepInterval = 0.3f;

        [Space]
        [SerializeField] MinigameBackground background;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] TMP_Text stakeText;
        [SerializeField] TMP_Text wagerBadgeText;

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
        [SerializeField] string wagerCaptionFormat = "Bet {0} to win {1}";
        [BoxGroup("Captions")]
        [SerializeField] string wagerBadgeCaption = "Bet game";
        [BoxGroup("Captions")]
        [SerializeField] string prizeCaptionFormat = "Prize: {0}";
        [BoxGroup("Captions")]
        [SerializeField] string winCaption = "You won!";
        [BoxGroup("Captions")]
        [SerializeField] string loseCaption = "You lost";
        [BoxGroup("Captions")]
        [SerializeField] string lostStakeFormat = "Lost {0}";
        [BoxGroup("Captions")]
        [SerializeField] string winButtonCaption = "Collect";
        [BoxGroup("Captions")]
        [SerializeField] string loseButtonCaption = "Good luck next time";

        [BoxGroup("Audio", "Audio")]
        [SerializeField, Range(0f, 1f)] float musicDuckVolume = 0.6f;
        [BoxGroup("Audio")]
        [SerializeField, Min(0.01f)] float musicDuckDuration = 0.3f;

        private TraderMinigameDefinition definition;
        private IMinigameStakeRule stakeRule;
        private MinigameContext context;

        private MinigameFinishedCallback settledCallback;
        private SimpleCallback closedCallback;

        private MinigameView activeView;

        private readonly MinigameIntroSequence introSequence = new MinigameIntroSequence();

        private bool isSettled;

        private TweenCase resultFadeCase;
        private TweenCase resultPanelCase;

        private MusicSource duckedMusicSource;

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
            LayoutFrame();

            HideResult();

            closeButton.gameObject.SetActive(true);

            SpawnView();

            DuckMusic();

            NotifyOpened();
        }

        protected override void OnHide()
        {
            SettleIfNeeded(MinigameResult.Abandoned);

            introSequence.Stop();

            RestoreMusic();

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
            introSequence.Stop();

            RestoreMusic();

            StopView();
            SettleIfNeeded(MinigameResult.Abandoned);
            ClearView();

            closedCallback = null;
            settledCallback = null;
        }

        private void DuckMusic()
        {
            MusicSource musicSource = MusicSource.ActiveMusicSource;

            if (musicSource == null)
                return;

            duckedMusicSource = musicSource;
            duckedMusicSource.Fade(musicDuckVolume, musicDuckDuration);
        }

        private void RestoreMusic()
        {
            if (duckedMusicSource == null)
                return;

            if (duckedMusicSource.IsActive())
                duckedMusicSource.Fade(1f, musicDuckDuration);

            duckedMusicSource = null;
        }

        private bool IsWager => stakeRule != null && stakeRule.Type == MinigameStakeType.Wager && stakeRule.Stake.amount > 0;

        private void OnRectTransformDimensionsChange() => LayoutFrame();

        private void LayoutFrame()
        {
            if (panelRectTransform == null || contentRoot == null)
                return;

            var panelHeight = panelRectTransform.rect.height;

            if (panelHeight <= 0f)
                return;

            var scale = Mathf.Clamp(panelHeight / referencePanelHeight, minGapScale, 1f);

            var contentTop = 0f;

            if (topBar != null)
            {
                var barHeight = topBar.rect.height;
                var barTop = topBarTopGap * scale;

                topBar.offsetMax = new Vector2(topBar.offsetMax.x, -barTop);
                topBar.offsetMin = new Vector2(topBar.offsetMin.x, -barTop - barHeight);

                contentTop = MeasureUsedHeight(topBar) + contentTopGap * scale;
            }

            contentRoot.offsetMax = new Vector2(contentRoot.offsetMax.x, -contentTop);
            contentRoot.offsetMin = new Vector2(contentRoot.offsetMin.x, contentBottomGap * scale);
        }

        private float MeasureUsedHeight(RectTransform bar)
        {
            var lowest = float.MaxValue;

            for (var i = 0; i < bar.childCount; i++)
            {
                var child = (RectTransform)bar.GetChild(i);

                if (!child.gameObject.activeSelf)
                    continue;

                lowest = Mathf.Min(lowest, GetBottomInPanelSpace(child));
            }

            if (lowest >= float.MaxValue)
                lowest = GetBottomInPanelSpace(bar);

            return Mathf.Max(0f, panelRectTransform.rect.yMax - lowest);
        }

        private float GetBottomInPanelSpace(RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            return panelRectTransform.InverseTransformPoint(corners[0]).y;
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
                stakeText.text = IsWager
                    ? string.Format(wagerCaptionFormat, TraderResourceFormat.Format(stakeRule.Stake), TraderResourceFormat.Format(stakeRule.Prize))
                    : string.Format(prizeCaptionFormat, TraderResourceFormat.Format(stakeRule?.Prize));
            }

            if (wagerBadgeText != null)
            {
                wagerBadgeText.text = wagerBadgeCaption;
                wagerBadgeText.gameObject.SetActive(IsWager);
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

            activeView.Prepare(context);

            PlayIntro();
        }

        private void PlayIntro()
        {
            introSequence.Clear();

            if (topBarIntro != null)
            {
                topBarIntro.Hide();

                introSequence.Add(MinigameIntroStage.Frame, topBarIntro.Reveal);
            }

            activeView.BuildIntro(introSequence);

            introSequence.Play(introStepInterval, StartGame);
        }

        private void StartGame()
        {
            if (activeView != null)
                activeView.Run();
        }

        private void OnGameFinished(MinigameResult result)
        {
            introSequence.Stop();

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
                var payout = string.Empty;

                if (result.IsWin && stakeRule != null)
                    payout = TraderResourceFormat.Format(stakeRule.Prize);
                else if (IsWager)
                    payout = string.Format(lostStakeFormat, TraderResourceFormat.Format(stakeRule.Stake));

                resultRewardText.text = payout;
                resultRewardText.gameObject.SetActive(!string.IsNullOrEmpty(payout));
            }

            if (resultButtonText != null)
                resultButtonText.text = result.IsWin ? winButtonCaption : loseButtonCaption;

            PlayResultAppearance();

            AudioController.PlaySound(AudioController.GetClip(result.IsWin ? "reward" : "player_death_sound"), 0.7f);
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
