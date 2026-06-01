using Functional.ECS.Components;
using UnityEngine;

namespace Functional.ECS.Systems
{
    /// <summary>
    /// 移动系统（托管版）。根据 Velocity 更新 Position，并同步到 ViewLink 关联的 GameObject。
    /// 大量怪物时安装 Burst 相关包，由 ECSWorldRunner 自动切换为 BurstMovementSystem。
    /// </summary>
    public class MovementSystem : SystemBase
    {
        private readonly ECSViewRegistry _views;

        public MovementSystem(ECSViewRegistry views)
        {
            _views = views;
        }

        public override void OnUpdate(World world, float deltaTime)
        {
            var positions = world.GetStore<PositionComponent>();
            var velocities = world.GetStore<VelocityComponent>();
            var views = world.GetStore<ViewLinkComponent>();

            int count = Mathf.Min(positions.Count, velocities.Count);
            for (int i = 0; i < count; i++)
            {
                var entity = positions.GetEntityAt(i);
                if (!velocities.TryGet(entity, out var vel)) continue;

                var pos = positions.GetAt(i);
                pos.Value += vel.Value * deltaTime;
                positions.Set(entity, pos);

                if (views.TryGet(entity, out var link) && _views != null)
                {
                    var go = _views.Get(link.ViewId);
                    if (go != null) go.transform.position = pos.Value;
                }
            }
        }
    }
}
