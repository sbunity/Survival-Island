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

        [SerializeField, Min(0f)] float alertCooldown = 5f;
        public float AlertCooldown => alertCooldown;

        public Vector3 DefensePosition => defensePoint != null
            ? defensePoint.position
            : worldBehavior != null ? worldBehavior.GetDefaultDefensePosition() : transform.position;

        public bool IsAlertActive { get; private set; }
        public IReadOnlyList<ICombatTarget> ActiveAttackers => activeAttackers;

        private readonly List<ICombatTarget> activeAttackers = new List<ICombatTarget>();
        private readonly List<AttackAnchor> attackAnchors = new List<AttackAnchor>();
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

            RegisterAnchor(building);

            if (!IsAlertActive)
                BeginAlert();
            else
                AssignAvailableHelpers();
        }

        private void RegisterAnchor(BuildingBehavior building)
        {
            if (building == null)
                return;

            for (var i = 0; i < attackAnchors.Count; i++)
            {
                if (attackAnchors[i].Building != building)
                    continue;

                attackAnchors[i].Position = building.Transform.position;
                attackAnchors[i].LastThreatTime = Time.time;
                return;
            }

            attackAnchors.Add(new AttackAnchor
            {
                Building = building,
                Position = building.Transform.position,
                LastThreatTime = Time.time,
            });
        }

        private void RefreshAnchors()
        {
            CombatTargetRegistry.RemoveInvalidTargets();

            for (var i = attackAnchors.Count - 1; i >= 0; i--)
            {
                var anchor = attackAnchors[i];

                if (anchor.Building != null)
                    anchor.Position = anchor.Building.Transform.position;

                if (HasHostileNear(anchor.Position))
                    anchor.LastThreatTime = Time.time;

                if (Time.time >= anchor.LastThreatTime + alertCooldown)
                    attackAnchors.RemoveAt(i);
            }
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
        {
            var nearest = DefensePosition;
            var nearestDistanceSqr = float.MaxValue;

            for (var i = 0; i < attackAnchors.Count; i++)
            {
                var anchorPosition = attackAnchors[i].Position;

                var offset = anchorPosition - from;
                offset.y = 0f;

                var distanceSqr = offset.sqrMagnitude;
                if (distanceSqr >= nearestDistanceSqr)
                    continue;

                nearestDistanceSqr = distanceSqr;
                nearest = anchorPosition;
            }

            return nearest;
        }

        public bool IsInsideDefenseRadius(ICombatTarget target)
            => IsHostileAvailable(target) && IsInsideDefenseRadius(target.Transform.position);

        public bool IsInsideDefenseRadius(Vector3 position)
        {
            for (var i = 0; i < attackAnchors.Count; i++)
            {
                if (IsInsideRadius(attackAnchors[i].Position, position))
                    return true;
            }

            return false;
        }

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
        }

        private class AttackAnchor
        {
            public BuildingBehavior Building;
            public Vector3 Position;
            public float LastThreatTime;
        }
    }
}
