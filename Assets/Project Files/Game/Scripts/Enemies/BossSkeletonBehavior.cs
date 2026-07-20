using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class BossSkeletonBehavior : SkeletonEnemyBehavior, IWorldElement
    {
        [BoxFoldout("Boss", label: "Boss")]
        [SerializeField] Transform bossSpawnPoint;

        [BoxFoldout("Boss")]
        [SerializeField, Min(1f)] float leashRadius = 12f;

        [BoxFoldout("Boss")]
        [SerializeField] string uniqueSaveID = "cave_boss_skeleton";

        public int InitialisationOrder => 100;
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public event SimpleCallback Defeated;

        public bool IsDefeated
        {
            get
            {
                if (save != null)
                    return save.IsDead;

                var worldSave = WorldController.CurrentWorld != null
                    ? SaveController.GetFile(WorldController.CurrentWorld.ID)
                    : null;

                return worldSave != null && worldSave.GetSaveObject<BossSkeletonSave>(uniqueSaveID).IsDead;
            }
        }

        private HealthBehavior healthBehavior;
        private BossSkeletonSave save;

        private bool isBossActive;
        private bool isReturningHome;
        private Vector3 homePosition;

        protected override void Awake()
        {
            base.Awake();

            healthBehavior = GetComponent<HealthBehavior>();
        }

        public void OnWorldLoaded()
        {
            InitialiseFromSave();
        }

        public void OnWorldUnloaded()
        {
            isBossActive = false;

            if (healthBehavior != null)
                healthBehavior.Damaged -= OnDamaged;
        }

        private void InitialiseFromSave()
        {
            var worldSave = LinkedWorldBehavior != null
                ? LinkedWorldBehavior.WorldSave
                : (WorldController.CurrentWorld != null ? SaveController.GetFile(WorldController.CurrentWorld.ID) : null);

            if (worldSave == null)
            {
                Debug.LogError($"[Boss Skeleton] World save is not available for '{name}'.", this);
                return;
            }

            save = worldSave.GetSaveObject<BossSkeletonSave>(uniqueSaveID);

            if (save.IsDead)
            {
                isBossActive = false;
                gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            var spawnTransform = bossSpawnPoint != null ? bossSpawnPoint : transform;
            homePosition = spawnTransform.position;

            Spawn(spawnTransform);

            if (save.IsInitialised)
                healthBehavior.SetCurrentHealth(save.CurrentHealth);
            else
                save.Initialise(healthBehavior.MaxHealth);

            healthBehavior.Damaged -= OnDamaged;
            healthBehavior.Damaged += OnDamaged;

            isReturningHome = false;
            isBossActive = true;
        }

        private void OnDamaged()
        {
            if (save == null)
                return;

            save.CurrentHealth = healthBehavior.CurrentHealth;
            SaveController.MarkAsSaveIsRequired();
        }

        protected override void OnDeathStarted()
        {
            base.OnDeathStarted();

            isBossActive = false;

            if (healthBehavior != null)
                healthBehavior.Damaged -= OnDamaged;

            if (save != null)
            {
                save.CurrentHealth = 0f;
                save.IsDead = true;
                SaveController.MarkAsSaveIsRequired();
            }

            Defeated?.Invoke();
        }

        private void LateUpdate()
        {
            if (!isBossActive || IsDead || !Agent.isActiveAndEnabled)
                return;

            if (!Agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(homePosition, out NavMeshHit navHit, 4f, Agent.areaMask))
                    Agent.Warp(navHit.position);

                return;
            }

            var distanceSqr = (transform.position - homePosition).sqrMagnitude;
            var leashSqr = leashRadius * leashRadius;
            var releaseSqr = (leashRadius * 0.5f) * (leashRadius * 0.5f);

            if (distanceSqr > leashSqr)
                isReturningHome = true;
            else if (isReturningHome && distanceSqr < releaseSqr)
                isReturningHome = false;

            if (isReturningHome)
            {
                ClearForcedTarget();
                Agent.stoppingDistance = 0f;
                Agent.SetDestination(homePosition);
            }
        }
    }
}
