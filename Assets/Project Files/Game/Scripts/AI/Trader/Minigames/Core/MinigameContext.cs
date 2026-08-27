namespace Watermelon
{
    public class MinigameContext
    {
        public int Seed { get; }

        public System.Random Random { get; }

        public Resource Stake { get; }

        public Resource[] Prize { get; }

        public bool HasStake => Stake.amount > 0;

        public MinigameContext(int seed, Resource stake, Resource[] prize)
        {
            Seed = seed;
            Random = new System.Random(seed);
            Stake = stake;
            Prize = prize;
        }
    }
}
