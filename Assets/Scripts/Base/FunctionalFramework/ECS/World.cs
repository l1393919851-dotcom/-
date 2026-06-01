using System;
using System.Collections.Generic;
using UnityEngine;

namespace Functional.ECS
{
    /// <summary>
    /// ECS 世界。管理实体生命周期、组件存储与系统调度。
    /// 
    /// 典型用法：
    ///   var world = new World();
    ///   var e = world.CreateEntity();
    ///   world.AddComponent(e, new PositionComponent { Value = Vector3.zero });
    ///   world.AddSystem(new MovementSystem());
    ///   world.Update(Time.deltaTime);
    /// </summary>
    public class World
    {
        private int _nextEntityId;
        private readonly Dictionary<int, int> _entityVersions = new Dictionary<int, int>();
        private readonly HashSet<int> _aliveEntities = new HashSet<int>();
        private readonly Dictionary<Type, IComponentStore> _stores = new Dictionary<Type, IComponentStore>();
        private readonly List<ISystem> _systems = new List<ISystem>();

        /// <summary>当前存活实体数量。</summary>
        public int EntityCount => _aliveEntities.Count;

        /// <summary>
        /// 创建实体。
        /// </summary>
        public Entity CreateEntity()
        {
            int id = _nextEntityId++;
            _entityVersions[id] = 0;
            _aliveEntities.Add(id);
            return new Entity(id, 0);
        }

        /// <summary>
        /// 销毁实体并移除其所有组件。
        /// </summary>
        public void DestroyEntity(Entity entity)
        {
            if (!entity.IsValid || !_aliveEntities.Contains(entity.Id)) return;

            foreach (var store in _stores.Values)
            {
                store.RemoveEntity(entity);
            }

            _aliveEntities.Remove(entity.Id);
            _entityVersions[entity.Id] = entity.Version + 1;
        }

        /// <summary>
        /// 实体是否存活。
        /// </summary>
        public bool IsAlive(Entity entity)
        {
            return entity.IsValid
                   && _aliveEntities.Contains(entity.Id)
                   && _entityVersions.TryGetValue(entity.Id, out int v)
                   && v == entity.Version;
        }

        /// <summary>
        /// 添加组件。
        /// </summary>
        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            ValidateEntity(entity);
            GetStore<T>().Add(entity, component);
        }

        /// <summary>
        /// 获取组件。
        /// </summary>
        public bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IComponent
        {
            component = default;
            if (!IsAlive(entity)) return false;
            return GetStore<T>().TryGet(entity, out component);
        }

        /// <summary>
        /// 移除组件。
        /// </summary>
        public bool RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            if (!IsAlive(entity)) return false;
            return GetStore<T>().Remove(entity);
        }

        /// <summary>
        /// 获取组件存储（系统批量处理时使用）。
        /// </summary>
        public ComponentStore<T> GetStore<T>() where T : struct, IComponent
        {
            var type = typeof(T);
            if (!_stores.TryGetValue(type, out var store))
            {
                store = new ComponentStore<T>();
                _stores[type] = store;
            }
            return (ComponentStore<T>)store;
        }

        /// <summary>
        /// 注册系统（按注册顺序执行）。
        /// </summary>
        public World AddSystem(ISystem system)
        {
            system.OnCreate(this);
            _systems.Add(system);
            return this;
        }

        /// <summary>
        /// 每帧更新所有系统。
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (var sys in _systems)
            {
                if (sys.Enabled) sys.OnUpdate(this, deltaTime);
            }
        }

        /// <summary>
        /// 销毁世界。
        /// </summary>
        public void Dispose()
        {
            foreach (var sys in _systems)
            {
                sys.OnDestroy(this);
            }
            _systems.Clear();
            foreach (var store in _stores.Values) store.Clear();
            _stores.Clear();
            _aliveEntities.Clear();
            _entityVersions.Clear();
        }

        private void ValidateEntity(Entity entity)
        {
            if (!IsAlive(entity))
            {
                throw new InvalidOperationException($"Entity 无效或已销毁：{entity}");
            }
        }
    }

}
