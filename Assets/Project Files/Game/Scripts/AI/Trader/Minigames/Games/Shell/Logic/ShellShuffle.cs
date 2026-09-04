using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class ShellShuffle
    {
        private const int REROLL_ATTEMPTS = 8;

        public static List<ShellSwap> Build(int slotCount, int swapCount, int prizeSlot, System.Random random)
        {
            var swaps = new List<ShellSwap>(Mathf.Max(0, swapCount));

            if (slotCount < ShellBoard.MIN_SLOTS || swapCount <= 0 || random == null)
                return swaps;

            var previous = default(ShellSwap);
            var prize = prizeSlot;
            var hasMovedPrize = false;

            for (var i = 0; i < swapCount; i++)
            {
                var isLast = i == swapCount - 1;

                var swap = isLast && !hasMovedPrize
                    ? RollInvolving(slotCount, prize, random)
                    : Roll(slotCount, previous, random);

                if (swap.Contains(prize))
                {
                    prize = swap.Other(prize);
                    hasMovedPrize = true;
                }

                swaps.Add(swap);

                previous = swap;
            }

            return swaps;
        }

        private static ShellSwap Roll(int slotCount, ShellSwap previous, System.Random random)
        {
            var swap = RollAny(slotCount, random);

            for (var i = 0; i < REROLL_ATTEMPTS && previous.IsValid && swap.SamePair(previous); i++)
                swap = RollAny(slotCount, random);

            return swap;
        }

        private static ShellSwap RollInvolving(int slotCount, int slot, System.Random random)
        {
            var other = random.Next(0, slotCount - 1);

            if (other >= slot)
                other++;

            return new ShellSwap(slot, other);
        }

        private static ShellSwap RollAny(int slotCount, System.Random random)
        {
            return RollInvolving(slotCount, random.Next(0, slotCount), random);
        }
    }
}
