namespace Watermelon
{
    public enum TreasureHintKind
    {
        Band = 0,
        Warmer = 1,
        Colder = 2,
        Found = 3
    }

    public readonly struct TreasureHint
    {
        public readonly TreasureHintKind Kind;

        public readonly int BandIndex;

        public readonly int Distance;

        public bool IsTrend => Kind == TreasureHintKind.Warmer || Kind == TreasureHintKind.Colder;

        public TreasureHint(TreasureHintKind kind, int bandIndex, int distance)
        {
            Kind = kind;
            BandIndex = bandIndex;
            Distance = distance;
        }
    }
}
