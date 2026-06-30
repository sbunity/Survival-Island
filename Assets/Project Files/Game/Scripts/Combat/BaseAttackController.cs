using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DisallowMultipleComponent]
    public class BaseAttackController : MonoBehaviour
    {
        [SerializeField] Transform defensePoint;
        public Transform DefensePoint => defensePoint != null ? defensePoint : transform;

        [SerializeField, Min(0f)] float defenseRadius = 15f;
        public float DefenseRadius => defenseRadius;

        [SerializeField, Min(0f)] float alertCooldown = 5f;
        public float AlertCooldown => alertCooldown;

        public Vector3 DefensePosition => defensePoint != null
            ? defensePoint.position
            : worldBehavior != null ? worldBehavior.GetDefaultDefensePosition() : transform.position;

        public bool IsAlertActive { get; private set; }
        public IReadOnlyList<ICombatTarget> ActiveAttackers => activeAttackers;

        private readonly List<ICombatTarget> activeAttackers = new List<ICombatTarget>();
        private readonly List<BuildingBehavior> buildings = new List<BuildingBehavior>();
        private readonly List<HelperBehavior> helpers = new List<HelperBehavior>();

        private BaseWorldBehavior worldBehavior;
        private DefendBaseTask defendBaseTask;
        private float lastThreatTime;
        private bool isInitialised;

        public void Initialise(BaseWorldBehavior worldBehavior, IWorldElement[] worldElements, TaskHandler taskHandler)
        {
            if (isInitialised)
                Unload();

            this.worldBehavior = worldBehavior;

            buildings.Clear();
            helpers.Clear();
            activeAttackers.Clear();

            for (var i = 0; worldElements != null && i < worldElements.Length; i++)
            {
                if (worldElements[i] is BuildingBehavior building)
                {
                    buildings.Add(building);
                    building.Attacked += OnBuildingAttacked;
                }

                if (worldElements[i] is HelperBehavior helper)
                    helpers.Add(helper);
            }

            defendBaseTask = new DefendBaseTask(this);
            defendBaseTask.Register(taskHandler);

            IsAlertActive = false;
            isInitialised = true;
        }

        public void OnWorldLoaded()
        {
            if (!isInitialised)
                return;

            EndAlert();
            activeAttackers.Clear();
            lastThreatTime = Time.time;
        }

        public void Unload()
        {
            if (!isInitialised)
                return;

            EndAlert();

            for (var i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] != null)
                    buildings[i].Attacked -= OnBuildingAttacked;
            }

            defendBaseTask?.Destroy();
            defendBaseTask = null;

            activeAttackers.Clear();
            buildings.Clear();
            helpers.Clear();

            worldBehavior = null;
            isInitialised = false;
        }

        private void Update()
        {
            if (!isInitialised || !IsAlertActive)
                return;

            RemoveInvalidAttackers();
            AssignAvailableHelpers();

            if (HasHostilesInsideDefenseRadius())
            {
                lastThreatTime = Time.time;
                return;
            }

            if (Time.time >= lastThreatTime + alertCooldown)
                EndAlert();
        }

        private void OnBuildingAttacked(DamageSource source)
        {
            var attacker = source?.CharacterSource as ICombatTarget;
            if (!IsHostileAvailable(attacker))
                return;

            if (!activeAttackers.Contains(attacker))
                activeAttackers.Add(attacker);

            lastThreatTime = Time.time;

            if (!IsAlertActive)
                BeginAlert();
            else
                AssignAvailableHelpers();
        }

        private void BeginAlert()
        {
            IsAlertActive = true;
            defendBaseTask.Activate();

            worldBehavior.NotifyBaseUnderAttack();
            AssignAvailableHelpers();
        }

        private void EndAlert()
        {
            if (defendBaseTask != null && defendBaseTask.IsActive)
                defendBaseTask.Disable();

            IsAlertActive = false;
            activeAttackers.Clear();
        }

        private void AssignAvailableHelpers()
        {
            if (!IsAlertActive || defendBaseTask == null)
                return;

            for (var i = 0; i < helpers.Count; i++)
            {
                var helper = helpers[i];
                if (helper == null || !helper.isActiveAndEnabled || !helper.gameObject.activeInHierarchy ||
                    !helper.IsOpened || helper.IsDead || helper.IsRecovering)
                    continue;

                helper.TryStartBaseDefense(defendBaseTask);
            }
        }

        private void RemoveInvalidAttackers()
        {
            for (var i = activeAttackers.Count - 1; i >= 0; i--)
            {
                var attacker = activeAttackers[i];
                if (!IsHostileAvailable(attacker))
                    activeAttackers.RemoveAt(i);
            }
        }

        public ICombatTarget GetNearestHostile(Vector3 position)
        {
            CombatTargetRegistry.RemoveInvalidTargets();

            ICombatTarget nearestTarget = null;
            var nearestDistanceSqr = float.MaxValue;

            for (var i = 0; i < CombatTargetRegistry.Count; i++)
            {
                var target = CombatTargetRegistry.GetTarget(i);
                if (!IsHostileAvailable(target) || !IsInsideDefenseRadius(target.Transform.position))
                    continue;

                var attackPosition = target.GetAttackPosition(position);
                var distanceSqr = (attackPosition - position).sqrMagnitude;
                if (distanceSqr >= nearestDistanceSqr)
                    continue;

                nearestDistanceSqr = distanceSqr;
                nearestTarget = target;
            }

            return nearestTarget;
        }

        public bool IsInsideDefenseRadius(ICombatTarget target) 
            => IsHostileAvailable(target) && IsInsideDefenseRadius(target.Transform.position);

        public bool IsInsideDefenseRadius(Vector3 position)
        {
            var offset = position - DefensePosition;
            offset.y = 0f;
            return offset.sqrMagnitude <= defenseRadius * defenseRadius;
        }

        private bool HasHostilesInsideDefenseRadius()
        {
            CombatTargetRegistry.RemoveInvalidTargets();

            for (var i = 0; i < CombatTargetRegistry.Count; i++)
            {
                var target = CombatTargetRegistry.GetTarget(i);
                if (IsHostileAvailable(target) && IsInsideDefenseRadius(target.Transform.position))
                    return true;
            }

            return false;
        }

        private bool IsHostileAvailable(ICombatTarget target)
        {
            if (target == null || target.Faction != CombatFaction.Hostile || !target.CanBeTargeted || target.IsDead || target.Transform == null)
                return false;

            return target is not Object unityObject || unityObject != null;
        }

        private void OnDestroy()
        {
            Unload();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(defensePoint != null ? defensePoint.position : transform.position, defenseRadius);
        }
    }
}
