using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.AI;

namespace Watermelon
{
    public class NavMeshAgentBehaviour
    {
        private static readonly Vector3[] DEFAULT_WAYPOINTS_ARRAY = new Vector3[1] { Vector3.zero };

        // When a waypoint sits inside a NavMesh Obstacle (e.g. a berry bush or coconut tree),
        // the agent only reaches a partial path and never gets within stoppingDistance, so the
        // regular arrival check never triggers. These thresholds detect that stall and treat the
        // waypoint as reached once the agent has stopped making progress for long enough.
        private const float STUCK_TIMEOUT = 1.0f;
        private const float STUCK_PROGRESS_THRESHOLD_SQR = 0.1f * 0.1f;

        private bool isMoving;
        public bool IsMoving => isMoving;

        private Vector3[] waypoints;
        private int currentWaypointIndex = 0;

        private Vector3 currentPoint;

        private Vector3 lastProgressPosition;
        private float stuckTime;

        private NavMeshAgent navMeshAgent;
        private INavMeshAgent navMeshAgentBehaviour;
        private NavMeshPath path;

        public event SimpleCallback PathFinished;

        public void Initialise(INavMeshAgent navMeshAgentBehaviour, NavMeshAgent navMeshAgent)
        {
            this.navMeshAgent = navMeshAgent;
            this.navMeshAgentBehaviour = navMeshAgentBehaviour;

            path = new NavMeshPath();

            waypoints = DEFAULT_WAYPOINTS_ARRAY;

            isMoving = false;
            navMeshAgent.enabled = false;
        }

        public void Unload()
        {
            isMoving = false;
            navMeshAgent.enabled = false;
        }

        public void SetWaypoints(params Vector3[] positions)
        {
            isMoving = true;

            navMeshAgent.enabled = true;

            PathFinished = null;

            waypoints = positions;

            currentPoint = positions[0];
            currentWaypointIndex = 0;

            ResetStuckTracking();

            if (navMeshAgent.isOnNavMesh)
            {
                if (navMeshAgent.isStopped)
                    navMeshAgent.isStopped = false;

                navMeshAgent.SetDestination(currentPoint);

                navMeshAgentBehaviour.OnNavMeshAgentStartedMovement(currentPoint);
            }
        }

        private void ResetStuckTracking()
        {
            lastProgressPosition = navMeshAgent.transform.position;
            stuckTime = 0f;
        }

        public void Update()
        {
            if (!isMoving)
                return;

            if (navMeshAgent.isActiveAndEnabled)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                    {
                        OnWaypointReached();
                    }
                    else if (IsStuck())
                    {
                        OnWaypointReached();
                    }
                }
            }
        }

        private bool IsStuck()
        {
            Vector3 currentPosition = navMeshAgent.transform.position;

            if ((currentPosition - lastProgressPosition).sqrMagnitude >= STUCK_PROGRESS_THRESHOLD_SQR)
            {
                lastProgressPosition = currentPosition;
                stuckTime = 0f;

                return false;
            }

            stuckTime += Time.deltaTime;

            return stuckTime >= STUCK_TIMEOUT;
        }

        private void OnWaypointReached()
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                isMoving = false;

                if (navMeshAgent.isActiveAndEnabled)
                    navMeshAgent.isStopped = true;

                navMeshAgentBehaviour.OnNavMeshAgentStopped();

                PathFinished?.Invoke();
                PathFinished = null;

                return;
            }

            currentPoint = waypoints[currentWaypointIndex];

            navMeshAgent.SetDestination(currentPoint);

            ResetStuckTracking();

            navMeshAgentBehaviour.OnNavMeshWaypointChanged(currentPoint);
        }

        public void Stop()
        {
            if (!isMoving)
                return;

            isMoving = false;

            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;

            navMeshAgentBehaviour.OnNavMeshAgentStopped();

            PathFinished = null;
        }

        public void Warp(Vector3 position, Quaternion quaternion)
        {
            navMeshAgentBehaviour.OnNavMeshWarpStarted();

            navMeshAgent.Warp(position);
            navMeshAgent.transform.rotation = quaternion;

            navMeshAgentBehaviour.OnNavMeshWarpFinished();
        }

        public void Warp(Vector3 position)
        {
            navMeshAgentBehaviour.OnNavMeshWarpStarted();

            navMeshAgent.Warp(position);

            navMeshAgentBehaviour.OnNavMeshWarpFinished();
        }

        public void Warp(Transform destinationTransform)
        {
            navMeshAgentBehaviour.OnNavMeshWarpStarted();

            navMeshAgent.Warp(destinationTransform.position);
            navMeshAgent.transform.rotation = destinationTransform.rotation;

            navMeshAgentBehaviour.OnNavMeshWarpFinished();
        }

        public void Warp(Transform destinationTransform, Quaternion quaternion)
        {
            navMeshAgentBehaviour.OnNavMeshWarpStarted();

            navMeshAgent.Warp(destinationTransform.position);
            navMeshAgent.transform.rotation = quaternion;

            navMeshAgentBehaviour.OnNavMeshWarpFinished();
        }

        public bool PathExists(Vector3 point)
        {
            // Calculate the path from the current position to the target position
            if (NavMesh.CalculatePath(navMeshAgent.transform.position, point, navMeshAgent.areaMask, path))
            {
                // Check if the path status is complete
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return true;
                }
            }

            return false;
        }
    }
}