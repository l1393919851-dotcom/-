using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 一个 ECS 会话。持有实体管理、计时器、对象池、事件和有序系统列表。
    /// 外部每帧调 Tick(dt)，顺序：实体管理 Tick → 计时器 Tick → 各系统 Tick → OnTick。
    /// </summary>
    public class LWorld
    {
        private readonly Dictionary<Type, LSystem> _systems = new Dictionary<Type, LSystem>();
        private readonly List<LSystem> _orderedSystems = new List<LSystem>();
        private int _nextEntityId = 1;

        public LEntityManager EntityManager { get; }
        public LTimerManager TimerManager { get; }
        public LObjectPoolManager ObjectPoolManager { get; }
        public LEventManager EventManager { get; }

        /// <summary>
        /// 构造后为 true，Dispose 一开始就置 false。异步回调回来时先判断它。
        /// </summary>
        public bool Alive { get; private set; }

        public LWorld()
        {
            ObjectPoolManager = new LObjectPoolManager();
            TimerManager = new LTimerManager();
            EventManager = new LEventManager();
            EntityManager = CreateEntityManager();
            Alive = true;
        }

        /// <summary>
        /// 换实体管理器的入口。基类构造函数里调用，此时子类自己的字段还没初始化，重写时不要用它们。
        /// </summary>
        protected virtual LEntityManager CreateEntityManager()
        {
            return new LEntityManager(this);
        }

        #region Entity

        public int GetEntityCount()
        {
            return EntityManager.EntityCount;
        }

        public LEntity GetEntity(int id)
        {
            return EntityManager.GetEntity(id);
        }

        /// <summary>
        /// 从池里取一个实体并分配 Id（从 1 递增），还没加入世界。挂完组件后调 AddEntity。
        /// </summary>
        public LEntity CreateEntity()
        {
            return CreateEntity<LEntity>();
        }

        public T CreateEntity<T>() where T : LEntity, new()
        {
            var entity = ObjectPoolManager.Take<T>();
            entity.Init(this, AllocEntityId());
            return entity;
        }

        /// <summary>
        /// 排队加入，下一次 Tick 才 Awake / Start。
        /// </summary>
        public void AddEntity(LEntity entity)
        {
            EntityManager.AddEntity(entity);
        }

        /// <summary>
        /// 排队移除，本帧 Tick 末尾 End / Destroy 并回池。
        /// </summary>
        public void RemoveEntity(int id)
        {
            EntityManager.RemoveEntity(id);
        }

        #endregion

        #region System

        public void AddSystem(LSystem system)
        {
            var key = system.SystemKey;
            if (_systems.ContainsKey(key))
            {
                EcsLog.Error("System " + key.Name + " already added");
                return;
            }
            _systems.Add(key, system);
            _orderedSystems.Add(system);
            system.Init(this);
        }

        public T GetSystem<T>() where T : LSystem
        {
            return _systems.TryGetValue(typeof(T), out var system) ? (T)system : null;
        }

        #endregion

        #region Event

        /// <summary>
        /// 从池里取一个实体事件并写入实体。填完其他字段后交给 TriggerAndRecycle。
        /// </summary>
        public T TakeEvent<T>(LEntity entity) where T : LEntityEvent, new()
        {
            var evt = ObjectPoolManager.Take<T>();
            evt.Apply(entity);
            return evt;
        }

        /// <summary>
        /// 同步派发后立刻回池。监听方不能在回调外继续持有这个事件对象。
        /// </summary>
        public void TriggerAndRecycle<T>(T evt) where T : LEntityEvent
        {
            EventManager.Trigger(evt);
            ObjectPoolManager.Recycle(evt);
        }

        #endregion

        #region Lifecycle

        public void Start()
        {
            for (int i = 0; i < _orderedSystems.Count; i++)
            {
                try
                {
                    _orderedSystems[i].OnSystemAwake();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < _orderedSystems.Count; i++)
            {
                try
                {
                    _orderedSystems[i].OnSystemStart();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            OnStart();
        }

        public void Tick(float dt)
        {
            EntityManager.Tick(dt);
            TimerManager.Tick(dt);
            for (int i = 0; i < _orderedSystems.Count; i++)
            {
                try
                {
                    _orderedSystems[i].OnSystemTick(dt);
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            OnTick(dt);
        }

        public void Dispose()
        {
            if (!Alive)
            {
                return;
            }
            Alive = false;
            for (int i = 0; i < _orderedSystems.Count; i++)
            {
                try
                {
                    _orderedSystems[i].OnSystemEnd();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < _orderedSystems.Count; i++)
            {
                try
                {
                    _orderedSystems[i].OnSystemDestroy();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < _orderedSystems.Count; i++)
            {
                try
                {
                    _orderedSystems[i].Dispose();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            EntityManager.Dispose();
            ObjectPoolManager.Dispose();
            TimerManager.Dispose();
            EventManager.Release();
            OnDispose();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnTick(float dt)
        {
        }

        protected virtual void OnDispose()
        {
        }

        #endregion

        private int AllocEntityId()
        {
            return _nextEntityId++;
        }
    }
}
