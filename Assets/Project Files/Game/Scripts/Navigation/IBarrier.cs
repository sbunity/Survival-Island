using UnityEngine;

namespace Watermelon
{
    public interface IBarrier
    {
        float Length { get; }

        bool Project(Vector3 worldPoint, out float alongPath, out float sideOffset);

        Vector3 GetNormal(float alongPath);

        Vector3 GetPoint(float alongPath);
    }
}
