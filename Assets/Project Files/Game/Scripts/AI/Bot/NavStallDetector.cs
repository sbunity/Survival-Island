using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public enum NavStallVerdict
    {
        Moving = 0,

        Stalled = 1,

        OffNavMesh = 2,
    }

    public struct NavStallDetector
    {
        private const float STALL_TIME = 1.0f;

        private const float MIN_PROGRESS = 0.15f;

        private float stalledTime;
        private float bestRemainingDistance;
        private bool hasSample;

        public void Reset()
        {
            stalledTime = 0f;
            bestRemainingDistance = 0f;
            hasSample = false;
        }

        public NavStallVerdict Tick(NavMeshAgent agent)
        {
            if (agent == null || !agent.isActiveAndEnabled)
            {
                Reset();

                return NavStallVerdict.Moving;
            }

            if (!agent.isOnNavMesh)
            {
                Reset();

                return NavStallVerdict.OffNavMesh;
            }

            if (agent.pathPending)
            {
                stalledTime = 0f;

                return NavStallVerdict.Moving;
            }

            if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance <= agent.stoppingDistance)
            {
                Reset();

                return NavStallVerdict.Moving;
            }

            var remainingDistance = GetRemainingDistance(agent);

            if (!hasSample || remainingDistance < bestRemainingDistance - MIN_PROGRESS)
            {
                hasSample = true;
                bestRemainingDistance = remainingDistance;
                stalledTime = 0f;

                return NavStallVerdict.Moving;
            }

            stalledTime += Time.deltaTime;

            if (stalledTime < STALL_TIME)
                return NavStallVerdict.Moving;

            return NavStallVerdict.Stalled;
        }

        private static float GetRemainingDistance(NavMeshAgent agent)
        {
            var remainingDistance = agent.remainingDistance;

            if (float.IsInfinity(remainingDistance) || float.IsNaN(remainingDistance))
                return Vector3.Distance(agent.transform.position, agent.destination);

            return remainingDistance;
        }
    }
}
