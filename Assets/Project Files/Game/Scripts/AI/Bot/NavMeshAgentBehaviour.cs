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

        private const int RECOVERY_ATTEMPTS = 2;

        private bool isMoving;
        public bool IsMoving => isMoving;

        private Vector3[] waypoints;
        private int currentWaypointIndex = 0;

        private Vector3 currentPoint;

        private NavStallDetector stallDetector;
        private int recoveryAttempts;

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

            ResetStallTracking();

            if (navMeshAgent.isOnNavMesh)
            {
                if (navMeshAgent.isStopped)
                    navMeshAgent.isStopped = false;

                navMeshAgent.SetDestination(currentPoint);

                navMeshAgentBehaviour.OnNavMeshAgentStartedMovement(currentPoint);
            }
        }

        private void ResetStallTracking()
        {
            stallDetector.Reset();
            recoveryAttempts = 0;
        }

        public void Update()
        {
            if (!isMoving)
                return;

            if (!navMeshAgent.isActiveAndEnabled || navMeshAgent.pathPending)
                return;

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                OnWaypointReached();

                return;
            }

            if (stallDetector.Tick(navMeshAgent))
                OnStalled();
        }

        private void OnStalled()
        {
            if (recoveryAttempts < RECOVERY_ATTEMPTS)
            {
                recoveryAttempts++;
                stallDetector.Reset();
                navMeshAgent.SetDestination(currentPoint);

                return;
            }

            isMoving = false;

            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;

            navMeshAgentBehaviour.OnNavMeshAgentStopped();

            MovementStalled?.Invoke();
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

            ResetStallTracking();

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