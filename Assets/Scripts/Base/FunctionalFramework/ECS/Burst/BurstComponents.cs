#if UNITY_BURST
using Unity.Mathematics;

namespace Functional.ECS.Burst
{
    /// <summary>
    /// Burst 友好的位置数据（float3）。
    /// </summary>
    public struct BurstPosition
    {
        public float3 Value;
    }

    /// <summary>
    /// Burst 友好的速度数据。
    /// </summary>
    public struct BurstVelocity
    {
        public float3 Value;
    }
}
#endif
