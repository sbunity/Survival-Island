using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class BuildingComplexBehavior : AbstractComplexBehavior<BuildingBehavior, PurchasePoint>, IGroundOpenable
    {
        private List<NavMeshObstacle> obstacles = new List<NavMeshObstacle>();

        private bool openingFromGroundStream;

        public override void Awake()
        {
            base.Awake();

            unlockable.SetComplex(this);
            GetComponentsInChildren(true, obstacles);
        }

        private void OnEnable()
        {
            if (openingFromGroundStream)
                return;

            if (!NavMeshController.IsNavMeshCalculated)
                return;

            RebuildNavMeshAndRefreshCarving();
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

        public override void Purchase()
        {
            base.Purchase();

            RebuildNavMeshAndRefreshCarving();
        }

        public override void Construct()
        {
            base.Construct();

            RebuildNavMeshAndRefreshCarving();
        }

        private void RebuildNavMeshAndRefreshCarving()
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] != null)
                    obstacles[i].carveOnlyStationary = true;
            }

            NavMeshController.CalculateNavMesh(() =>
            {
                for (int i = 0; i < obstacles.Count; i++)
                {
                    if (obstacles[i] != null)
                        obstacles[i].carveOnlyStationary = false;
                }
            });
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
