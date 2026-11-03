using UnityEngine;

namespace Watermelon
{
    public class PirateEnemyBehavior : SkeletonEnemyBehavior
    {
        private static readonly int IS_SWIMMING_HASH = Animator.StringToHash("IsSwimming");
        private static readonly int SWIMMING_STATE_HASH = Animator.StringToHash("Swimming");
        private static readonly int MOVEMENT_STATE_HASH = Animator.StringToHash("Movement");

        private const float APPROACH_TIME_FACTOR = 2f;
        private const float APPROACH_TIME_MARGIN = 5f;

        [BoxFoldout("Model", label: "Model")]
        [SerializeField] Transform modelHolder;
        [BoxFoldout("Model")]
        [SerializeField] RuntimeAnimatorController animatorController;

        [BoxFoldout("Approach", label: "Sea Approach")]
        [SerializeField, Range(0.1f, 1f)] float swimSpeedMultiplier = 0.7f;
        [BoxFoldout("Approach")]
        [SerializeField, Min(0.1f)] float landingDistance = 0.75f;

        public bool IsApproaching { get; private set; }
        public bool IsSwimming { get; private set; }

        protected override bool EntersCombatOnSpawn => false;

        private EnemyModelSlot modelSlot;
        private WaterSurfaceProbe waterProbe;
        private NavStallDetector approachStallDetector;

        private int landAreaMask;
        private float landSpeed;
        private float approachDeadline;

        protected override void Awake()
        {
            base.Awake();

            landAreaMask = Agent.areaMask;
            landSpeed = Agent.speed;

            modelSlot = new EnemyModelSlot(modelHolder != null ? modelHolder : transform, this, animatorController);
        }

        public void SetModel(PlayerGraphics skin)
        {
            var model = modelSlot.Wear(skin);

            if (model != null)
                BindGraphics(model.Animator, model.BodyRenderer);
        }

        public void Spawn(Vector3 seaPosition, Transform landingPoint)
        {
            Agent.areaMask = landAreaMask | NavMeshAreas.Water;

            Spawn(seaPosition, RaidPointLine.FlatLookRotation(seaPosition, landingPoint.position, transform.rotation), landingPoint);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            waterProbe.Reset(transform.position, Agent.areaMask);
            ApplySwimming(waterProbe.IsInWater);

            var model = modelSlot.Current;
            if (model != null)
                model.Animator.Play(IsSwimming ? SWIMMING_STATE_HASH : MOVEMENT_STATE_HASH, 0, 0f);

            BeginApproach();
        }

        protected override void Update()
        {
            base.Update();

            if (!IsApproaching)
                return;

            if (waterProbe.Tick(transform.position, Agent.areaMask, Time.deltaTime))
                ApplySwimming(waterProbe.IsInWater);

            TickApproach();
        }

        private void BeginApproach()
        {
            IsApproaching = true;

            var distance = Vector3.Distance(transform.position, SpawnPoint.position);
            approachDeadline = Time.time + distance / (landSpeed * swimSpeedMultiplier) * APPROACH_TIME_FACTOR + APPROACH_TIME_MARGIN;

            Agent.stoppingDistance = 0f;

            if (!Agent.isOnNavMesh || !Agent.SetDestination(SpawnPoint.position))
                RescueToLandingPoint();
        }

        private void TickApproach()
        {
            if (!Agent.isOnNavMesh)
            {
                RescueToLandingPoint();
                return;
            }

            if (Agent.pathPending)
                return;

            if (!IsSwimming)
            {
                if (Agent.remainingDistance <= landingDistance || approachStallDetector.Tick(Agent) != NavStallVerdict.Moving)
                    Land();

                return;
            }

            if (Time.time >= approachDeadline)
                RescueToLandingPoint();
        }

        private void RescueToLandingPoint()
        {
            Debug.LogWarning($"[Pirate Enemy] {name} could not swim to its landing point. Check that the raid's Sea Spawn Line reaches the Landing Line over water.", this);

            Agent.Warp(SpawnPoint.position);

            Land();
        }

        private void Land()
        {
            IsApproaching = false;

            ApplySwimming(false);

            Agent.areaMask = landAreaMask;
            StopMoving();

            EnterCombat();
            stateMachine.StartMachine();
        }

        private void ApplySwimming(bool value)
        {
            IsSwimming = value;

            approachStallDetector.Reset();
            Agent.speed = value ? landSpeed * swimSpeedMultiplier : landSpeed;

            var model = modelSlot.Current;
            if (model == null)
                return;

            model.Animator.SetBool(IS_SWIMMING_HASH, value);

            model.SetWaterTrailStatus(value);
            model.SetDustParticleStatus(!value);
            model.SetSubmersionValue(value ? 1f : 0f);
        }

        protected override void OnReturnedToPool()
        {
            base.OnReturnedToPool();

            IsApproaching = false;
        }
    }
}
