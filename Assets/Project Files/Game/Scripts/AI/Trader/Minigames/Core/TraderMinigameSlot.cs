using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The one minigame a trader offers during a single visit: which game it is, what is on the line and whether it has
    /// already been played. Owns no UI and no movement logic — it is composed into <see cref="WanderingTraderBehavior"/>
    /// the same way <see cref="RescueAreaGate"/> is, so the behaviour itself stays untouched apart from three hooks.
    ///
    /// <para>Lifecycle: <see cref="Initialise"/> on world load → <see cref="RollForVisit"/> when the trader docks →
    /// <see cref="TryBeginPlay"/> / <see cref="CompletePlay"/> per session → <see cref="Clear"/> when he sails home.</para>
    /// </summary>
    public class TraderMinigameSlot
    {
        private TraderMinigamesDatabase database;
        private TraderSave save;

        private TraderMinigameDefinition definition;
        public TraderMinigameDefinition Definition => definition;

        private IMinigameStakeRule stakeRule;
        public IMinigameStakeRule StakeRule => stakeRule;

        /// <summary>Raised whenever the slot content or state changes, so the trader window can redraw.</summary>
        public event SimpleCallback Changed;

        /// <summary>True while a session is running. The trader must not sail away and the visit timer must not tick.</summary>
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

        /// <summary>
        /// Picks the game for this visit, rolls its seed and — for wager games — the resource and amount at stake.
        /// Does nothing if the visit already has a game, so restoring a saved visit keeps the same offer.
        /// </summary>
        public void RollForVisit()
        {
            if (database == null || save == null)
                return;

            if (!string.IsNullOrEmpty(save.MinigameId))
            {
                Resolve();

                return;
            }

            TraderMinigameDefinition picked = database.GetRandom();
            if (picked == null)
                return;

            Resource stake = picked.RollStake();

            save.MinigameId = picked.ID;
            save.MinigameState = (int)MinigameSlotState.Available;
            save.MinigameSeed = Random.Range(int.MinValue, int.MaxValue);
            save.MinigameStakeCurrency = stake.currency;
            save.MinigameStakeAmount = stake.amount;

            Resolve();

            Changed?.Invoke();
        }

        /// <summary>Wipes the slot. Called when the trader leaves so the next visit rolls a fresh game.</summary>
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
            }

            Changed?.Invoke();
        }

        /// <summary>
        /// Validates and pays for a session. On success the stake is already taken and the slot is marked as lost —
        /// winning flips it back in <see cref="CompletePlay"/>. That makes "quit mid-game" and "kill the app mid-game"
        /// behave identically: the stake burns.
        /// </summary>
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

            // Deliberately no Changed here: the trader window is being covered by the host page right now, and rebuilding
            // its items would destroy the very button that is handling this click. CompletePlay refreshes it instead.

            return true;
        }

        /// <summary>Settles the session: pays out on a win and releases the trader.</summary>
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
                // The game was removed from the database after the visit was saved — drop the slot for this visit.
                save.MinigameId = string.Empty;

                return;
            }

            stakeRule = MinigameStakeRuleFactory.Create(definition, new Resource(save.MinigameStakeCurrency, save.MinigameStakeAmount));
        }
    }
}
