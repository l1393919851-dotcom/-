using Unity.Entities;
using Unity.Mathematics;

namespace Functional.DOTS.Components
{
    /// <summary>怪物移动速度（纯数据组件）。</summary>
    public struct MonsterVelocity : IComponentData
    {
        public float3 Value;
    }

}
