using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class Match3MinigameView : MinigameView
    {
        [SerializeField] Match3FieldView field;
        [SerializeField] Match3FieldInput input;
        [SerializeField] Match3Hud hud;

        [BoxGroup("Intro", "Intro")]
        [SerializeField] MinigameIntroElement fieldIntro;
        [BoxGroup("Intro")]
        [SerializeField] MinigameIntroElement hudIntro;

        [BoxGroup("Timing", "Timing")]
        [SerializeField, Min(0f)] float finishDelay = 0.35f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0.5f)] float hintDelay = 4f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float shuffleDelay = 0.5f;

        private Match3Settings settings;
        private Match3Board board;
        private Match3Objective objective;

        private CurrencyType[] tileCurrencies;
        private int movesLeft;
        private bool isBusy;

        private float idleTime;
        private readonly List<Match3Move> moveBuffer = new List<Match3Move>();

        private TweenCase finishCase;
        private TweenCase shuffleCase;

        public void Configure(Match3Settings settings)
        {
            this.settings = settings;
        }

        protected override void OnPrepare(MinigameContext context)
        {
            if (settings == null || settings.TilePool.IsNullOrEmpty())
            {
                Debug.LogError("[Match3]: Settings are missing or the tile pool is empty.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            RollSession(context);

            board = new Match3Board(settings.Columns, settings.Rows, tileCurrencies.Length, context.Random);
            board.Fill();

            field.Build(BuildIcons(), settings);

            hud.Setup(GetIcon(tileCurrencies[objective.TileId]), 0, objective.Required, movesLeft);

            input.ResetState();
            input.IsEnabled = false;

            isBusy = true;
            idleTime = 0f;

            if (fieldIntro != null)
                fieldIntro.Hide();

            if (hudIntro != null)
                hudIntro.Hide();
        }

        protected override void OnBuildIntro(MinigameIntroSequence sequence)
        {
            if (board == null)
                return;

            if (fieldIntro != null)
                sequence.Add(MinigameIntroStage.Board, fieldIntro.Reveal);

            if (hudIntro != null)
                sequence.Add(MinigameIntroStage.Hud, hudIntro.Reveal);

            sequence.Add(MinigameIntroStage.Pieces, SpawnStartingTiles);
        }

        private void SpawnStartingTiles()
        {
            if (board != null)
                field.SpawnTiles(board);
        }

        protected override void OnRun()
        {
            if (board == null)
                return;

            input.ResetState();
            input.SwapRequested += OnSwapRequested;
            input.Interacted += OnPlayerInteracted;
            input.IsEnabled = true;

            isBusy = false;
            idleTime = 0f;
        }

        protected override void OnStop()
        {
            Lock();

            finishCase.KillActive();
            shuffleCase.KillActive();

            if (input != null)
            {
                input.SwapRequested -= OnSwapRequested;
                input.Interacted -= OnPlayerInteracted;
            }

            if (field != null)
                field.StopAllAnimations();
        }

        private void RollSession(MinigameContext context)
        {
            var pool = new List<CurrencyType>(settings.TilePool);

            var goal = !context.Prize.IsNullOrEmpty()
                ? context.Prize[0]
                : new Resource(context.Random.Pick(pool), 10);

            var chosen = context.Random.PickDistinct(pool, Mathf.Max(Match3Board.MIN_MATCH_LENGTH, settings.TileTypesPerGame), new[] { goal.currency });

            context.Random.Shuffle(chosen);

            tileCurrencies = chosen.ToArray();

            objective = new Match3Objective(chosen.IndexOf(goal.currency), goal.amount);
            movesLeft = Mathf.Max(1, context.Random.Range(settings.MovesRange));
        }

        private Sprite[] BuildIcons()
        {
            var icons = new Sprite[tileCurrencies.Length];

            for (var i = 0; i < tileCurrencies.Length; i++)
                icons[i] = GetIcon(tileCurrencies[i]);

            return icons;
        }

        private static Sprite GetIcon(CurrencyType currencyType)
        {
            var currency = CurrencyController.GetCurrency(currencyType);

            return currency?.Icon;
        }

        private void Update()
        {
            if (!IsRunning || isBusy || field == null || field.IsHintShown)
                return;

            idleTime += Time.deltaTime;

            if (idleTime >= hintDelay)
                ShowHint();
        }

        private void ShowHint()
        {
            idleTime = 0f;

            if (board == null || !board.CollectMoves(moveBuffer))
                return;

            var move = moveBuffer[Random.Range(0, moveBuffer.Count)];

            field.ShowHint(move.From, move.To);
        }

        private void HideHint()
        {
            idleTime = 0f;

            if (field != null)
                field.ClearHint();
        }

        private void OnPlayerInteracted()
        {
            HideHint();
        }

        private void OnSwapRequested(Vector2Int from, Vector2Int to)
        {
            if (!IsRunning || isBusy)
                return;

            var resolution = board.Swap(from, to);

            Lock();

            if (!resolution.IsValid)
            {
                field.PlayInvalidSwap(from, to, Unlock);

                return;
            }

            movesLeft--;
            hud.SetMoves(movesLeft);

            field.PlayResolution(resolution, OnStepResolved, OnResolutionFinished);
        }

        private void OnStepResolved(Match3Step step)
        {
            var previous = objective.Collected;

            objective.Report(step.Cleared);

            if (objective.Collected != previous)
                hud.SetProgress(objective.Collected, objective.Required);
        }

        private void OnResolutionFinished()
        {
            if (objective.IsComplete)
            {
                FinishDelayed(true);

                return;
            }

            if (movesLeft <= 0)
            {
                FinishDelayed(false);

                return;
            }

            if (!board.HasAvailableMove())
            {
                StartShuffle();

                return;
            }

            Unlock();
        }

        private void StartShuffle()
        {
            Lock();

            shuffleCase.KillActive();
            shuffleCase = Tween.DelayedCall(shuffleDelay, () =>
            {
                if (!IsRunning)
                    return;

                if (!board.Shuffle())
                    Debug.LogWarning("[Match3]: the tile mix cannot be arranged into a playable board.", this);

                field.PlayShuffle(board, Unlock);
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

            HideHint();

            if (input != null)
                input.IsEnabled = false;
        }

        private void Unlock()
        {
            if (!IsRunning)
                return;

            isBusy = false;
            idleTime = 0f;

            if (input != null)
                input.IsEnabled = true;
        }
    }
}
