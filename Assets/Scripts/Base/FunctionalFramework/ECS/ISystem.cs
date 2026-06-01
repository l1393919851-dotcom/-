namespace Functional.ECS
{
    /// <summary>
    /// ECS 系统接口。系统在 World 中按注册顺序每帧执行。
    /// </summary>
    public interface ISystem
    {
        /// <summary>是否启用。</summary>
        bool Enabled { get; set; }

        /// <summary>系统创建时调用。</summary>
        void OnCreate(World world);

        /// <summary>每帧更新。</summary>
        void OnUpdate(World world, float deltaTime);

        /// <summary>世界销毁时调用。</summary>
        void OnDestroy(World world);
    }

    /// <summary>
    /// 系统基类，提供 Enabled 默认实现。
    /// </summary>
    public abstract class SystemBase : ISystem
    {
        public bool Enabled { get; set; } = true;

        public virtual void OnCreate(World world) { }
        public abstract void OnUpdate(World world, float deltaTime);
        public virtual void OnDestroy(World world) { }
    }
}
