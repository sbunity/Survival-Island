using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ComicPlayer : MonoBehaviour
    {
        [SerializeField] List<ComicPage> pages = new List<ComicPage>();

        [Header("Controls")]
        [SerializeField] Button nextButton;
        [SerializeField] Button skipButton;
        [SerializeField] Button tapAreaButton;

        [Header("Pacing")]
        [SerializeField] float holdAfterTextBase = 0.8f;
        [SerializeField] float holdAfterTextPerCharacter = 0.045f;
        [SerializeField] float holdAfterSilentPanel = 1.2f;
        [SerializeField] float holdAfterPage = 0.8f;

        [Header("Page Transition")]
        [SerializeField] float pageTransitionDuration = 0.45f;
        [SerializeField] float pageTransitionOffset = 400f;

        private enum Step { None, PageTransition, PanelReveal, Bubble, Hold }

        private Step step = Step.None;
        private int pageIndex = -1;
        private int panelIndex = -1;
        private int bubbleIndex = -1;

        private TweenCase holdTweenCase;
        private SimpleCallback holdCallback;

        private SimpleCallback finishedCallback;
        private bool isPlaying;

        private ComicPage CurrentPage => pages[pageIndex];
        private ComicPanel CurrentPanel => CurrentPage.Panels[panelIndex];
        private ComicBubble CurrentBubble => CurrentPanel.Bubbles[bubbleIndex];

        private void Awake()
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            skipButton.onClick.AddListener(OnSkipButtonClicked);

            if (tapAreaButton != null)
                tapAreaButton.onClick.AddListener(Advance);

            foreach (ComicPage page in pages)
                page.Prepare();
        }

        public void Play(SimpleCallback onFinished)
        {
            if (isPlaying) return;

            isPlaying = true;
            finishedCallback = onFinished;

            pageIndex = -1;
            nextButton.interactable = true;
            skipButton.interactable = true;

            ShowNextPage();
        }

        public void Skip()
        {
            Finish();
        }

        private void ShowNextPage()
        {
            ComicPage previousPage = pageIndex >= 0 ? CurrentPage : null;

            pageIndex++;
            if (pageIndex >= pages.Count)
            {
                Finish();

                return;
            }

            panelIndex = -1;
            step = Step.PageTransition;

            if (previousPage != null)
                previousPage.Hide(pageTransitionDuration, pageTransitionOffset);

            CurrentPage.Show(pageTransitionDuration, pageTransitionOffset, ShowNextPanel);
        }

        private void ShowNextPanel()
        {
            panelIndex++;
            if (panelIndex >= CurrentPage.Panels.Count)
            {
                Hold(holdAfterPage, ShowNextPage);

                return;
            }

            bubbleIndex = -1;
            step = Step.PanelReveal;

            CurrentPanel.Reveal(ShowNextBubble);
        }

        private void ShowNextBubble()
        {
            bubbleIndex++;
            if (bubbleIndex >= CurrentPanel.Bubbles.Count)
            {
                if (CurrentPanel.Bubbles.Count == 0)
                {
                    Hold(holdAfterSilentPanel, ShowNextPanel);
                }
                else
                {
                    ShowNextPanel();
                }

                return;
            }

            step = Step.Bubble;

            ComicBubble bubble = CurrentBubble;
            bubble.Play(() =>
            {
                Hold(holdAfterTextBase + bubble.CharacterCount * holdAfterTextPerCharacter, ShowNextBubble);
            });
        }

        private void Hold(float duration, SimpleCallback onCompleted)
        {
            step = Step.Hold;
            holdCallback = onCompleted;
            holdTweenCase = Tween.DelayedCall(duration, EndHold, unscaledTime: true);
        }

        private void EndHold()
        {
            holdTweenCase.KillActive();

            SimpleCallback callback = holdCallback;
            holdCallback = null;
            callback?.Invoke();
        }

        private void Advance()
        {
            if (!isPlaying) return;

            switch (step)
            {
                case Step.PanelReveal:
                    CurrentPanel.Complete();
                    break;

                case Step.Bubble:
                    CurrentBubble.Complete();
                    break;

                case Step.Hold:
                    EndHold();
                    break;
            }
        }

        private void Finish()
        {
            if (!isPlaying) return;

            isPlaying = false;
            step = Step.None;

            holdTweenCase.KillActive();
            holdCallback = null;

            nextButton.interactable = false;
            skipButton.interactable = false;

            SimpleCallback callback = finishedCallback;
            finishedCallback = null;
            callback?.Invoke();
        }

        private void OnNextButtonClicked()
        {
            PlayButtonFeedback();

            Advance();
        }

        private void OnSkipButtonClicked()
        {
            PlayButtonFeedback();

            Skip();
        }

        private void PlayButtonFeedback()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));
        }

        private void OnDestroy()
        {
            holdTweenCase.KillActive();
        }
    }
}
