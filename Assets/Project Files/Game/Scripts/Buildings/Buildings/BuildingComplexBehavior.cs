namespace Watermelon
{
    public class BuildingComplexBehavior : AbstractComplexBehavior<BuildingBehavior, PurchasePoint>, IGroundOpenable
    {
        private bool openingFromGroundStream;

        public override void Awake()
        {
            base.Awake();

            unlockable.SetComplex(this);
        }

        private void OnEnable()
        {
            if (openingFromGroundStream)
                return;

            if (!NavMeshController.IsNavMeshCalculated)
                return;

            NavMeshController.CalculateNavMesh();
        }

        public override void Init()
        {
            if (unlockable.IsDestroyed)
            {
                unlockable.SpawnDestroyed();
                InitialiseReconstruction(false);
                InvokeInitialiseCallback();
                return;
            }

            base.Init();
        }

        public void BeginReconstruction()
        {
            if (!unlockable.IsDestroyed)
                return;

            InitialiseReconstruction(true);
        }

        public void OnGroundOpen(bool immediately = false)
        {
            openingFromGroundStream = true;
            gameObject.SetActive(true);
            openingFromGroundStream = false;

            Init();
        }

        public void OnGroundHidden(bool immediately = false)
        {
            gameObject.SetActive(false);
        }
    }
}
