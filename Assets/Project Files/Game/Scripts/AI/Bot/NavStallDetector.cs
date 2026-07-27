using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public struct NavStallDetector
    {
        private const float STALL_TIME = 1.0f;
        private const float MIN_SPEED_SQR = 0.05f * 0.05f;

        private float stalledTime;

        public void Reset()
        {
            stalledTime = 0f;
        }

        public bool Tick(NavMeshAgent agent)
        {
            if (agent == null || !agent.isOnNavMesh || agent.pathPending)
            {
                stalledTime = 0f;
                return false;
            }

            var reachedDestination = agent.pathStatus == NavMeshPathStatus.PathComplete &&
                agent.remainingDistance <= agent.stoppingDistance;

            if (reachedDestination || agent.velocity.sqrMagnitude > MIN_SPEED_SQR)
            {
                stalledTime = 0f;
                return false;
            }

            stalledTime += Time.deltaTime;

            return stalledTime >= STALL_TIME;
        }
    }
}
