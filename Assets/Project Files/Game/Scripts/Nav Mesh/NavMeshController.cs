using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public static class NavMeshController
    {
        private static List<NavMeshSurface> navMeshSurfaces = new List<NavMeshSurface>();
        public static List<NavMeshSurface> NavMeshSurface => navMeshSurfaces;

        private static readonly List<NavMeshObstacle> parkedCarvers = new();

        private static bool isNavMeshCalculated;
        public static bool IsNavMeshCalculated => isNavMeshCalculated;

        public static event SimpleCallback NavMeshRecalculated;

        private static bool navMeshRecalculating;
        private static bool pendingRecalculation;
        private static Coroutine updateCoroutine;

        public static void AddNavMeshSurface(NavMeshSurface navMeshSurface)
        {
            if (navMeshSurfaces.FindIndex(x => x == navMeshSurface) == -1)
                navMeshSurfaces.Add(navMeshSurface);

            isNavMeshCalculated = false;
        }

        public static void RemoveNavMeshSurface(NavMeshSurface navMeshSurface)
        {
            int surfaceIndex = navMeshSurfaces.FindIndex((x) => x == navMeshSurface);
            if(surfaceIndex != -1)
            {
                navMeshSurfaces.RemoveAt(surfaceIndex);
            }
        }

        public static void CalculateNavMesh(SimpleCallback simpleCallback = null)
        {
            if (simpleCallback != null)
                NavMeshRecalculated += simpleCallback;

            if (navMeshRecalculating)
            {
                pendingRecalculation = true;
                return;
            }

            RunCalculation();
        }

        private static void RunCalculation()
        {
            navMeshRecalculating = true;
            pendingRecalculation = false;

            updateCoroutine = Tween.InvokeCoroutine(CalculationCoroutine(() =>
            {
                isNavMeshCalculated = true;
                navMeshRecalculating = false;

                if (pendingRecalculation)
                {
                    RunCalculation();
                    return;
                }

                var callbacks = NavMeshRecalculated;
                NavMeshRecalculated = null;
                callbacks?.Invoke();
            }));
        }

        private static IEnumerator CalculationCoroutine(SimpleCallback onRecalculated)
        {
            AsyncOperation updateOperation;

            ParkCarvers();

            foreach(var navMeshSurface in navMeshSurfaces)
            {
                updateOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

                while(!updateOperation.isDone)
                {
                    yield return null;
                }
            }

            yield return null;

            ReleaseCarvers();

            onRecalculated?.Invoke();
        }

        private static void ParkCarvers()
        {
            ReleaseCarvers();

            var obstacles = Object.FindObjectsByType<NavMeshObstacle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (var i = 0; i < obstacles.Length; i++)
            {
                var obstacle = obstacles[i];

                if (!obstacle.carving)
                    continue;

                obstacle.carving = false;

                parkedCarvers.Add(obstacle);
            }
        }

        private static void ReleaseCarvers()
        {
            for (var i = 0; i < parkedCarvers.Count; i++)
            {
                if (parkedCarvers[i] != null)
                    parkedCarvers[i].carving = true;
            }

            parkedCarvers.Clear();
        }

        public static void InvokeOrSubscribe(SimpleCallback callback)
        {
            if (isNavMeshCalculated)
            {
                callback?.Invoke();
            }
            else
            {
                NavMeshRecalculated += callback;
            }
        }

        public static void Reset()
        {
            if (updateCoroutine != null)
            {
                Tween.StopCustomCoroutine(updateCoroutine);

                updateCoroutine = null;
            }

            ReleaseCarvers();

            navMeshRecalculating = false;
            pendingRecalculation = false;
            isNavMeshCalculated = false;

            NavMeshRecalculated = null;

            navMeshSurfaces.Clear();
        }
    }
}
