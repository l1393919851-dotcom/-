using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 实体：一个 Id 加一组组件，本身不写玩法逻辑。
    /// 组件按 ComponentKey 存一份字典用于查找，另存一份列表保证生命周期按添加顺序执行。
    /// 不要在组件的生命周期回调里增删本实体的组件。
    /// </summary>
    public class LEntity : IPoolable
    {
        public IReadOnlyList<LComponent> Components => _componentList;

        private readonly Dictionary<Type, LComponent> _components = new Dictionary<Type, LComponent>();
        private readonly List<LComponent> _componentList = new List<LComponent>();

        public LWorld World { get; private set; }
        public int Id { get; private set; }

        public void Init(LWorld world, int id)
        {
            World = world;
            Id = id;
        }

        public virtual void Tick(float dt)
        {
            for (int i = 0; i < _componentList.Count; i++)
            {
                _componentList[i].Tick(dt);
            }
        }

        public void OnEntityAwake()
        {
            for (int i = 0; i < _componentList.Count; i++)
            {
                _componentList[i].OnComponentAwake();
            }
        }

        public void OnEntityStart()
        {
            for (int i = 0; i < _componentList.Count; i++)
            {
                _componentList[i].OnComponentStart();
            }
        }

        public void OnEntityEnd()
        {
            for (int i = 0; i < _componentList.Count; i++)
            {
                _componentList[i].OnComponentEnd();
            }
        }

        public void OnEntityDestroy()
        {
            for (int i = 0; i < _componentList.Count; i++)
            {
                _componentList[i].OnComponentDestroy();
            }
        }

        /// <summary>
        /// 回池：组件逐个还回世界的对象池，再清空自己。
        /// </summary>
        public virtual void OnRecycle()
        {
            var pools = World != null ? World.ObjectPoolManager : null;
            for (int i = 0; i < _componentList.Count; i++)
            {
                if (pools != null)
                {
                    pools.Recycle(_componentList[i]);
                }
                else
                {
                    _componentList[i].OnRecycle();
                }
            }
            _componentList.Clear();
            _components.Clear();
            World = null;
            Id = 0;
        }

        public T GetComponent<T>() where T : LComponent
        {
            return _components.TryGetValue(typeof(T), out var com) ? (T)com : null;
        }

        public bool TryGetComponent<T>(out T component) where T : LComponent
        {
            component = GetComponent<T>();
            return component != null;
        }

        public bool HasComponent<T>() where T : LComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 挂一个已经 Init 过的组件。同槽位已有组件时，旧组件会被回池并报错。
        /// </summary>
        public void AddComponent(LComponent component)
        {
            var key = component.ComponentKey;
            if (_components.TryGetValue(key, out var old))
            {
                EcsLog.Error("Entity " + Id + " already has component " + key.Name + ", old one recycled");
                _componentList.Remove(old);
                if (World != null)
                {
                    World.ObjectPoolManager.Recycle(old);
                }
            }
            component.Entity = this;
            _components[key] = component;
            _componentList.Add(component);
        }

        /// <summary>
        /// 从世界对象池取一个组件，Init 后挂到本实体。实体必须已经 Init 过。
        /// </summary>
        public T AddComponent<T>(object createParam = null) where T : LComponent, new()
        {
            var component = World.ObjectPoolManager.Take<T>();
            component.Init(createParam);
            AddComponent(component);
            return component;
        }

        /// <summary>
        /// 只摘掉不回调、不回池，返回被摘掉的组件，由调用方决定怎么处理。
        /// </summary>
        public LComponent RemoveComponent<T>() where T : LComponent
        {
            return RemoveComponent(typeof(T));
        }

        public LComponent RemoveComponent(Type key)
        {
            if (!_components.TryGetValue(key, out var component))
            {
                return null;
            }
            _components.Remove(key);
            _componentList.Remove(component);
            return component;
        }
    }
}
