using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 实体管理：增删都先排队，在 Tick 里统一生效。每帧顺序：
    /// 1. 本帧待加入的实体全部进表并 Awake，再全部 Start；
    /// 2. 所有在表实体 Tick；
    /// 3. 本帧待移除的实体全部 End，再逐个 Destroy、触发 OnRemoveEntityInternal、回池。
    /// 第 1、3 步执行期间新排队的增删留到下一帧。每个实体的回调单独 try/catch，一个出错不影响其他实体。
    /// </summary>
    public class LEntityManager
    {
        public int EntityCount => _entities.Count;
        public IReadOnlyDictionary<int, LEntity> Entities => _entities;

        private readonly Dictionary<int, LEntity> _entities = new Dictionary<int, LEntity>();
        private readonly List<LEntity> _entityList = new List<LEntity>();
        private readonly Dictionary<int, LEntity> _pendingAdd = new Dictionary<int, LEntity>();
        private List<LEntity> _pendingAddList = new List<LEntity>();
        private List<LEntity> _flushAddList = new List<LEntity>();
        private readonly HashSet<int> _pendingRemove = new HashSet<int>();
        private List<int> _pendingRemoveList = new List<int>();
        private List<int> _flushRemoveList = new List<int>();

        public LWorld World { get; }

        public LEntityManager(LWorld world)
        {
            World = world;
        }

        public LEntity GetEntity(int id)
        {
            return _entities.TryGetValue(id, out var entity) ? entity : null;
        }

        /// <summary>
        /// 先查在表实体，再查本帧刚加、还没进表的实体。
        /// </summary>
        public LEntity GetEntityIncludingPending(int id)
        {
            if (_entities.TryGetValue(id, out var entity))
            {
                return entity;
            }
            return _pendingAdd.TryGetValue(id, out entity) ? entity : null;
        }

        public bool IsPendingAdd(int id)
        {
            return _pendingAdd.ContainsKey(id);
        }

        public void AddEntity(LEntity entity)
        {
            if (_entities.ContainsKey(entity.Id) || _pendingAdd.ContainsKey(entity.Id))
            {
                return;
            }
            _pendingAdd.Add(entity.Id, entity);
            _pendingAddList.Add(entity);
        }

        public void RemoveEntity(int id)
        {
            if (!_pendingRemove.Add(id))
            {
                return;
            }
            _pendingRemoveList.Add(id);
        }

        public void Tick(float dt)
        {
            FlushAdd();
            int count = _entityList.Count;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    _entityList[i].Tick(dt);
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            FlushRemove();
            OnTick(dt);
        }

        /// <summary>
        /// 立刻结束并回收所有实体，丢弃排队中的增删。World 销毁时调用。
        /// </summary>
        public void ClearEntity()
        {
            for (int i = 0; i < _entityList.Count; i++)
            {
                var entity = _entityList[i];
                try
                {
                    entity.OnEntityEnd();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
                try
                {
                    entity.OnEntityDestroy();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
                try
                {
                    OnRemoveEntityInternal(entity);
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < _entityList.Count; i++)
            {
                World.ObjectPoolManager.Recycle(_entityList[i]);
            }
            _entityList.Clear();
            _entities.Clear();
            _pendingAdd.Clear();
            _pendingAddList.Clear();
            _flushAddList.Clear();
            _pendingRemove.Clear();
            _pendingRemoveList.Clear();
            _flushRemoveList.Clear();
        }

        public void Dispose()
        {
            ClearEntity();
            OnDispose();
        }

        protected virtual void OnAddEntityInternal(LEntity entity)
        {
        }

        /// <summary>
        /// 实体 Destroy 之后、离表和回池之前调用，此时 entity.Id 和组件都还在。
        /// </summary>
        protected virtual void OnRemoveEntityInternal(LEntity entity)
        {
        }

        protected virtual void OnTick(float dt)
        {
        }

        protected virtual void OnDispose()
        {
        }

        private void FlushAdd()
        {
            if (_pendingAddList.Count == 0)
            {
                return;
            }
            var list = _pendingAddList;
            _pendingAddList = _flushAddList;
            _flushAddList = list;

            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
                if (!AddEntityInternal(entity))
                {
                    list[i] = null;
                    continue;
                }
                try
                {
                    entity.OnEntityAwake();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
                if (entity == null)
                {
                    continue;
                }
                try
                {
                    entity.OnEntityStart();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    _pendingAdd.Remove(list[i].Id);
                }
            }
            list.Clear();
        }

        private bool AddEntityInternal(LEntity entity)
        {
            if (_entities.ContainsKey(entity.Id))
            {
                _pendingAdd.Remove(entity.Id);
                return false;
            }
            _entities.Add(entity.Id, entity);
            _entityList.Add(entity);
            try
            {
                OnAddEntityInternal(entity);
            }
            catch (Exception ex)
            {
                EcsLog.LogException(ex);
            }
            return true;
        }

        private void FlushRemove()
        {
            if (_pendingRemoveList.Count == 0)
            {
                return;
            }
            var list = _pendingRemoveList;
            _pendingRemoveList = _flushRemoveList;
            _flushRemoveList = list;
            for (int i = 0; i < list.Count; i++)
            {
                _pendingRemove.Remove(list[i]);
            }

            for (int i = 0; i < list.Count; i++)
            {
                var entity = GetEntity(list[i]);
                if (entity == null)
                {
                    continue;
                }
                try
                {
                    entity.OnEntityEnd();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                RemoveEntityInternal(list[i]);
            }
            list.Clear();
        }

        private void RemoveEntityInternal(int id)
        {
            if (!_entities.TryGetValue(id, out var entity))
            {
                return;
            }
            try
            {
                entity.OnEntityDestroy();
            }
            catch (Exception ex)
            {
                EcsLog.LogException(ex);
            }
            try
            {
                OnRemoveEntityInternal(entity);
            }
            catch (Exception ex)
            {
                EcsLog.LogException(ex);
            }
            _entities.Remove(id);
            _entityList.Remove(entity);
            World.ObjectPoolManager.Recycle(entity);
        }
    }
}
