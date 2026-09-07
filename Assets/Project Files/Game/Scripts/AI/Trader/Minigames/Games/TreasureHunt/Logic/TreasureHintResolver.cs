namespace Watermelon
{
    public class TreasureHintResolver
    {
        private const int NO_BAND = -1;

        private readonly int[] bandDistances;

        private int lastBand = NO_BAND;
        private int lastDistance = NO_BAND;

        public int BandCount => bandDistances != null ? bandDistances.Length : 0;

        public TreasureHintResolver(int[] bandDistances)
        {
            this.bandDistances = bandDistances;
        }

        public TreasureHint Resolve(int distance)
        {
            var band = GetBandIndex(distance);

            var kind = ResolveKind(band, distance);

            lastBand = band;
            lastDistance = distance;

            return new TreasureHint(kind, band, distance);
        }

        public int GetBandIndex(int distance)
        {
            if (BandCount == 0)
                return NO_BAND;

            for (var i = 0; i < bandDistances.Length; i++)
            {
                if (distance <= bandDistances[i])
                    return i;
            }

            return bandDistances.Length - 1;
        }

        public void Reset()
        {
            lastBand = NO_BAND;
            lastDistance = NO_BAND;
        }

        private TreasureHintKind ResolveKind(int band, int distance)
        {
            if (distance <= 0)
                return TreasureHintKind.Found;

            if (lastDistance < 0 || lastBand != band)
                return TreasureHintKind.Band;

            if (distance < lastDistance)
                return TreasureHintKind.Warmer;

            if (distance > lastDistance)
                return TreasureHintKind.Colder;

            return TreasureHintKind.Band;
        }
    }
}
