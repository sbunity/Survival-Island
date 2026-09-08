using System;
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

        [SerializeField, Min(0f)] float anchorMergeDistance = 7.5f;

        [SerializeField, Min(0f)] float alertCooldown = 5f;
        public float AlertCooldown => alertCooldown;

        public Vector3 DefensePosition => defensePoint != null
            ? defensePoint.position
            : worldBehavior != null ? worldBehavior.GetDefaultDefensePosition() : transform.position;

        public bool IsAlertActive { get; private set; }
        public IReadOnlyList<ICombatTarget> ActiveAttackers => activeAttackers;

        private readonly List<ICombatTarget> activeAttackers = new List<ICombatTarget>();
        private readonly AttackAnchorSet attackAnchors = new AttackAnchorSet();
        private readonly Dictionary<BuildingBehavior, Action<DamageSource>> buildingHandlers = new Dictionary<BuildingBehavior, Action<DamageSource>>();
        private readonly List<HelperBehavior> helpers = new List<HelperBehavior>();

        private BaseWorldBehavior worldBehavior;
        private DefendBaseTask defendBaseTask;
        private bool isInitialised;

        public void Initialise(BaseWorldBehavior worldBehavior, IWorldElement[] worldElements, TaskHandler taskHandler)
        {
            if (isInitialised)
                Unload();

            this.worldBehavior = worldBehavior;

            helpers.Clear();
            activeAttackers.Clear();
            attackAnchors.Clear();

            for (var i = 0; worldElements != null && i < worldElements.Length; i++)
            {
                if (worldElements[i] is BuildingBehavior building)
                    RegisterBuilding(building);

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
        }

        public void Unload()
        {
            if (!isInitialised)
                return;

            EndAlert();

            foreach (var pair in buildingHandlers)
            {
                if (pair.Key != null)
                    pair.Key.Attacked -= pair.Value;
            }

            defendBaseTask?.Destroy();
            defendBaseTask = null;

            buildingHandlers.Clear();
            activeAttackers.Clear();
            attackAnchors.Clear();
            helpers.Clear();

            worldBehavior = null;
            isInitialised = false;
        }

        private void Update()
        {
            if (!isInitialised || !IsAlertActive)
                return;

            RemoveInvalidAttackers();
            RefreshAnchors();
            AssignAvailableHelpers();

            if (attackAnchors.Count == 0)
                EndAlert();
        }

        private void RegisterBuilding(BuildingBehavior building)
        {
            if (building == null || buildingHandlers.ContainsKey(building))
                return;

            Action<DamageSource> handler = source => OnBuildingAttacked(building, source);

            building.Attacked += handler;
            buildingHandlers.Add(building, handler);
        }

        private void OnBuildingAttacked(BuildingBehavior building, DamageSource source)
        {
            var attacker = source?.CharacterSource as ICombatTarget;
            if (!IsHostileAvailable(attacker))
                return;

            if (!activeAttackers.Contains(attacker))
                activeAttackers.Add(attacker);

            attackAnchors.Register(building, attacker.Transform.position, anchorMergeDistance, Time.time);

            if (!IsAlertActive)
                BeginAlert();
            else
                AssignAvailableHelpers();
        }

        private void RefreshAnchors()
        {
            CombatTargetRegistry.RemoveInvalidTargets();

            var anchors = attackAnchors.Anchors;

            for (var i = 0; i < anchors.Count; i++)
            {
                if (HasHostileNear(anchors[i].Position))
                    anchors[i].KeepAlive(Time.time);
            }

            attackAnchors.RemoveExpired(Time.time, alertCooldown);
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

            var wasActive = IsAlertActive;

            IsAlertActive = false;
            activeAttackers.Clear();
            attackAnchors.Clear();

            if (wasActive && worldBehavior != null)
                worldBehavior.NotifyBaseAttackEnded();
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

        public Vector3 GetNearestDefensePosition(Vector3 from)
            => attackAnchors.GetNearest(from, DefensePosition);

        public bool IsInsideDefenseRadius(ICombatTarget target)
            => IsHostileAvailable(target) && IsInsideDefenseRadius(target.Transform.position);

        public bool IsInsideDefenseRadius(Vector3 position)
            => attackAnchors.IsInsideRadius(position, defenseRadius);

        public Vector3 ClampMovementInsideDefenseRadius(Vector3 position, float inset)
        {
            var center = GetNearestDefensePosition(position);
            return CombatSystemLogic.ClampInsideRadius(center, position, defenseRadius, inset);
        }

        private bool HasHostileNear(Vector3 position)
        {
            for (var i = 0; i < CombatTargetRegistry.Count; i++)
            {
                var target = CombatTargetRegistry.GetTarget(i);
                if (IsHostileAvailable(target) && IsInsideRadius(position, target.Transform.position))
                    return true;
            }

            return false;
        }

        private bool IsInsideRadius(Vector3 center, Vector3 position)
        {
            var offset = position - center;
            offset.y = 0f;
            return offset.sqrMagnitude <= defenseRadius * defenseRadius;
        }

        private bool IsHostileAvailable(ICombatTarget target)
        {
            if (target == null || target.Faction != CombatFaction.Hostile || !target.CanBeTargeted || target.IsDead || target.Transform == null)
                return false;

            return target is not UnityEngine.Object unityObject || unityObject != null;
        }

        private void OnDestroy()
        {
            Unload();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(defensePoint != null ? defensePoint.position : transform.position, defenseRadius);

            var anchors = attackAnchors.Anchors;

            Gizmos.color = Color.yellow;
            for (var i = 0; i < anchors.Count; i++)
            {
                Gizmos.DrawWireSphere(anchors[i].Position, defenseRadius);
                Gizmos.DrawSphere(anchors[i].Position, 0.35f);
            }
        }
    }
}
