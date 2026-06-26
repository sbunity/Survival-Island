using UnityEngine;
using Unity.AI.Navigation;

namespace Watermelon
{
    public class NavMeshSurfaceTweenCase : TweenCase
    {
        private NavMeshSurface navMeshSurface;

        private AsyncOperation asyncOperation;

        public NavMeshSurfaceTweenCase(NavMeshSurface navMeshSurface)
        {
            this.navMeshSurface = navMeshSurface;
            SetDelay(0);
            SetDuration(float.MaxValue);
            SetUnscaledMode(true);

            asyncOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        public override void DefaultComplete()
        {

        }

        public override void Invoke(float deltaTime)
        {
            if (asyncOperation.isDone)
                Complete();
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
