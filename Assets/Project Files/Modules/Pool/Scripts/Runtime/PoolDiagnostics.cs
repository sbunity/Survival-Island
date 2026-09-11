using UnityEngine;

namespace Watermelon
{
    internal static class PoolDiagnostics
    {
        private const string DESTROYED_OBJECT_FORMAT = "[Pool]: A pooled object ({0}) was destroyed externally and has been dropped from the pool.\nThis usually means the object was not returned to the pool before its parent got destroyed.\nPlease review your object management logic to prevent unintended object destruction.";

        public static void LogDestroyedObject(string poolName)
        {
            Debug.LogError(string.Format(DESTROYED_OBJECT_FORMAT, poolName));
        }
    }
}
