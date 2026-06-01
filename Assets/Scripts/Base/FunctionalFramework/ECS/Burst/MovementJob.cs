#if UNITY_BURST
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Functional.ECS.Burst
{
    /// <summary>
    /// Burst 编译的移动 Job。批量更新位置数组。
    /// 需要安装 com.unity.burst、com.unity.collections、com.unity.mathematics。
    /// 安装后 Unity 自动定义 UNITY_BURST，ECSWorldRunner 即可启用本 Job。
    /// </summary>
    [BurstCompile]
    public struct MovementJob : IJobParallelFor
    {
        public float DeltaTime;
        public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<float3> Velocities;

        public void Execute(int index)
        {
            Positions[index] += Velocities[index] * DeltaTime;
        }
    }
}
#endif
