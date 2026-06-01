using Functional.DOTS.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Functional.DOTS.Systems
{
    /// <summary>
    /// Burst 移动系统：只更新 LocalTransform，不触碰 GameObject。
    /// 10 万实体时主要开销在渲染合批，而非本系统。
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MonsterDotsMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            foreach (var (transform, velocity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MonsterVelocity>>())
            {
                transform.ValueRW.Position += velocity.ValueRO.Value * dt;
            }
        }
    }
}
