#if UNITY_BURST
using Functional.ECS.Components;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Functional.ECS.Systems
{
    /// <summary>
    /// Burst 版移动系统。将 Position/Velocity 拷入 NativeArray，用 Job 并行更新后再写回。
    /// 适合大量怪物等实体。
    /// </summary>
    public class BurstMovementSystem : SystemBase
    {
        private readonly ECSViewRegistry _views;

        public BurstMovementSystem(ECSViewRegistry views) => _views = views;

        public override void OnUpdate(World world, float deltaTime)
        {
            var posStore = world.GetStore<PositionComponent>();
            var velStore = world.GetStore<VelocityComponent>();
            var viewStore = world.GetStore<ViewLinkComponent>();
            int count = posStore.Count;
            if (count == 0) return;

            var positions = new NativeArray<float3>(count, Allocator.TempJob);
            var velocities = new NativeArray<float3>(count, Allocator.TempJob);

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var entity = posStore.GetEntityAt(i);
                    positions[i] = posStore.GetAt(i).Value;
                    velocities[i] = velStore.TryGet(entity, out var v) ? (float3)v.Value : float3.zero;
                }

                var job = new Burst.MovementJob
                {
                    DeltaTime = deltaTime,
                    Positions = positions,
                    Velocities = velocities
                };
                job.Schedule(count, 64).Complete();

                for (int i = 0; i < count; i++)
                {
                    var entity = posStore.GetEntityAt(i);
                    posStore.Set(entity, new PositionComponent { Value = positions[i] });

                    if (viewStore.TryGet(entity, out var link) && _views != null)
                    {
                        var go = _views.Get(link.ViewId);
                        if (go != null) go.transform.position = positions[i];
                    }
                }
            }
            finally
            {
                if (positions.IsCreated) positions.Dispose();
                if (velocities.IsCreated) velocities.Dispose();
            }
        }
    }
}
#endif
