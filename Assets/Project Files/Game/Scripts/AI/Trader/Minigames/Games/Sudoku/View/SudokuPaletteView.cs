using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public delegate void SudokuSymbolCallback(int symbol);

    public class SudokuPaletteView : MonoBehaviour
    {
        [SerializeField] RectTransform itemsRoot;
        [SerializeField] SudokuPaletteItem itemPrefab;
        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField, Range(0f, 1f)] float disabledAlpha = 0.6f;

        public event SudokuSymbolCallback SymbolSelected;

        private readonly List<SudokuPaletteItem> items = new();

        private bool isEnabled;

        public int SelectedSymbol { get; private set; } = SudokuBoard.EMPTY;

        public bool HasSelection => SelectedSymbol != SudokuBoard.EMPTY;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;

                if (canvasGroup == null)
                    return;

                canvasGroup.interactable = value;
                canvasGroup.blocksRaycasts = value;
                canvasGroup.alpha = value ? 1f : disabledAlpha;
            }
        }

        public void Build(Sprite[] icons)
        {
            Clear();

            if (icons == null || itemPrefab == null || itemsRoot == null)
                return;

            for (var symbol = 0; symbol < icons.Length; symbol++)
            {
                var item = Instantiate(itemPrefab, itemsRoot);

                item.gameObject.SetActive(true);
                item.Setup(symbol, icons[symbol], OnItemClicked);

                items.Add(item);
            }
        }

        public void SetRemaining(int symbol, int remaining)
        {
            var item = Find(symbol);

            if (item == null)
                return;

            item.SetRemaining(remaining);

            if (item.IsDepleted && SelectedSymbol == symbol)
                ClearSelection();
        }

        public void Select(int symbol)
        {
            var item = Find(symbol);

            if (item == null || item.IsDepleted)
                return;

            SelectedSymbol = symbol;

            for (var i = 0; i < items.Count; i++)
                items[i].SetSelected(items[i].Symbol == symbol);
        }

        public void ClearSelection()
        {
            SelectedSymbol = SudokuBoard.EMPTY;

            for (var i = 0; i < items.Count; i++)
                items[i].SetSelected(false);
        }

        public void Clear()
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                    Destroy(items[i].gameObject);
            }

            items.Clear();

            SelectedSymbol = SudokuBoard.EMPTY;
        }

        private SudokuPaletteItem Find(int symbol)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Symbol == symbol)
                    return items[i];
            }

            return null;
        }

        private void OnItemClicked(int symbol)
        {
            if (!isEnabled)
                return;

            SymbolSelected?.Invoke(symbol);
        }

        private void OnDestroy()
        {
            SymbolSelected = null;
        }
    }
}
