using UnityEngine;

namespace Watermelon
{
    public class SudokuMinigameView : MinigameView
    {
        [SerializeField] SudokuFieldView field;
        [SerializeField] SudokuFieldInput input;
        [SerializeField] SudokuPaletteView palette;
        [SerializeField] SudokuHud hud;

        [BoxGroup("Intro", "Intro")]
        [SerializeField] MinigameIntroElement fieldIntro;
        [BoxGroup("Intro")]
        [SerializeField] MinigameIntroElement hudIntro;
        [BoxGroup("Intro")]
        [SerializeField] MinigameIntroElement paletteIntro;

        [BoxGroup("Timing", "Timing")]
        [SerializeField, Min(0f)] float finishDelay = 0.45f;

        private SudokuSettings settings;
        private SudokuDifficulty difficulty;
        private SudokuBoard board;

        private CurrencyType[] symbolCurrencies;

        private int livesLeft;
        private bool isBusy;

        private bool hasSelectedCell;
        private Vector2Int selectedCell;

        private TweenCase finishCase;

        public void Configure(SudokuSettings settings)
        {
            this.settings = settings;
        }

        protected override void OnPrepare(MinigameContext context)
        {
            if (settings == null || settings.SymbolPool.IsNullOrEmpty() || settings.Difficulties.IsNullOrEmpty())
            {
                Debug.LogError("[Sudoku]: Settings are missing, or the symbol pool or difficulty list is empty.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            difficulty = MinigameDifficultyPicker.Pick(settings.Difficulties, context.Seed);

            if (difficulty == null)
            {
                Debug.LogError("[Sudoku]: The difficulty list holds no usable entry.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            var rules = difficulty.BuildRules();
            var layout = rules.Layout;

            if (rules.IsEmpty)
            {
                Debug.LogError($"[Sudoku]: The \"{difficulty.Title}\" difficulty has no rules switched on.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            if (settings.SymbolPool.Length < layout.SymbolCount)
            {
                Debug.LogError($"[Sudoku]: The \"{difficulty.Title}\" difficulty needs {layout.SymbolCount} resources, but the symbol pool holds {settings.SymbolPool.Length}.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            board = SudokuGenerator.Generate(rules, difficulty.RollHoles(layout, context.Random), context.Random);

            if (board == null)
            {
                FinishGame(MinigameResult.Lose());

                return;
            }

            symbolCurrencies = context.Random.PickDistinct(settings.SymbolPool, layout.SymbolCount).ToArray();

            livesLeft = difficulty.Lives;

            var icons = BuildIcons();

            field.Build(icons, settings, layout);
            palette.Build(icons);

            for (var symbol = 0; symbol < layout.SymbolCount; symbol++)
                palette.SetRemaining(symbol, board.CountRemaining(symbol));

            hud.Setup(livesLeft, rules.Describe(), TraderResourceFormat.Format(context.Prize));

            input.ResetState();
            palette.IsEnabled = false;

            isBusy = true;

            if (fieldIntro != null)
                fieldIntro.Hide();

            if (hudIntro != null)
                hudIntro.Hide();

            if (paletteIntro != null)
                paletteIntro.Hide();
        }

        protected override void OnBuildIntro(MinigameIntroSequence sequence)
        {
            if (board == null)
                return;

            if (fieldIntro != null)
                sequence.Add(MinigameIntroStage.Board, fieldIntro.Reveal);

            if (hudIntro != null)
                sequence.Add(MinigameIntroStage.Hud, hudIntro.Reveal);

            sequence.Add(MinigameIntroStage.Pieces, SpawnStartingCells);

            if (paletteIntro != null)
                sequence.Add(MinigameIntroStage.Controls, paletteIntro.Reveal);
        }

        private void SpawnStartingCells()
        {
            if (board != null)
                field.SpawnCells(board);
        }

        protected override void OnRun()
        {
            if (board == null)
                return;

            input.CellTapped += OnCellTapped;
            palette.SymbolSelected += OnSymbolTapped;

            Unlock();
        }

        protected override void OnStop()
        {
            Lock();

            finishCase.KillActive();

            if (input != null)
                input.CellTapped -= OnCellTapped;

            if (palette != null)
                palette.SymbolSelected -= OnSymbolTapped;

            if (field != null)
                field.StopAllAnimations();
        }

        private Sprite[] BuildIcons()
        {
            var icons = new Sprite[symbolCurrencies.Length];

            for (var i = 0; i < symbolCurrencies.Length; i++)
                icons[i] = GetIcon(symbolCurrencies[i]);

            return icons;
        }

        private static Sprite GetIcon(CurrencyType currencyType)
        {
            var currency = CurrencyController.GetCurrency(currencyType);

            return currency?.Icon;
        }

        private void OnCellTapped(Vector2Int cell)
        {
            if (!IsRunning || isBusy)
                return;

            if (!board.IsEmpty(cell))
            {
                field.PlayReject(cell);

                return;
            }

            if (palette.HasSelection)
            {
                Commit(cell, palette.SelectedSymbol);

                return;
            }

            SelectCell(cell);
        }

        private void OnSymbolTapped(int symbol)
        {
            if (!IsRunning || isBusy)
                return;

            if (hasSelectedCell)
            {
                var cell = selectedCell;

                ClearCellSelection();
                Commit(cell, symbol);

                return;
            }

            palette.Toggle(symbol);
            field.SetHighlightedSymbol(palette.SelectedSymbol);
        }

        private void SelectCell(Vector2Int cell)
        {
            hasSelectedCell = true;
            selectedCell = cell;

            field.SetSelectedCell(cell);
        }

        private void ClearCellSelection()
        {
            if (!hasSelectedCell)
                return;

            hasSelectedCell = false;

            field.ClearSelectedCell();
        }

        private void Commit(Vector2Int cell, int symbol)
        {
            switch (board.Place(cell, symbol))
            {
                case SudokuPlacement.Rejected:
                    field.PlayReject(cell);
                    return;

                case SudokuPlacement.Correct:
                    OnSymbolPlaced(cell, symbol);
                    return;

                default:
                    OnSymbolRefused(cell, symbol);
                    return;
            }
        }

        private void OnSymbolPlaced(Vector2Int cell, int symbol)
        {
            field.PlayPlace(cell, symbol);

            palette.SetRemaining(symbol, board.CountRemaining(symbol));
            field.SetHighlightedSymbol(palette.SelectedSymbol);

            if (board.IsSolved)
                FinishDelayed(true);
        }

        private void OnSymbolRefused(Vector2Int cell, int symbol)
        {
            livesLeft--;

            hud.SetLives(livesLeft);

            var hasConflict = board.TryGetConflict(cell, symbol, out var conflict);

            Lock();

            field.PlayMistake(cell, symbol, hasConflict, conflict, () =>
            {
                if (livesLeft <= 0)
                {
                    FinishDelayed(false);

                    return;
                }

                Unlock();
            });
        }

        private void FinishDelayed(bool isWin)
        {
            Lock();

            finishCase.KillActive();
            finishCase = Tween.DelayedCall(finishDelay, () => FinishGame(isWin));
        }

        private void Lock()
        {
            isBusy = true;

            if (input != null)
                input.IsEnabled = false;

            if (palette != null)
                palette.IsEnabled = false;
        }

        private void Unlock()
        {
            if (!IsRunning)
                return;

            isBusy = false;

            input.IsEnabled = true;
            palette.IsEnabled = true;
        }
    }
}
