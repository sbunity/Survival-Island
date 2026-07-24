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

        public override void OnStart()
        {
            navMeshAgent.Stop();
            target.Graphics.InteractionAnimations.Disable();
            target.ShowRecoveryHealthbar();
        }

        public override void OnUpdate()
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

        public override void OnStart()
        {
            defendTask = target.ActiveTask as DefendBaseTask;
            controller = defendTask?.Controller;
            nextMovementRefreshTime = Time.time;

            navMeshAgent.Stop();
            target.ClearCombatTarget();
        }

        public override void OnUpdate()
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
                var attackPosition = combatTarget.GetAttackPosition(target.transform.position);
                var offset = attackPosition - target.transform.position;
                offset.y = 0f;

                if (offset.sqrMagnitude <= target.CombatRange * target.CombatRange)
                {
                    target.TryAttack();
                }
                else if (Time.time >= nextMovementRefreshTime)
                {
                    nextMovementRefreshTime = Time.time + TARGET_MOVEMENT_REFRESH_DELAY;
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

        public override void OnEnd()
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

        public override void OnStart()
        {
            leashOrigin = target.transform.position;
            nextMovementRefreshTime = Time.time;

            navMeshAgent.Stop();

            AcquireTarget();
        }

        public override void OnUpdate()
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

            var attackPosition = combatTarget.GetAttackPosition(target.transform.position);
            var offset = attackPosition - target.transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= target.CombatRange * target.CombatRange)
            {
                target.TryAttack();
            }
            else if (Time.time >= nextMovementRefreshTime)
            {
                nextMovementRefreshTime = Time.time + MOVEMENT_REFRESH_DELAY;
                target.MoveToCombatTarget();
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

        public override void OnEnd()
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

        public override void OnStart()
        {
            isSitting = false;

            if (target != null)
            {
                navMeshAgent.Stop();
                navMeshAgent.SetWaypoints(target.GetRestPosition());
                navMeshAgent.PathFinished += RestPositionReached;
            }
        }

        private void RestPositionReached()
        {
            isSitting = true;

            target.ActivateSittingAnimation();
        }

        public override void OnUpdate()
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

        public override void OnEnd()
        {
            isSitting = false;

            target.DisableSittingAnimation();

            target.EmoteBehavior.Hide();
        }
    }

    public class FishingState : HelperStateBehavior
    {
        private FishingPlaceBehavior fishingPlace;

        private bool isGathering;

        public FishingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            isGathering = false;
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                FishingTask fishingTask = (FishingTask)task;
                if (fishingTask != null)
                {
                    fishingPlace = fishingTask.FishingPlaceBehavior;

                    target.SetTargetHitableObject(fishingPlace);

                    Vector3 direction = (target.transform.position - fishingPlace.transform.position).normalized;

                    navMeshAgent.SetWaypoints(fishingPlace.transform.position + direction);
                    navMeshAgent.PathFinished += OnResourceReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnResourceReached()
        {
            if (fishingPlace.Health > 0 && Vector3.Distance(target.transform.position, fishingPlace.transform.position) <= 2.0f)
            {
                isGathering = true;

                navMeshAgent.Stop();

                fishingPlace.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
            }
            else
            {
                InvokeOnFinished();
            }
        }

        public override void OnUpdate()
        {
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
                target.SnapToHittable(fishingPlace);
            }
        }

        public override void OnEnd()
        {
            target.Graphics.InteractionAnimations.Disable();

            navMeshAgent.Stop();

            isGathering = false;

            target.UnlinkActiveTask();
        }
    }

    public class GatheringState : HelperStateBehavior
    {
        private ResourceSourceBehavior targetResource;

        private bool isGathering;

        public GatheringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            isGathering = false;
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if(task != null)
            {
                GatheringTask geatheringTask = (GatheringTask)task;
                if(geatheringTask != null)
                {
                    targetResource = geatheringTask.ResourceSource;

                    target.SetTargetHitableObject(targetResource);

                    Vector3 direction = (target.transform.position - targetResource.transform.position).normalized;

                    navMeshAgent.SetWaypoints(targetResource.transform.position + direction);
                    navMeshAgent.PathFinished += OnResourceReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnResourceReached()
        {
            if(targetResource.Health > 0 && Vector3.Distance(target.transform.position, targetResource.transform.position) <= 2.0f)
            {
                isGathering = true;

                navMeshAgent.Stop();

                targetResource.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
            }
            else
            {
                InvokeOnFinished();
            }
        }

        public override void OnUpdate()
        {
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
                target.SnapToHittable(targetResource);
            }
        }

        public override void OnEnd()
        {
            target.Graphics.InteractionAnimations.Disable();

            navMeshAgent.Stop();

            isGathering = false;

            target.UnlinkActiveTask();
        }
    }

    public class StoringState : HelperStateBehavior
    {
        private ResourceStorageBuildingBehavior targetStorage;

        private bool storageReached;

        public StoringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                StoreResourcesTask storeResourcesTask = (StoreResourcesTask)task;
                if (storeResourcesTask != null)
                {
                    targetStorage = storeResourcesTask.StorageBuildingBehavior;

                    Transform targetTransform = targetStorage.Storage.ResourceTakingPoint.transform;

                    storageReached = false;

                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
                    randomOffset.y = 0;

                    navMeshAgent.SetWaypoints(targetTransform.transform.position + randomOffset);
                    navMeshAgent.PathFinished += OnStorageReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnStorageReached()
        {
            storageReached = true;
        }

        public override void OnUpdate()
        {
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

            if(!storageReached && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }
        }

        public override void OnEnd()
        {
            navMeshAgent.Stop();

            target.UnlinkActiveTask();
        }
    }

    public class ConverterStoringState : HelperStateBehavior
    {
        private ResourceConverterBuildingBehavior targetStorage;

        private bool storageReached;

        public ConverterStoringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                ConverterStoringTask converterStoringTask = (ConverterStoringTask)task;
                if (converterStoringTask != null)
                {
                    targetStorage = converterStoringTask.ResourceConverter;

                    Transform targetTransform = targetStorage.InStorage.ResourceTakingPoint.transform;

                    storageReached = false;

                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
                    randomOffset.y = 0;

                    navMeshAgent.SetWaypoints(targetTransform.transform.position + randomOffset);
                    navMeshAgent.PathFinished += OnStorageReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnStorageReached()
        {
            storageReached = true;
        }

        public override void OnUpdate()
        {
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

            if (!storageReached && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }
        }

        public override void OnEnd()
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

        public BuildingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();

            isBuilding = false;

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                ConstructionTask constructionTask = (ConstructionTask)task;
                if (constructionTask != null)
                {
                    targetConstructionPoint = constructionTask.ConstructionPointBehavior;

                    target.SetTargetHitableObject(targetConstructionPoint);

                    navMeshAgent.SetWaypoints(GetApproachPosition());
                    navMeshAgent.PathFinished += OnBuildingReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private Vector3 GetApproachPosition()
        {
            var helperPosition = target.transform.position;

            var surfacePoint = targetConstructionPoint.BoxCollider.ClosestPoint(helperPosition);

            var direction = (helperPosition - surfacePoint).SetY(0).normalized;
            if (direction == Vector3.zero)
                direction = (helperPosition - targetConstructionPoint.transform.position).SetY(0).normalized;
            if (direction == Vector3.zero)
                direction = target.transform.forward;

            return surfacePoint.SetY(helperPosition.y) + direction * BUILDING_STANDOFF_DISTANCE;
        }

        private void OnBuildingReached()
        {
            if (targetConstructionPoint != null)
            {
                if(!targetConstructionPoint.IsBuilt)
                {
                    targetConstructionPoint.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
                }

                Vector3 lookAt = (targetConstructionPoint.transform.position - target.transform.position).SetY(0).normalized;

                target.transform.rotation = Quaternion.LookRotation(lookAt);

                isBuilding = true;
            }
        }

        public override void OnUpdate()
        {
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

            if (isBuilding && targetConstructionPoint != null)
            {
                target.SnapToHittable(targetConstructionPoint);
            }
        }

        public override void OnEnd()
        {
            navMeshAgent.Stop();

            target.Graphics.InteractionAnimations.Disable();

            target.UnlinkActiveTask();

            isBuilding = false;
        }
    }

    public class WaitingState : HelperStateBehavior
    {
        public WaitingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();
        }

        public override void OnUpdate()
        {

        }

        public override void OnEnd()
        {

        }
    }
}
