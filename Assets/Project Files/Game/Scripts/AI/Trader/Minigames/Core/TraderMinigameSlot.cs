using UnityEngine;

namespace Watermelon
{
    public class TraderMinigameSlot
    {
        private TraderMinigamesDatabase database;
        private TraderSave save;

        private TraderMinigameDefinition definition;
        public TraderMinigameDefinition Definition => definition;

        private IMinigameStakeRule stakeRule;
        public IMinigameStakeRule StakeRule => stakeRule;

        public event SimpleCallback Changed;

        public bool IsBusy { get; private set; }

        public bool HasGame => definition != null && stakeRule != null;

        public MinigameSlotState State => save != null ? (MinigameSlotState)save.MinigameState : MinigameSlotState.Available;

        public bool IsPlayable => HasGame && !IsBusy && State == MinigameSlotState.Available;

        public void Initialise(TraderMinigamesDatabase database, TraderSave save)
        {
            this.database = database;
            this.save = save;

            IsBusy = false;

            Resolve();
        }

        public void RollForVisit()
        {
            if (database == null || save == null)
                return;

            if (!string.IsNullOrEmpty(save.MinigameId))
            {
                Resolve();

                return;
            }

            var picked = database.GetRandom();
            if (picked == null)
                return;

            var stake = picked.RollStake();

            save.MinigameId = picked.ID;
            save.MinigameState = (int)MinigameSlotState.Available;
            save.MinigameSeed = Random.Range(int.MinValue, int.MaxValue);
            save.MinigameStakeCurrency = stake.currency;
            save.MinigameStakeAmount = stake.amount;
            save.MinigameReward = picked.RollReward(save.MinigameSeed);

            Resolve();

            Changed?.Invoke();
        }

        public void Clear()
        {
            IsBusy = false;
            definition = null;
            stakeRule = null;

            if (save != null)
            {
                save.MinigameId = string.Empty;
                save.MinigameState = (int)MinigameSlotState.Available;
                save.MinigameSeed = 0;
                save.MinigameStakeCurrency = default;
                save.MinigameStakeAmount = 0;
                save.MinigameReward = null;
            }

            Changed?.Invoke();
        }

        public bool TryBeginPlay(out MinigameContext context, out string blockReason)
        {
            context = null;
            blockReason = null;

            if (IsBusy)
            {
                blockReason = "Minigame is already running";

                return false;
            }

            if (!HasGame)
            {
                blockReason = "No minigame available";

                return false;
            }

            if (State != MinigameSlotState.Available)
            {
                blockReason = "Minigame is already played";

                return false;
            }

            if (!stakeRule.CanStart(out blockReason))
                return false;

            stakeRule.Charge();

            IsBusy = true;
            save.MinigameState = (int)MinigameSlotState.Lost;

            context = new MinigameContext(save.MinigameSeed, stakeRule.Stake, stakeRule.Prize);

            SaveController.MarkAsSaveIsRequired();

            return true;
        }

        public void CompletePlay(MinigameResult result)
        {
            if (!IsBusy)
                return;

            IsBusy = false;

            stakeRule.Payout(result);

            save.MinigameState = (int)(result.IsWin ? MinigameSlotState.Won : MinigameSlotState.Lost);

            SaveController.MarkAsSaveIsRequired();

            Changed?.Invoke();
        }

        public void Dispose()
        {
            IsBusy = false;
            Changed = null;

            definition = null;
            stakeRule = null;
            database = null;
            save = null;
        }

        private void Resolve()
        {
            definition = null;
            stakeRule = null;

            if (database == null || save == null || string.IsNullOrEmpty(save.MinigameId))
                return;

            definition = database.GetByID(save.MinigameId);

            if (definition == null)
            {
                save.MinigameId = string.Empty;

                return;
            }

            stakeRule = MinigameStakeRuleFactory.Create(definition, save.MinigameSeed, new Resource(save.MinigameStakeCurrency, save.MinigameStakeAmount), save.MinigameReward);
        }
    }
}
