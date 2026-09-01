using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SudokuPaletteItem : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image frameImage;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text countText;
        [SerializeField] CanvasGroup canvasGroup;

        [BoxGroup("Selection", "Selection")]
        [SerializeField] Color normalColor = Color.white;
        [BoxGroup("Selection")]
        [SerializeField] Color selectedColor = new Color(1f, 0.85f, 0.35f, 1f);
        [BoxGroup("Selection")]
        [SerializeField, Min(1f)] float selectedScale = 1.12f;
        [BoxGroup("Selection")]
        [SerializeField, Min(0.01f)] float selectionDuration = 0.12f;

        [BoxGroup("Depleted", "Depleted")]
        [SerializeField, Range(0f, 1f)] float depletedAlpha = 0.35f;

        private SudokuSymbolCallback clickedCallback;

        private TweenCase scaleCase;

        public int Symbol { get; private set; } = SudokuBoard.EMPTY;

        public int Remaining { get; private set; }

        public bool IsDepleted => Remaining <= 0;

        public void Setup(int symbol, Sprite icon, SudokuSymbolCallback onClicked)
        {
            Symbol = symbol;

            clickedCallback = onClicked;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (frameImage != null)
                frameImage.color = normalColor;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }

            SetSelected(false, true);
            SetRemaining(0);
        }

        public void SetRemaining(int remaining)
        {
            Remaining = Mathf.Max(0, remaining);

            if (countText != null)
                countText.text = Remaining.ToString();

            if (canvasGroup != null)
                canvasGroup.alpha = IsDepleted ? depletedAlpha : 1f;

            if (button != null)
                button.interactable = !IsDepleted;
        }

        public void SetSelected(bool isSelected, bool instant = false)
        {
            if (frameImage != null)
                frameImage.color = isSelected ? selectedColor : normalColor;

            var target = isSelected ? selectedScale : 1f;

            scaleCase.KillActive();

            if (instant)
            {
                transform.localScale = Vector3.one * target;

                return;
            }

            scaleCase = transform.DOScale(target, selectionDuration).SetEasing(Ease.Type.SineOut);
        }

        private void OnButtonClicked()
        {
            clickedCallback?.Invoke(Symbol);
        }

        private void OnDisable()
        {
            scaleCase.KillActive();
        }
    }
}
