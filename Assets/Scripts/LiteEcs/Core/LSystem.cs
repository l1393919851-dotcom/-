using System;

namespace LiteEcs
{
    /// <summary>
    /// 系统基类。按 AddSystem 的顺序执行：
    /// World.Start 时全部 Awake 再全部 Start；每帧 OnSystemTick；World.Dispose 时全部 End、全部 Destroy、全部 Dispose。
    /// 系统按 SystemKey（默认具体类型）注册，同一 World 里每种系统只有一个。
    /// </summary>
    public abstract class LSystem
    {
        public virtual Type SystemKey => GetType();

        public LWorld World { get; private set; }

        internal void Init(LWorld world)
        {
            World = world;
            OnInit();
        }

        internal void Dispose()
        {
            OnDispose();
        }

        public virtual void OnSystemAwake()
        {
        }

        public virtual void OnSystemStart()
        {
        }

        public virtual void OnSystemTick(float dt)
        {
            OnTick(dt);
        }

        public virtual void OnSystemEnd()
        {
        }

        public virtual void OnSystemDestroy()
        {
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnTick(float dt)
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}
