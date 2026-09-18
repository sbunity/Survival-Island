using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class NavMeshAgentBehaviour
    {
        private static readonly Vector3[] DEFAULT_WAYPOINTS_ARRAY = new Vector3[1] { Vector3.zero };

        private const int RECOVERY_ATTEMPTS = 2;
        private const int OFF_MESH_RECOVERY_ATTEMPTS = 2;

        private const float OFF_MESH_SEARCH_RADIUS = 3f;
        private const float DESTINATION_EPSILON_SQR = 0.04f;

        private const float RETRY_BUDGET_RESET_DELAY = 10f;

        private bool isMoving;
        public bool IsMoving => isMoving;

        public bool IsOnNavMesh => navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh;

        public float AgentRadius => navMeshAgent != null ? navMeshAgent.radius : 0f;

        private Vector3[] waypoints;
        private int currentWaypointIndex = 0;

        private Vector3 currentPoint;
        private bool hasDestination;
        private float lastFailureTime = float.NegativeInfinity;

        private NavStallDetector stallDetector;
        private int recoveryAttempts;
        private int offMeshRecoveryAttempts;

        private bool isTransformDrivenExternally;

        private NavMeshAgent navMeshAgent;
        private INavMeshAgent navMeshAgentBehaviour;
        private NavMeshPath path;

        public event SimpleCallback PathFinished;
        public event SimpleCallback MovementStalled;

        public void Initialise(INavMeshAgent navMeshAgentBehaviour, NavMeshAgent navMeshAgent)
        {
            this.navMeshAgent = navMeshAgent;
            this.navMeshAgentBehaviour = navMeshAgentBehaviour;

            path = new NavMeshPath();

            waypoints = DEFAULT_WAYPOINTS_ARRAY;

            isMoving = false;
            hasDestination = false;

            navMeshAgent.enabled = false;
        }

        public void Unload()
        {
            isMoving = false;
            hasDestination = false;

            PathFinished = null;

            stallDetector.Reset();

            isTransformDrivenExternally = false;

            navMeshAgent.enabled = false;
        }

        #region Movement

        public bool SetWaypoints(params Vector3[] positions)
        {
            if (navMeshAgent == null || positions.IsNullOrEmpty())
                return false;

            navMeshAgent.enabled = true;

            if (!navMeshAgent.isOnNavMesh && !TryRestoreToNavMesh())
            {
                isMoving = false;

                PathFinished = null;

                return false;
            }

            PathFinished = null;

            waypoints = positions;
            currentWaypointIndex = 0;

            var nextPoint = positions[0];

            var destinationChanged = !hasDestination || (nextPoint - currentPoint).sqrMagnitude > DESTINATION_EPSILON_SQR;
            var retryBudgetExpired = Time.time - lastFailureTime > RETRY_BUDGET_RESET_DELAY;

            currentPoint = nextPoint;
            hasDestination = true;

            stallDetector.Reset();
            offMeshRecoveryAttempts = 0;

            if (destinationChanged || retryBudgetExpired)
                recoveryAttempts = 0;

            SetTransformDrivenExternally(false);

            if (navMeshAgent.isStopped)
                navMeshAgent.isStopped = false;

            if (!navMeshAgent.SetDestination(currentPoint))
            {
                isMoving = false;

                navMeshAgentBehaviour.OnNavMeshAgentStopped();

                return false;
            }

            isMoving = true;

            navMeshAgentBehaviour.OnNavMeshAgentStartedMovement(currentPoint);

            return true;
        }

        public bool MoveToTarget(Vector3 targetPosition, float desiredDistance)
        {
            if (!TryResolveApproachPoint(targetPosition, desiredDistance, out Vector3 approachPoint))
                return false;

            return SetWaypoints(approachPoint);
        }

        public bool TrySnapToNavMesh(Vector3 position, out Vector3 point)
        {
            var areaMask = navMeshAgent != null ? navMeshAgent.areaMask : NavMesh.AllAreas;

            return ApproachPointResolver.TrySnapToNavMesh(position, OFF_MESH_SEARCH_RADIUS, areaMask, out point);
        }

        public bool TryResolveApproachPoint(Vector3 targetPosition, float desiredDistance, out Vector3 point)
        {
            point = targetPosition;

            if (navMeshAgent == null)
                return false;

            var request = new ApproachRequest(
                targetPosition,
                navMeshAgent.transform.position,
                desiredDistance,
                navMeshAgent.radius,
                navMeshAgent.areaMask);

            return ApproachPointResolver.TryResolve(request, out point);
        }

        public void Update()
        {
            if (!isMoving)
                return;

            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled)
                return;

            var verdict = stallDetector.Tick(navMeshAgent);

            if (verdict == NavStallVerdict.OffNavMesh)
            {
                HandleOffNavMesh();

                return;
            }

            if (navMeshAgent.pathPending)
                return;

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    OnWaypointReached();
                }
                else
                {
                    FailMovement();
                }

                return;
            }

            if (verdict == NavStallVerdict.Stalled)
                OnStalled();
        }

        private void HandleOffNavMesh()
        {
            if (offMeshRecoveryAttempts >= OFF_MESH_RECOVERY_ATTEMPTS)
            {
                FailMovement();

                return;
            }

            offMeshRecoveryAttempts++;

            if (TryRestoreToNavMesh())
            {
                navMeshAgent.SetDestination(currentPoint);

                return;
            }

            FailMovement();
        }

        private bool TryRestoreToNavMesh()
        {
            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled)
                return false;

            if (navMeshAgent.isOnNavMesh)
                return true;

            if (!ApproachPointResolver.TrySnapToNavMesh(navMeshAgent.transform.position, OFF_MESH_SEARCH_RADIUS, navMeshAgent.areaMask, out Vector3 point))
                return false;

            navMeshAgent.Warp(point);

            return navMeshAgent.isOnNavMesh;
        }

        private void OnStalled()
        {
            if (recoveryAttempts < RECOVERY_ATTEMPTS)
            {
                recoveryAttempts++;

                stallDetector.Reset();

                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.SetDestination(currentPoint);

                return;
            }

            FailMovement();
        }

        private void FailMovement()
        {
            isMoving = false;
            lastFailureTime = Time.time;

            PathFinished = null;

            StopAgent();

            navMeshAgentBehaviour.OnNavMeshAgentStopped();

            MovementStalled?.Invoke();
        }

        private void OnWaypointReached()
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                isMoving = false;

                StopAgent();

                navMeshAgentBehaviour.OnNavMeshAgentStopped();

                var pathFinished = PathFinished;
                PathFinished = null;

                pathFinished?.Invoke();

                return;
            }

            currentPoint = waypoints[currentWaypointIndex];

            ResetStallTracking();

            navMeshAgent.SetDestination(currentPoint);

            navMeshAgentBehaviour.OnNavMeshWaypointChanged(currentPoint);
        }

        private void ResetStallTracking()
        {
            stallDetector.Reset();

            recoveryAttempts = 0;
            offMeshRecoveryAttempts = 0;
        }

        public void Stop()
        {
            PathFinished = null;

            if (!isMoving)
                return;

            isMoving = false;

            StopAgent();

            navMeshAgentBehaviour.OnNavMeshAgentStopped();
        }

        private void StopAgent()
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;
        }

        #endregion

        #region Interaction snapping

        public void SetTransformDrivenExternally(bool isDrivenExternally)
        {
            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled)
                return;

            if (isTransformDrivenExternally == isDrivenExternally)
                return;

            isTransformDrivenExternally = isDrivenExternally;

            if (isDrivenExternally)
            {
                navMeshAgent.updatePosition = false;

                return;
            }

            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.nextPosition = navMeshAgent.transform.position;

            navMeshAgent.updatePosition = true;
        }

        #endregion

        #region Warp

        public void Warp(Vector3 position, Quaternion quaternion)
        {
            Warp(position);

            navMeshAgent.transform.rotation = quaternion;
        }

        public void Warp(Vector3 position)
        {
            navMeshAgentBehaviour.OnNavMeshWarpStarted();

            SetTransformDrivenExternally(false);

            if (!ApproachPointResolver.TrySnapToNavMesh(position, OFF_MESH_SEARCH_RADIUS, navMeshAgent.areaMask, out Vector3 navMeshPosition))
                navMeshPosition = position;

            navMeshAgent.Warp(navMeshPosition);

            ResetStallTracking();

            navMeshAgentBehaviour.OnNavMeshWarpFinished();
        }

        public void Warp(Transform destinationTransform)
        {
            Warp(destinationTransform.position, destinationTransform.rotation);
        }

        public void Warp(Transform destinationTransform, Quaternion quaternion)
        {
            Warp(destinationTransform.position, quaternion);
        }

        #endregion

        public bool PathExists(Vector3 point)
        {
            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
                return false;

            if (NavMesh.CalculatePath(navMeshAgent.transform.position, point, navMeshAgent.areaMask, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
