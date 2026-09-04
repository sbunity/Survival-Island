using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ShellTableView : MonoBehaviour
    {
        [SerializeField] RectTransform tableRoot;
        [SerializeField] Image tableImage;
        [SerializeField] RectTransform shellsRoot;
        [SerializeField] ShellView shellPrefab;
        [SerializeField] Image prizeImage;

        [BoxGroup("Animation", "Animation")]
        [SerializeField, Min(0.01f)] float spawnDuration = 0.26f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0f)] float spawnStagger = 0.08f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type spawnEasing = Ease.Type.BackOut;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float liftDuration = 0.28f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type liftEasing = Ease.Type.CubicOut;
        [BoxGroup("Animation")]
        [SerializeField, Range(0f, 2f)] float liftHeight = 0.85f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type swapEasing = Ease.Type.SineInOut;
        [BoxGroup("Animation")]
        [SerializeField, Range(0f, 2f)] float swapArcHeight = 0.45f;
        [BoxGroup("Animation")]
        [SerializeField, Min(1f)] float tapPunchScale = 1.12f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float tapPunchDuration = 0.1f;

        [BoxGroup("Prize", "Prize")]
        [SerializeField, Range(-1f, 1f)] float prizeOffset = -0.12f;
        [BoxGroup("Prize")]
        [SerializeField, Min(0.01f)] float prizeAppearDuration = 0.2f;
        [BoxGroup("Prize")]
        [SerializeField] Ease.Type prizeAppearEasing = Ease.Type.BackOut;

        public event ShellTappedCallback ShellTapped;

        public bool IsInputEnabled
        {
            get => isInputEnabled;
            set
            {
                isInputEnabled = value;

                if (shells == null)
                    return;

                for (var i = 0; i < shells.Length; i++)
                {
                    if (shells[i] != null)
                        shells[i].IsInteractable = value;
                }
            }
        }

        public float LiftDuration => liftDuration;

        public float SpawnDuration => spawnDuration + spawnStagger * Mathf.Max(0, slotCount - 1);

        private ShellSettings settings;
        private MinigameGridLayout grid;

        private ShellView[] shells;
        private int slotCount;

        private Vector2 shellSize;

        private Vector2 shellsOffset;
        private bool hasShellsOffset;

        private bool isInputEnabled;

        private TweenCase prizeCase;

        public void Build(ShellSettings settings, int slotCount)
        {
            this.settings = settings;
            this.slotCount = Mathf.Max(ShellBoard.MIN_SLOTS, slotCount);

            if (tableImage != null)
                tableImage.sprite = settings.TableSprite;

            ClearShells();
            ApplyLayout();

            shells = new ShellView[this.slotCount];

            for (var slot = 0; slot < this.slotCount; slot++)
            {
                var shell = Instantiate(shellPrefab, shellsRoot);

                shell.gameObject.SetActive(false);
                shell.SetSprite(settings.ShellSprite);
                shell.Resize(shellSize);
                shell.PlaceAt(SlotPosition(slot));

                shell.Slot = slot;
                shell.IsInteractable = false;
                shell.Tapped += OnShellTapped;

                shells[slot] = shell;
            }

            HidePrize();
        }

        public void SetPrizeIcon(Sprite icon)
        {
            if (prizeImage != null)
                prizeImage.sprite = icon;
        }

        public void SpawnShells()
        {
            if (shells == null)
                return;

            for (var slot = 0; slot < shells.Length; slot++)
            {
                var shell = shells[slot];

                if (shell == null)
                    continue;

                shell.gameObject.SetActive(true);
                shell.AnimateSpawn(SlotPosition(slot), spawnDuration, spawnEasing, spawnStagger * slot);
            }
        }

        public void Lift(int slot)
        {
            var shell = GetShell(slot);

            if (shell != null)
                shell.Lift(shellSize.y * liftHeight, liftDuration, liftEasing);
        }

        public void LiftAll()
        {
            if (shells == null)
                return;

            for (var slot = 0; slot < shells.Length; slot++)
                Lift(slot);
        }

        public void DropAll()
        {
            if (shells == null)
                return;

            for (var slot = 0; slot < shells.Length; slot++)
            {
                var shell = shells[slot];

                if (shell != null)
                    shell.Drop(liftDuration, liftEasing);
            }
        }

        public void PlaySwap(ShellSwap swap, float duration)
        {
            var first = GetShell(swap.First);
            var second = GetShell(swap.Second);

            if (first == null || second == null)
                return;

            (shells[swap.First], shells[swap.Second]) = (second, first);

            first.Slot = swap.Second;
            second.Slot = swap.First;

            first.transform.SetAsLastSibling();

            var arc = shellSize.y * swapArcHeight;

            first.MoveTo(SlotPosition(swap.Second), duration, arc, swapEasing);
            second.MoveTo(SlotPosition(swap.First), duration, -arc, swapEasing);
        }

        public void ShowPrize(int slot)
        {
            if (prizeImage == null || prizeImage.sprite == null)
                return;

            prizeCase.KillActive();

            var rect = prizeImage.rectTransform;

            rect.anchoredPosition = SlotPosition(slot) + new Vector2(0f, shellSize.y * prizeOffset);
            rect.localScale = Vector3.zero;

            prizeImage.gameObject.SetActive(true);

            prizeCase = rect.DOScale(1f, prizeAppearDuration).SetEasing(prizeAppearEasing);
        }

        public void HidePrize()
        {
            prizeCase.KillActive();

            if (prizeImage != null)
                prizeImage.gameObject.SetActive(false);
        }

        public void StopAllAnimations()
        {
            prizeCase.KillActive();

            if (shells == null)
                return;

            for (var i = 0; i < shells.Length; i++)
            {
                if (shells[i] != null)
                    shells[i].KillTweens();
            }
        }

        public void ApplyLayout()
        {
            if (tableRoot == null || shellsRoot == null)
                return;

            CacheShellsOffset();

            var parent = tableRoot.parent as RectTransform;
            var available = parent != null ? parent.rect.size : tableRoot.rect.size;
            var aspect = MinigameGridLayout.GetAspect(tableImage != null ? tableImage.sprite : null);

            var slotsRect = settings != null ? settings.SlotsRect : new Rect(0f, 0f, 1f, 1f);
            var shellScale = settings != null ? settings.ShellScale : 1f;

            grid = new MinigameGridLayout(available, aspect, slotsRect, slotCount, 1, shellScale);

            tableRoot.sizeDelta = grid.FieldSize;

            shellsRoot.anchorMin = new Vector2(0.5f, 0.5f);
            shellsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            shellsRoot.pivot = new Vector2(0.5f, 0.5f);
            shellsRoot.sizeDelta = grid.FieldSize;

            shellsRoot.anchoredPosition = shellsOffset * TableFitScale(available, aspect);

            shellSize = ResolveShellSize();

            if (prizeImage != null)
            {
                var prizeScale = settings != null ? settings.PrizeScale : 0.5f;

                prizeImage.rectTransform.sizeDelta = Vector2.one * (grid.CellExtent * prizeScale);
            }
        }

        private void CacheShellsOffset()
        {
            if (hasShellsOffset || shellsRoot == null)
                return;

            hasShellsOffset = true;
            shellsOffset = shellsRoot.anchoredPosition;
        }

        private float TableFitScale(Vector2 available, float aspect)
        {
            if (aspect <= 0f)
                return 1f;

            var unclampedHeight = available.x / aspect;

            return unclampedHeight > 0f ? grid.FieldSize.y / unclampedHeight : 1f;
        }

        private Vector2 ResolveShellSize()
        {
            var width = grid.CellExtent;
            var aspect = MinigameGridLayout.GetAspect(settings != null ? settings.ShellSprite : null);

            return new Vector2(width, width / Mathf.Max(0.01f, aspect));
        }

        private Vector2 SlotPosition(int slot) => grid.CellToPosition(slot, 0);

        private ShellView GetShell(int slot)
        {
            if (shells == null || slot < 0 || slot >= shells.Length)
                return null;

            return shells[slot];
        }

        private void ClearShells()
        {
            if (shells == null)
                return;

            for (var i = 0; i < shells.Length; i++)
            {
                if (shells[i] == null)
                    continue;

                shells[i].Tapped -= OnShellTapped;

                Destroy(shells[i].gameObject);
            }

            shells = null;
        }

        private void OnShellTapped(ShellView shell)
        {
            if (!isInputEnabled)
                return;

            shell.PlayPunch(tapPunchScale, tapPunchDuration, Ease.Type.SineOut);

            ShellTapped?.Invoke(shell);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (settings == null || shells == null)
                return;

            for (var i = 0; i < shells.Length; i++)
            {
                if (shells[i] != null && shells[i].IsMoving)
                    return;
            }

            ApplyLayout();

            for (var slot = 0; slot < shells.Length; slot++)
            {
                var shell = shells[slot];

                if (shell == null)
                    continue;

                shell.Resize(shellSize);
                shell.PlaceAt(SlotPosition(slot));
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }
    }
}
