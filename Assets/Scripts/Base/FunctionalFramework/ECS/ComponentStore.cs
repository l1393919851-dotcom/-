using System;
using System.Collections.Generic;

namespace Functional.ECS
{
    /// <summary>
    /// 某类型组件的稠密存储。使用数组存储，便于 Job/Burst 批量访问。
    /// </summary>
    public class ComponentStore<T> : IComponentStore where T : struct, IComponent
    {
        void IComponentStore.RemoveEntity(Entity entity) => Remove(entity);
        void IComponentStore.Clear() => Clear();
        private readonly List<T> _data = new List<T>();
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly Dictionary<int, int> _entityToIndex = new Dictionary<int, int>();

        public int Count => _data.Count;

        /// <summary>底层数据数组（只读访问，系统批量处理时使用）。</summary>
        public IReadOnlyList<T> Data => _data;

        /// <summary>对应的实体列表。</summary>
        public IReadOnlyList<Entity> Entities => _entities;

        public bool Has(Entity entity)
        {
            return entity.IsValid && _entityToIndex.ContainsKey(entity.Id);
        }

        public bool TryGet(Entity entity, out T component)
        {
            component = default;
            if (!entity.IsValid || !_entityToIndex.TryGetValue(entity.Id, out int idx))
            {
                return false;
            }
            component = _data[idx];
            return true;
        }

        public T Get(Entity entity)
        {
            if (!_entityToIndex.TryGetValue(entity.Id, out int idx))
            {
                throw new KeyNotFoundException($"Entity {entity} 没有组件 {typeof(T).Name}");
            }
            return _data[idx];
        }

        /// <summary>按索引获取（系统批量遍历用）。</summary>
        public T GetAt(int index) => _data[index];

        /// <summary>按索引获取对应实体。</summary>
        public Entity GetEntityAt(int index) => _entities[index];

        public void Set(Entity entity, T component)
        {
            if (!_entityToIndex.TryGetValue(entity.Id, out int idx))
            {
                Add(entity, component);
                return;
            }
            _data[idx] = component;
        }

        public void Add(Entity entity, T component)
        {
            if (_entityToIndex.ContainsKey(entity.Id))
            {
                throw new InvalidOperationException($"Entity {entity} 已存在组件 {typeof(T).Name}");
            }
            int idx = _data.Count;
            _entityToIndex[entity.Id] = idx;
            _entities.Add(entity);
            _data.Add(component);
        }

        public bool Remove(Entity entity)
        {
            if (!_entityToIndex.TryGetValue(entity.Id, out int idx))
            {
                return false;
            }

            int last = _data.Count - 1;
            if (idx != last)
            {
                _data[idx] = _data[last];
                _entities[idx] = _entities[last];
                _entityToIndex[_entities[idx].Id] = idx;
            }

            _data.RemoveAt(last);
            _entities.RemoveAt(last);
            _entityToIndex.Remove(entity.Id);
            return true;
        }

        public void Clear()
        {
            _data.Clear();
            _entities.Clear();
            _entityToIndex.Clear();
        }

        /// <summary>
        /// 将数据复制到数组（供 Burst Job 使用）。
        /// </summary>
        public void CopyTo(T[] buffer, int count = -1)
        {
            int n = count < 0 ? _data.Count : Math.Min(count, _data.Count);
            for (int i = 0; i < n; i++) buffer[i] = _data[i];
        }
    }

    internal interface IComponentStore
    {
        void RemoveEntity(Entity entity);
        void Clear();
    }


}
