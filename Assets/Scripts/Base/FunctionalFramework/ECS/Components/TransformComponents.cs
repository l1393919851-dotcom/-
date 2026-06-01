using UnityEngine;

namespace Functional.ECS.Components
{
    /// <summary>
    /// 位置组件（ECS 数据，与 GameObject 解耦）。
    /// </summary>
    public struct PositionComponent : IComponent
    {
        public Vector3 Value;
    }

    /// <summary>
    /// 速度组件。
    /// </summary>
    public struct VelocityComponent : IComponent
    {
        public Vector3 Value;
    }

    /// <summary>
    /// 生命值组件（怪物示例）。
    /// </summary>
    public struct HealthComponent : IComponent
    {
        public float Current;
        public float Max;
    }

    /// <summary>
    /// 关联的 GameObject 视图（可选，用于表现层同步）。
    /// </summary>
    public struct ViewLinkComponent : IComponent
    {
        public int ViewId;
    }
}
