using UnityEngine;

namespace Watermelon
{
    public class TreasureHuntMinigameView : MinigameView
    {
        [SerializeField] TreasureFieldView field;
        [SerializeField] TreasureHuntHud hud;

        [BoxGroup("Intro", "Intro")]
        [SerializeField] MinigameIntroElement fieldIntro;
        [BoxGroup("Intro")]
        [SerializeField] MinigameIntroElement hudIntro;

        [BoxGroup("Timing", "Timing")]
        [SerializeField, Min(0f)] float winDelay = 1f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float revealDelay = 0.6f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float loseDelay = 1.2f;

        [BoxGroup("Audio", "Audio")]
        [SerializeField] AudioClip digSound;

        [BoxGroup("Audio")]
        [SerializeField] DuoFloat digPitchRange = new(1.3f, 0.8f);
        [BoxGroup("Audio")]
        [SerializeField] AudioClip rejectSound;
        [BoxGroup("Audio")]
        [SerializeField] AudioClip treasureSound;

        private TreasureHuntSettings settings;
        private TreasureHuntDifficulty difficulty;

        private TreasureHintBand[] bands;

        private TreasureBoard board;
        private TreasureHintResolver resolver;

        private TweenCase stepCase;
        private TweenCase audioCase;

        public void Configure(TreasureHuntSettings settings)
        {
            this.settings = settings;
        }

        protected override void OnPrepare(MinigameContext context)
        {
            if (settings == null || settings.Difficulties.IsNullOrEmpty())
            {
                Debug.LogError("[Treasure Hunt]: Settings are missing or the difficulty list is empty.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            difficulty = MinigameDifficultyPicker.Pick(settings.Difficulties, context.Seed);

            if (difficulty == null)
            {
                Debug.LogError("[Treasure Hunt]: The difficulty list holds no usable entry.", this);

                FinishGame(MinigameResult.Lose());

                return;
            }

            bands = difficulty.GetHintBands(settings.HintBands);

            if (bands.IsNullOrEmpty())
            {
                Debug.LogError("[Treasure Hunt]: No hint bands are set up, the trader has nothing to say.", this);

                bands = new[] { new TreasureHintBand() };
            }

            var treasure = TreasureBoard.RollTreasure(difficulty.Columns, difficulty.Rows, context.Random);

            board = new TreasureBoard(difficulty.Columns, difficulty.Rows, treasure, difficulty.Digs);
            resolver = new TreasureHintResolver(TreasureHintBand.ToDistances(bands));

            field.Build(settings, board.Columns, board.Rows);
            field.SetPrizeIcon(GetIcon(context.Prize));
            field.IsInputEnabled = false;

            hud.Setup(board.DigsLeft);

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

            sequence.Add(MinigameIntroStage.Pieces, field.SpawnCells);
        }

        protected override void OnRun()
        {
            if (board == null)
                return;

            field.CellTapped += OnCellTapped;

            Schedule(field.SpawnDuration, EnableInput);
        }

        protected override void OnStop()
        {
            stepCase.KillActive();
            audioCase.KillActive();

            if (field == null)
                return;

            field.CellTapped -= OnCellTapped;
            field.IsInputEnabled = false;
            field.StopAllAnimations();
        }

        private void EnableInput()
        {
            field.IsInputEnabled = true;
        }

        private void OnCellTapped(Vector2Int cell)
        {
            if (!IsRunning || board == null)
                return;

            if (!board.CanDig(cell))
            {
                field.PlayReject(cell);

                PlaySound(rejectSound);

                return;
            }

            board.Dig(cell);

            var distance = board.DistanceTo(cell, settings.DistanceMode);
            var hint = resolver.Resolve(distance);

            field.PlayDig(cell, GetBandColor(hint.BandIndex));

            PlaySound(digSound, GetDigPitch(hint.BandIndex));

            hud.SetDigsLeft(board.DigsLeft);

            if (board.IsFound)
            {
                field.IsInputEnabled = false;
                field.ShowPrize(cell);

                PlaySoundDelayed(treasureSound, field.DigDuration);

                hud.ShowFound();

                Schedule(winDelay, () => FinishGame(true));

                return;
            }

            hud.ShowHint(hint, GetBand(hint.BandIndex));

            if (!board.IsOutOfDigs)
                return;

            field.IsInputEnabled = false;

            Schedule(revealDelay, RevealTreasure);
        }

        private void RevealTreasure()
        {
            field.PlayDig(board.Treasure, GetBandColor(0));
            field.ShowPrize(board.Treasure);

            PlaySound(digSound, GetDigPitch(0));

            hud.ShowOutOfDigs();

            Schedule(loseDelay, () => FinishGame(false));
        }

        private TreasureHintBand GetBand(int index)
        {
            if (bands.IsNullOrEmpty() || index < 0 || index >= bands.Length)
                return null;

            return bands[index];
        }

        private Color GetBandColor(int index)
        {
            var band = GetBand(index);

            return band != null ? band.Color : Color.white;
        }

        private float GetDigPitch(int bandIndex)
        {
            var count = bands != null ? bands.Length : 0;

            if (count <= 1 || bandIndex < 0)
                return digPitchRange.firstValue;

            return digPitchRange.Lerp(Mathf.Clamp01((float)bandIndex / (count - 1)));
        }

        private static void PlaySound(AudioClip clip, float pitch = 1f)
        {
            if (clip == null)
                return;

            AudioController.PlaySound(clip, 1f, pitch);
        }

        private void PlaySoundDelayed(AudioClip clip, float delay)
        {
            if (clip == null)
                return;

            audioCase.KillActive();
            audioCase = Tween.DelayedCall(delay, () => AudioController.PlaySound(clip));
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
            audioCase.KillActive();

            base.OnDestroy();
        }
    }
}
