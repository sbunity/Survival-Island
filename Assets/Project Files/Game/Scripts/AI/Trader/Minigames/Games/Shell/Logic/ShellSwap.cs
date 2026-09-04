namespace Watermelon
{
    public readonly struct ShellSwap
    {
        public readonly int First;
        public readonly int Second;

        public ShellSwap(int first, int second)
        {
            First = first < second ? first : second;
            Second = first < second ? second : first;
        }

        public bool IsValid => First >= 0 && Second > First;

        public bool Contains(int slot) => First == slot || Second == slot;

        public int Other(int slot) => slot == First ? Second : First;

        public bool SamePair(ShellSwap other) => First == other.First && Second == other.Second;
    }
}
