using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class Match3FieldInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] Match3FieldView field;
        [SerializeField, Min(1f)] float dragThreshold = 24f;

        public event SwapRequestedCallback SwapRequested;

        public bool IsEnabled { get; set; }

        private bool isPressed;
        private bool isDragHandled;

        private Vector2 pressPosition;
        private Vector2Int pressedCell;

        private bool hasSelection;
        private Vector2Int selectedCell;

        public void ResetState()
        {
            isPressed = false;
            isDragHandled = false;
            hasSelection = false;

            if (field != null)
                field.ClearSelection();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = false;
            isDragHandled = false;

            if (!IsEnabled || field == null)
                return;

            if (!TryResolveCell(eventData, out pressedCell))
                return;

            isPressed = true;
            pressPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsEnabled || !isPressed || isDragHandled)
                return;

            var delta = eventData.position - pressPosition;
            if (delta.magnitude < dragThreshold)
                return;

            isDragHandled = true;

            var direction = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? new Vector2Int(delta.x > 0f ? 1 : -1, 0)
                : new Vector2Int(0, delta.y > 0f ? -1 : 1);

            RequestSwap(pressedCell, pressedCell + direction);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsEnabled || !isPressed || isDragHandled)
            {
                isPressed = false;

                return;
            }

            isPressed = false;

            if (!hasSelection)
            {
                Select(pressedCell);

                return;
            }

            if (selectedCell == pressedCell)
            {
                ClearSelection();

                return;
            }

            if (Match3Board.AreAdjacent(selectedCell, pressedCell))
            {
                var from = selectedCell;

                ClearSelection();
                RequestSwap(from, pressedCell);

                return;
            }

            Select(pressedCell);
        }

        private void RequestSwap(Vector2Int from, Vector2Int to)
        {
            ClearSelection();

            if (to.x < 0 || to.x >= field.Columns || to.y < 0 || to.y >= field.Rows)
                return;

            SwapRequested?.Invoke(from, to);
        }

        private void Select(Vector2Int cell)
        {
            hasSelection = true;
            selectedCell = cell;

            field.SetSelected(cell);
        }

        private void ClearSelection()
        {
            hasSelection = false;

            field.ClearSelection();
        }

        private bool TryResolveCell(PointerEventData eventData, out Vector2Int cell)
        {
            cell = Vector2Int.zero;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(field.TilesRoot, eventData.position, eventData.pressEventCamera, out var localPoint))
                return false;

            return field.TryGetCell(localPoint, out cell);
        }
    }

    public delegate void SwapRequestedCallback(Vector2Int from, Vector2Int to);
}
