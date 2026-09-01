using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public delegate void SudokuCellCallback(Vector2Int cell);

    public class SudokuFieldInput : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] SudokuFieldView field;

        public event SudokuCellCallback CellTapped;

        public bool IsEnabled { get; set; }

        public void ResetState()
        {
            IsEnabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsEnabled || field == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(field.CellsRoot, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            if (field.TryGetCell(localPoint, out var cell))
                CellTapped?.Invoke(cell);
        }
    }
}
