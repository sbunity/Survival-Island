using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.AI
{
    public class HelperStateMachine : AbstractStateMachine<HelperStateMachine.State>
    {
        private const float TASK_UPDATE_DELAY = 3.0f;

        private HelperBehavior helperBehavior;
        private NavMeshAgentBehaviour navMeshAgentBehaviour;

        private float taskWaitingDelay;

        public void Initialise(HelperBehavior helperBehavior, NavMeshAgentBehaviour navMeshAgentBehaviour)
        {
            this.helperBehavior = helperBehavior;
            this.navMeshAgentBehaviour = navMeshAgentBehaviour;

            var waitingForTaskStateCase = new StateCase
            {
                state = new WaitingForTaskState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(HostileNearbyTransition, transitionType: StateTransitionType.Independent),
                    new(WaitForTaskStateTransition, transitionType: StateTransitionType.Independent),
                }
            };

            var gatheringStateCase = new StateCase
            {
                state = new GatheringState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(HostileNearbyTransition, transitionType: StateTransitionType.Independent),
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var storingStateCase = new StateCase
            {
                state = new StoringState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(HostileNearbyTransition, transitionType: StateTransitionType.Independent),
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var buildingStateCase = new StateCase
            {
                state = new BuildingState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(HostileNearbyTransition, transitionType: StateTransitionType.Independent),
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var converterStoringStateCase = new StateCase
            {
                state = new ConverterStoringState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(HostileNearbyTransition, transitionType: StateTransitionType.Independent),
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var fishingStateCase = new StateCase
            {
                state = new FishingState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(HostileNearbyTransition, transitionType: StateTransitionType.Independent),
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var attackingStateCase = new StateCase
            {
                state = new AttackingState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var recoveringAtBaseStateCase = new StateCase
            {
                state = new RecoveringAtBaseState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(RecoveryFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            var defendingBaseStateCase = new StateCase
            {
                state = new DefendingBaseState(helperBehavior),
                transitions = new List<StateTransition<State>>
                {
                    new(TaskFinish, transitionType: StateTransitionType.OnFinish),
                }
            };

            states.Add(State.WaitingForTask, waitingForTaskStateCase);
            states.Add(State.Gathering, gatheringStateCase);
            states.Add(State.Storing, storingStateCase);
            states.Add(State.Building, buildingStateCase);
            states.Add(State.ConverterStoring, converterStoringStateCase);
            states.Add(State.Fishing, fishingStateCase);
            states.Add(State.RecoveringAtBase, recoveringAtBaseStateCase);
            states.Add(State.DefendingBase, defendingBaseStateCase);
            states.Add(State.Attacking, attackingStateCase);

            startState = State.WaitingForTask;
        }

        public void StartMachine(State initialState)
        {
            startState = initialState;

            StartMachine();
        }

        private bool WaitForTaskStateTransition(out State nextState)
        {
            if (helperBehavior.ActiveTask != null && helperBehavior.ActiveTask.IsActive && helperBehavior.ActiveTask.Validate(helperBehavior))
            {
                if (helperBehavior.ActiveTask.GetStateMachineState(out nextState))
                {
                    return true;
                }
            }

            if (Time.time > taskWaitingDelay)
            {
                taskWaitingDelay = Time.time + TASK_UPDATE_DELAY;

                if(!EnergyController.IsEnergySystemEnabled || EnergyController.EnergyPoints > 0)
                {
                    BaseTask task = helperBehavior.FindAvailableTask();
                    if (task != null)
                    {
                        helperBehavior.SetActiveTask(task);

                        if (task.GetStateMachineState(out nextState))
                        {
                            return true;
                        }
                    }
                }
            }

            nextState = State.WaitingForTask;

            return false;
        }

        private bool HostileNearbyTransition(out State nextState)
        {
            nextState = State.Attacking;

            return CanEngageHostiles() && helperBehavior.HasHostileInAggroRange();
        }

        private bool CanEngageHostiles()
        {
            return helperBehavior != null && helperBehavior.IsOpened && !helperBehavior.IsDead && !helperBehavior.IsRecovering;
        }

        private bool TaskFinish(out State nextState)
        {
            nextState = State.WaitingForTask;

            return true;
        }

        private bool RecoveryFinish(out State nextState)
        {
            nextState = State.WaitingForTask;

            return !helperBehavior.IsRecovering;
        }

        public enum State
        {
            WaitingForTask = 0,
            Idle = 1,
            Gathering = 2,
            Storing = 3,
            Building = 4,
            ConverterStoring = 5,
            Fishing = 6,
            RecoveringAtBase = 7,
            DefendingBase = 8,
            Attacking = 9,
        }
    }

    public class RecoveringAtBaseState : HelperStateBehavior
    {
        public RecoveringAtBaseState(HelperBehavior helperBehavior) : base(helperBehavior)
        {
        }

        protected override void OnEnter()
        {
            navMeshAgent.Stop();
            target.StopSnapping();
            target.Graphics.InteractionAnimations.Disable();
            target.ShowRecoveryHealthbar();
        }

        protected override void OnTick()
        {
            if (target.UpdateRecovery(Time.deltaTime))
                InvokeOnFinished();
        }
    }

    public class DefendingBaseState : HelperStateBehavior
    {
        private const float TARGET_MOVEMENT_REFRESH_DELAY = 0.2f;
        private const float DEFENSE_POINT_REACH_DISTANCE = 0.25f;

        private DefendBaseTask defendTask;
        private BaseAttackController controller;
        private float nextMovementRefreshTime;

        public DefendingBaseState(HelperBehavior helperBehavior) : base(helperBehavior)
        {
        }

        protected override void OnEnter()
        {
            defendTask = target.ActiveTask as DefendBaseTask;
            controller = defendTask?.Controller;
            nextMovementRefreshTime = Time.time;

            navMeshAgent.Stop();
            target.StopSnapping();
            target.ClearCombatTarget();
        }

        protected override void OnTick()
        {
            if (defendTask == null || controller == null || !defendTask.Validate(target))
            {
                InvokeOnFinished();
                return;
            }

            var combatTarget = target.CombatTarget;
            if (!target.IsCombatTargetValid(combatTarget) || !controller.IsInsideDefenseRadius(combatTarget))
            {
                navMeshAgent.Stop();
                target.ClearCombatTarget();
                combatTarget = null;
            }

            if (combatTarget == null)
            {
                combatTarget = controller.GetNearestHostile(target.transform.position);
                if (combatTarget != null && !target.SetCombatTarget(combatTarget))
                    combatTarget = null;
            }

            if (combatTarget != null)
            {
                if (target.CanAttack(combatTarget))
                {
                    target.TryAttack();
                }
                else if (Time.time >= nextMovementRefreshTime)
                {
                    nextMovementRefreshTime = Time.time + TARGET_MOVEMENT_REFRESH_DELAY;

                    var attackPosition = combatTarget.GetAttackPosition(target.transform.position);
                    var movementPosition = controller.ClampMovementInsideDefenseRadius(attackPosition, target.CombatRange);
                    target.MoveToCombatPosition(movementPosition);
                }

                return;
            }

            HoldDefensePoint();
        }

        private void HoldDefensePoint()
        {
            var defensePosition = controller.GetNearestDefensePosition(target.transform.position);
            var offset = defensePosition - target.transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= DEFENSE_POINT_REACH_DISTANCE * DEFENSE_POINT_REACH_DISTANCE)
            {
                navMeshAgent.Stop();
                return;
            }

            if (!navMeshAgent.IsMoving && navMeshAgent.PathExists(defensePosition))
                navMeshAgent.SetWaypoints(defensePosition);
        }

        protected override void OnExit()
        {
            navMeshAgent.Stop();
            target.ClearCombatTarget();
            target.UnlinkActiveTask();

            defendTask = null;
            controller = null;
        }
    }

    public class AttackingState : HelperStateBehavior
    {
        private const float MOVEMENT_REFRESH_DELAY = 0.2f;

        private Vector3 leashOrigin;
        private float nextMovementRefreshTime;

        public AttackingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {
        }

        protected override void OnEnter()
        {
            leashOrigin = target.transform.position;
            nextMovementRefreshTime = Time.time;

            navMeshAgent.Stop();
            target.StopSnapping();

            AcquireTarget();
        }

        protected override void OnTick()
        {
            if (target.IsDead || target.IsRecovering)
            {
                InvokeOnFinished();
                return;
            }

            var combatTarget = target.CombatTarget;
            if (!target.IsCombatTargetValid(combatTarget) || !IsInsideLeash(combatTarget))
            {
                target.ClearCombatTarget();
                combatTarget = AcquireTarget();
            }

            if (combatTarget == null)
            {
                InvokeOnFinished();
                return;
            }

            if (target.CanAttack(combatTarget))
            {
                target.TryAttack();
            }
            else if (Time.time >= nextMovementRefreshTime)
            {
                nextMovementRefreshTime = Time.time + MOVEMENT_REFRESH_DELAY;

                if (!target.MoveToCombatTarget())
                    target.ClearCombatTarget();
            }
        }

        private ICombatTarget AcquireTarget()
        {
            var hostile = target.FindNearestHostile(leashOrigin, target.AggroRadius);
            if (hostile != null && target.SetCombatTarget(hostile))
                return hostile;

            target.ClearCombatTarget();
            return null;
        }

        private bool IsInsideLeash(ICombatTarget combatTarget)
        {
            if (combatTarget == null || combatTarget.Transform == null)
                return false;

            var offset = combatTarget.Transform.position - leashOrigin;
            offset.y = 0f;

            return offset.sqrMagnitude <= target.AggroRadius * target.AggroRadius;
        }

        protected override void OnExit()
        {
            navMeshAgent.Stop();
            target.ClearCombatTarget();
        }
    }

    public class WaitingForTaskState : HelperStateBehavior
    {
        private bool isSitting;

        public WaitingForTaskState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        protected override void OnEnter()
        {
            isSitting = false;

            if (target != null)
            {
                navMeshAgent.MovementStalled += OnMovementStalled;

                MoveToRestPosition();
            }
        }

        private void MoveToRestPosition()
        {
            navMeshAgent.Stop();
            target.StopSnapping();

            var restPosition = target.GetRestPosition();

            if (!navMeshAgent.TrySnapToNavMesh(restPosition, out restPosition))
            {
                Rest();

                return;
            }

            if (!navMeshAgent.SetWaypoints(restPosition))
            {
                Rest();

                return;
            }

            navMeshAgent.PathFinished += RestPositionReached;
        }

        private void OnMovementStalled()
        {
            if (isSitting)
                return;

            Rest();
        }

        private void RestPositionReached()
        {
            Rest();
        }

        private void Rest()
        {
            if (isSitting)
                return;

            isSitting = true;

            target.ActivateSittingAnimation();
        }

        protected override void OnTick()
        {
            if(isSitting)
            {
                if (EnergyController.IsEnergySystemEnabled && EnergyController.EnergyPoints == 0)
                {
                    target.EmoteBehavior.Show(SimpleEmoteBehavior.EmoteType.Hunger);
                }
                else
                {
                    target.EmoteBehavior.Show(SimpleEmoteBehavior.EmoteType.StorageIsFull);
                }
            }
        }

        protected override void OnExit()
        {
            isSitting = false;

            navMeshAgent.MovementStalled -= OnMovementStalled;

            target.DisableSittingAnimation();

            target.EmoteBehavior.Hide();
        }
    }

    public class FishingState : HelperStateBehavior
    {
        private const float INTERACTION_DISTANCE = 2f;

        private FishingPlaceBehavior fishingPlace;

        private bool isGathering;
        private float lastHealth;

        protected override float MaxDuration => HelperStateTimeouts.GATHERING;

        public FishingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        protected override void OnEnter()
        {
            isGathering = false;

            navMeshAgent.Stop();
            target.StopSnapping();

            if (target.ActiveTask is not FishingTask fishingTask)
            {
                InvokeOnFinished();

                return;
            }

            fishingPlace = fishingTask.FishingPlaceBehavior;
            lastHealth = fishingPlace.Health;

            target.SetTargetHitableObject(fishingPlace);

            if (!navMeshAgent.MoveToTarget(fishingPlace.transform.position, fishingTask.OffsetRadius))
            {
                OnWatchdogTimeout();

                return;
            }

            navMeshAgent.PathFinished += OnResourceReached;
        }

        private void OnResourceReached()
        {
            if (fishingPlace.Health > 0 && Vector3.Distance(target.transform.position, fishingPlace.transform.position) <= INTERACTION_DISTANCE)
            {
                isGathering = true;

                navMeshAgent.Stop();

                KeepAlive();

                fishingPlace.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
            }
            else
            {
                InvokeOnFinished();
            }
        }

        protected override void OnTick()
        {
            if (navMeshAgent.IsMoving)
                KeepAlive();

            if (fishingPlace != null && fishingPlace.Health <= 0)
            {
                InvokeOnFinished();

                return;
            }

            if (!isGathering && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();

                return;
            }

            if (isGathering)
            {
                if (fishingPlace.Health < lastHealth)
                {
                    lastHealth = fishingPlace.Health;

                    KeepAlive();
                }

                target.SnapToHittable(fishingPlace);
            }
        }

        protected override void OnExit()
        {
            target.Graphics.InteractionAnimations.Disable();

            target.StopSnapping();

            navMeshAgent.Stop();

            isGathering = false;

            target.UnlinkActiveTask();
        }
    }

    public class GatheringState : HelperStateBehavior
    {
        private const float INTERACTION_DISTANCE = 2f;

        private ResourceSourceBehavior targetResource;

        private bool isGathering;
        private float lastHealth;

        protected override float MaxDuration => HelperStateTimeouts.GATHERING;

        public GatheringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        protected override void OnEnter()
        {
            isGathering = false;

            navMeshAgent.Stop();
            target.StopSnapping();

            if (target.ActiveTask is not GatheringTask gatheringTask)
            {
                InvokeOnFinished();

                return;
            }

            targetResource = gatheringTask.ResourceSource;
            lastHealth = targetResource.Health;

            target.SetTargetHitableObject(targetResource);

            if (!navMeshAgent.MoveToTarget(targetResource.transform.position, gatheringTask.OffsetRadius))
            {
                OnWatchdogTimeout();

                return;
            }

            navMeshAgent.PathFinished += OnResourceReached;
        }

        private void OnResourceReached()
        {
            if(targetResource.Health > 0 && Vector3.Distance(target.transform.position, targetResource.transform.position) <= INTERACTION_DISTANCE)
            {
                isGathering = true;

                navMeshAgent.Stop();

                KeepAlive();

                targetResource.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
            }
            else
            {
                InvokeOnFinished();
            }
        }

        protected override void OnTick()
        {
            if (navMeshAgent.IsMoving)
                KeepAlive();

            if (targetResource != null && targetResource.Health <= 0)
            {
                InvokeOnFinished();

                return;
            }

            if(!isGathering && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();

                return;
            }

            if(isGathering)
            {
                if (targetResource.Health < lastHealth)
                {
                    lastHealth = targetResource.Health;

                    KeepAlive();
                }

                target.SnapToHittable(targetResource);
            }
        }

        protected override void OnExit()
        {
            target.Graphics.InteractionAnimations.Disable();

            target.StopSnapping();

            navMeshAgent.Stop();

            isGathering = false;

            target.UnlinkActiveTask();
        }
    }

    public class StoringState : HelperStateBehavior
    {
        private ResourceStorageBuildingBehavior targetStorage;

        private bool storageReached;
        private float lastDeliveryTime;

        protected override float MaxDuration => HelperStateTimeouts.STORING;

        public StoringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        protected override void OnEnter()
        {
            navMeshAgent.Stop();
            target.StopSnapping();

            if (target.ActiveTask is not StoreResourcesTask storeResourcesTask)
            {
                InvokeOnFinished();

                return;
            }

            targetStorage = storeResourcesTask.StorageBuildingBehavior;

            storageReached = false;
            lastDeliveryTime = target.LastTimeResourceGiven;

            if (!MoveToTakingPoint(targetStorage.Storage.ResourceTakingPoint.transform.position))
            {
                OnWatchdogTimeout();

                return;
            }

            navMeshAgent.PathFinished += OnStorageReached;
        }

        private bool MoveToTakingPoint(Vector3 takingPointPosition)
        {
            var randomOffset = UnityEngine.Random.insideUnitSphere;
            randomOffset.y = 0;

            if (navMeshAgent.TrySnapToNavMesh(takingPointPosition + randomOffset, out Vector3 destination))
                return navMeshAgent.SetWaypoints(destination);

            return navMeshAgent.SetWaypoints(takingPointPosition);
        }

        private void OnStorageReached()
        {
            storageReached = true;

            KeepAlive();
        }

        protected override void OnTick()
        {
            if (navMeshAgent.IsMoving)
                KeepAlive();

            if (target.ActiveTask == null || !target.ActiveTask.IsActive || !targetStorage.IsOperational)
            {
                InvokeOnFinished();
                return;
            }

            if(targetStorage.IsFull)
            {
                InvokeOnFinished();

                return;
            }

            if(!target.Inventory.HasResource(targetStorage.StoredResources))
            {
                InvokeOnFinished();

                return;
            }

            if (target.LastTimeResourceGiven > lastDeliveryTime)
            {
                lastDeliveryTime = target.LastTimeResourceGiven;

                KeepAlive();
            }

            if(!storageReached && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }
        }

        protected override void OnExit()
        {
            navMeshAgent.Stop();

            target.UnlinkActiveTask();
        }
    }

    public class ConverterStoringState : HelperStateBehavior
    {
        private ResourceConverterBuildingBehavior targetStorage;

        private bool storageReached;
        private float lastDeliveryTime;

        protected override float MaxDuration => HelperStateTimeouts.STORING;

        public ConverterStoringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        protected override void OnEnter()
        {
            navMeshAgent.Stop();
            target.StopSnapping();

            if (target.ActiveTask is not ConverterStoringTask converterStoringTask)
            {
                InvokeOnFinished();

                return;
            }

            targetStorage = converterStoringTask.ResourceConverter;

            storageReached = false;
            lastDeliveryTime = target.LastTimeResourceGiven;

            if (!MoveToTakingPoint(targetStorage.InStorage.ResourceTakingPoint.transform.position))
            {
                OnWatchdogTimeout();

                return;
            }

            navMeshAgent.PathFinished += OnStorageReached;
        }

        private bool MoveToTakingPoint(Vector3 takingPointPosition)
        {
            var randomOffset = UnityEngine.Random.insideUnitSphere;
            randomOffset.y = 0;

            if (navMeshAgent.TrySnapToNavMesh(takingPointPosition + randomOffset, out Vector3 destination))
                return navMeshAgent.SetWaypoints(destination);

            return navMeshAgent.SetWaypoints(takingPointPosition);
        }

        private void OnStorageReached()
        {
            storageReached = true;

            KeepAlive();
        }

        protected override void OnTick()
        {
            if (navMeshAgent.IsMoving)
                KeepAlive();

            if (target.ActiveTask == null || !target.ActiveTask.IsActive || !targetStorage.IsOperational)
            {
                InvokeOnFinished();
                return;
            }

            if (targetStorage.InStorage.IsFull())
            {
                InvokeOnFinished();

                return;
            }

            if (!target.Inventory.HasResource(targetStorage.InStorage.RequiredResources))
            {
                InvokeOnFinished();

                return;
            }

            if (target.LastTimeResourceGiven > lastDeliveryTime)
            {
                lastDeliveryTime = target.LastTimeResourceGiven;

                KeepAlive();
            }

            if (!storageReached && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }
        }

        protected override void OnExit()
        {
            navMeshAgent.Stop();

            target.UnlinkActiveTask();
        }
    }

    public class BuildingState : HelperStateBehavior
    {
        private const float BUILDING_STANDOFF_DISTANCE = 0.5f;

        private ConstructionPointBehavior targetConstructionPoint;
        private bool isBuilding;
        private int lastHitsMade;

        protected override float MaxDuration => HelperStateTimeouts.BUILDING;

        public BuildingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        protected override void OnEnter()
        {
            navMeshAgent.Stop();
            target.StopSnapping();

            isBuilding = false;

            if (target.ActiveTask is not ConstructionTask constructionTask)
            {
                InvokeOnFinished();

                return;
            }

            targetConstructionPoint = constructionTask.ConstructionPointBehavior;
            lastHitsMade = targetConstructionPoint.HitsMade;

            target.SetTargetHitableObject(targetConstructionPoint);

            if (!MoveToApproachPosition())
            {
                OnWatchdogTimeout();

                return;
            }

            navMeshAgent.PathFinished += OnBuildingReached;
        }

        private bool MoveToApproachPosition()
        {
            var helperPosition = target.transform.position;

            var surfacePoint = targetConstructionPoint.BoxCollider.ClosestPoint(helperPosition);

            var direction = (helperPosition - surfacePoint).SetY(0).normalized;
            if (direction == Vector3.zero)
                direction = (helperPosition - targetConstructionPoint.transform.position).SetY(0).normalized;
            if (direction == Vector3.zero)
                direction = target.transform.forward;

            var approachPosition = surfacePoint.SetY(helperPosition.y) + direction * BUILDING_STANDOFF_DISTANCE;

            if (navMeshAgent.TrySnapToNavMesh(approachPosition, out Vector3 destination) && navMeshAgent.SetWaypoints(destination))
                return true;

            var extents = targetConstructionPoint.BoxCollider.bounds.extents;
            var approachDistance = Mathf.Max(extents.x, extents.z) + BUILDING_STANDOFF_DISTANCE;

            return navMeshAgent.MoveToTarget(targetConstructionPoint.transform.position, approachDistance);
        }

        private void OnBuildingReached()
        {
            if (targetConstructionPoint != null)
            {
                if(!targetConstructionPoint.IsBuilt)
                {
                    targetConstructionPoint.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
                }

                var lookAt = (targetConstructionPoint.transform.position - target.transform.position).SetY(0).normalized;

                if (lookAt.sqrMagnitude > Mathf.Epsilon)
                    target.transform.rotation = Quaternion.LookRotation(lookAt);

                isBuilding = true;

                KeepAlive();
            }
        }

        protected override void OnTick()
        {
            if (navMeshAgent.IsMoving)
                KeepAlive();

            if(Target.ActiveTask == null || !Target.ActiveTask.IsActive)
            {
                InvokeOnFinished();

                return;
            }

            if (targetConstructionPoint != null && targetConstructionPoint.IsBuilt)
            {
                InvokeOnFinished();

                return;
            }

            if (!isBuilding && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();

                return;
            }

            if (isBuilding && targetConstructionPoint != null)
            {
                if (targetConstructionPoint.HitsMade > lastHitsMade)
                {
                    lastHitsMade = targetConstructionPoint.HitsMade;

                    KeepAlive();
                }

                target.SnapToHittable(targetConstructionPoint);
            }
        }

        protected override void OnExit()
        {
            navMeshAgent.Stop();

            target.StopSnapping();

            target.Graphics.InteractionAnimations.Disable();

            target.UnlinkActiveTask();

            isBuilding = false;
        }
    }

    public static class HelperStateTimeouts
    {
        public const float GATHERING = 15f;
        public const float STORING = 15f;
        public const float BUILDING = 20f;
    }
}
