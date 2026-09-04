namespace Watermelon
{
    public class ShellBoard
    {
        public const int MIN_SLOTS = 2;

        public int SlotCount { get; }

        public int PrizeSlot { get; private set; }

        public ShellBoard(int slotCount, int prizeSlot)
        {
            SlotCount = slotCount < MIN_SLOTS ? MIN_SLOTS : slotCount;

            PrizeSlot = prizeSlot < 0 || prizeSlot >= SlotCount ? 0 : prizeSlot;
        }

        public void Apply(ShellSwap swap)
        {
            if (swap.Contains(PrizeSlot))
                PrizeSlot = swap.Other(PrizeSlot);
        }

        public bool IsPrize(int slot) => slot == PrizeSlot;
    }
}
