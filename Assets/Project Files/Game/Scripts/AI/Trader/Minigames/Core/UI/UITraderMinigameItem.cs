using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UITraderMinigameItem : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text descriptionText;

        [Space]
        [SerializeField] GameObject wagerBadge;
        [SerializeField] TMP_Text wagerNoticeText;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] TMP_Text buttonText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField, Range(0f, 1f)] float disabledAlpha = 0.6f;

        [BoxGroup("Captions", "Captions")]
        [SerializeField] string stakeCaptionFormat = "Bet {0}";
        [BoxGroup("Captions")]
        [SerializeField] string prizeCaptionFormat = "Win {0}";
        [BoxGroup("Captions")]
        [SerializeField] string wagerNoticeFormat = "Bet game: win {1}";
        [BoxGroup("Captions")]
        [SerializeField] string wonCaption = "Won";
        [BoxGroup("Captions")]
        [SerializeField] string lostCaption = "Lost";

        private UITrader page;
        private TraderMinigameSlot slot;

        public void Init(UITrader page, TraderMinigameSlot slot)
        {
            this.page = page;
            this.slot = slot;

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);

            Build();
            Redraw();
        }

        private void Build()
        {
            var definition = slot.Definition;
            if (definition == null)
                return;

            if (iconImage != null)
            {
                iconImage.sprite = definition.Icon;
                iconImage.gameObject.SetActive(definition.Icon != null);
            }

            if (titleText != null)
                titleText.text = definition.Title;

            if (descriptionText != null)
                descriptionText.text = definition.Description;
        }

        public void Redraw()
        {
            var rule = slot.StakeRule;

            if (buttonText != null && rule != null)
            {
                buttonText.text = rule.Stake.amount > 0
                    ? string.Format(stakeCaptionFormat, TraderResourceFormat.Format(rule.Stake))
                    : string.Format(prizeCaptionFormat, TraderResourceFormat.Format(rule.Prize));
            }

            var isWager = rule != null && rule.Type == MinigameStakeType.Wager;

            if (wagerNoticeText != null)
            {
                wagerNoticeText.text = isWager
                    ? string.Format(wagerNoticeFormat, TraderResourceFormat.Format(rule.Stake), TraderResourceFormat.Format(rule.Prize))
                    : string.Empty;
            }

            if (wagerBadge != null)
                wagerBadge.SetActive(isWager);

            var isPlayed = slot.State != MinigameSlotState.Available;

            if (isPlayed && buttonText != null)
                buttonText.text = slot.State == MinigameSlotState.Won ? wonCaption : lostCaption;

            var canStart = rule != null && rule.CanStart(out _);

            playButton.interactable = slot.IsPlayable && canStart;

            if (canvasGroup != null)
                canvasGroup.alpha = playButton.interactable ? 1f : disabledAlpha;
        }

        private void OnPlayClicked()
        {
            if (!slot.IsPlayable)
                return;

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            page.PlayMinigame();
        }
    }
}
