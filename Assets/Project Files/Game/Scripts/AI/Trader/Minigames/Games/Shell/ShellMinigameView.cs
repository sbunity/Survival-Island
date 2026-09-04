using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ShellMinigameView : MinigameView
    {
        [SerializeField] ShellTableView table;
        [SerializeField] ShellHud hud;

        [BoxGroup("Intro", "Intro")]
        [SerializeField] MinigameIntroElement tableIntro;
        [BoxGroup("Intro")]
        [SerializeField] MinigameIntroElement hudIntro;

        [BoxGroup("Timing", "Timing")]
        [SerializeField, Min(0f)] float revealHold = 1.1f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float shuffleDelay = 0.4f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float missHold = 0.7f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float finishDelay = 0.9f;

        private ShellSettings settings;
        private ShellDifficulty difficulty;

        private ShellBoard board;
        private List<ShellSwap> swaps;
        private int swapIndex;

        private TweenCase stepCase;

        public void Configure(ShellSettings settings)
        {
            this.settings = settings;
        }

        protected override void OnPrepare(MinigameContext context)
        {
            if (settings == null || settings.Difficulties.IsNullOrEmpty())
            {
                Debug.LogError("[Shell Game]: Settings are missing or the difficulty list is empty.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            difficulty = MinigameDifficultyPicker.Pick(settings.Difficulties, context.Seed);

            if (difficulty == null)
            {
                Debug.LogError("[Shell Game]: The difficulty list holds no usable entry.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            var slotCount = difficulty.ShellCount;

            board = new ShellBoard(slotCount, context.Random.Next(0, slotCount));
            swaps = ShellShuffle.Build(slotCount, difficulty.RollSwapCount(context.Random), board.PrizeSlot, context.Random);
            swapIndex = 0;

            table.Build(settings, slotCount);
            table.SetPrizeIcon(GetIcon(context.Prize));
            table.IsInputEnabled = false;

            hud.SetPhase(ShellPhase.Watch);

            if (tableIntro != null)
                tableIntro.Hide();

            if (hudIntro != null)
                hudIntro.Hide();
        }

        protected override void OnBuildIntro(MinigameIntroSequence sequence)
        {
            if (board == null)
                return;

            if (tableIntro != null)
                sequence.Add(MinigameIntroStage.Board, tableIntro.Reveal);

            if (hudIntro != null)
                sequence.Add(MinigameIntroStage.Hud, hudIntro.Reveal);

            sequence.Add(MinigameIntroStage.Pieces, table.SpawnShells);
        }

        protected override void OnRun()
        {
            if (board == null)
                return;

            table.ShellTapped += OnShellTapped;

            Schedule(table.SpawnDuration, RevealPrize);
        }

        protected override void OnStop()
        {
            stepCase.KillActive();

            if (table == null)
                return;

            table.ShellTapped -= OnShellTapped;
            table.IsInputEnabled = false;
            table.StopAllAnimations();
        }

        private void RevealPrize()
        {
            hud.SetPhase(ShellPhase.Watch);

            table.LiftAll();
            table.ShowPrize(board.PrizeSlot);

            Schedule(table.LiftDuration + revealHold, CoverPrize);
        }

        private void CoverPrize()
        {
            table.HidePrize();
            table.DropAll();

            Schedule(table.LiftDuration + shuffleDelay, StartShuffle);
        }

        private void StartShuffle()
        {
            hud.SetPhase(ShellPhase.Shuffle);

            PlayNextSwap();
        }

        private void PlayNextSwap()
        {
            if (swaps == null || swapIndex >= swaps.Count)
            {
                AskForPick();

                return;
            }

            var swap = swaps[swapIndex++];

            board.Apply(swap);
            table.PlaySwap(swap, difficulty.SwapDuration);

            Schedule(difficulty.SwapDuration + difficulty.SwapInterval, PlayNextSwap);
        }

        private void AskForPick()
        {
            hud.SetPhase(ShellPhase.Pick);

            table.IsInputEnabled = true;
        }

        private void OnShellTapped(ShellView shell)
        {
            if (!IsRunning || shell == null)
                return;

            table.IsInputEnabled = false;

            var isWin = board.IsPrize(shell.Slot);

            hud.SetPhase(isWin ? ShellPhase.Won : ShellPhase.Lost);

            table.Lift(shell.Slot);

            if (isWin)
            {
                table.ShowPrize(board.PrizeSlot);

                Schedule(table.LiftDuration + finishDelay, () => FinishGame(true));

                return;
            }

            Schedule(table.LiftDuration + missHold, ShowMissedShell);
        }

        private void ShowMissedShell()
        {
            table.Lift(board.PrizeSlot);
            table.ShowPrize(board.PrizeSlot);

            Schedule(table.LiftDuration + finishDelay, () => FinishGame(false));
        }

        private void Schedule(float delay, SimpleCallback callback)
        {
            stepCase.KillActive();
            stepCase = Tween.DelayedCall(delay, callback);
        }

        private static Sprite GetIcon(Resource[] prize)
        {
            if (prize.IsNullOrEmpty())
                return null;

            var currency = CurrencyController.GetCurrency(prize[0].currency);

            return currency?.Icon;
        }

        protected override void OnDestroy()
        {
            stepCase.KillActive();

            base.OnDestroy();
        }
    }
}
