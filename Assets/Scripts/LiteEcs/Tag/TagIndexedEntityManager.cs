using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 按指定的几个标签给在表实体建索引，按类型取实体时不用遍历全部实体。
    /// 只在实体进表时读一次 TagsComponent；进表之后再加减的标签不会更新索引，所以只适合放「实体类型」这类不变的标签。
    /// 用法：在 LWorld 子类里重写 CreateEntityManager，传入静态的标签数组。
    /// </summary>
    public class TagIndexedEntityManager : LEntityManager
    {
        private readonly string[] _indexTags;
        private readonly Dictionary<string, Dictionary<int, LEntity>> _entitiesByTag = new Dictionary<string, Dictionary<int, LEntity>>();
        private readonly Dictionary<int, List<string>> _entityTagKeys = new Dictionary<int, List<string>>();

        public TagIndexedEntityManager(LWorld world, IReadOnlyList<string> indexTags) : base(world)
        {
            _indexTags = new string[indexTags.Count];
            for (int i = 0; i < indexTags.Count; i++)
            {
                _indexTags[i] = indexTags[i];
            }
        }

        /// <returns>该标签下的在表实体，返回的是内部表，不要修改；没有实体时为 null</returns>
        public IReadOnlyDictionary<int, LEntity> GetEntitiesByTag(string tag)
        {
            return _entitiesByTag.TryGetValue(tag, out var bucket) ? bucket : null;
        }

        protected override void OnAddEntityInternal(LEntity entity)
        {
            var tags = entity.GetComponent<TagsComponent>();
            if (tags == null)
            {
                return;
            }
            List<string> keys = null;
            for (int i = 0; i < _indexTags.Length; i++)
            {
                var tag = _indexTags[i];
                if (!tags.HasTag(tag))
                {
                    continue;
                }
                if (!_entitiesByTag.TryGetValue(tag, out var bucket))
                {
                    bucket = new Dictionary<int, LEntity>();
                    _entitiesByTag.Add(tag, bucket);
                }
                bucket[entity.Id] = entity;
                if (keys == null)
                {
                    keys = new List<string>(1);
                }
                keys.Add(tag);
            }
            if (keys != null)
            {
                _entityTagKeys[entity.Id] = keys;
            }
        }

        protected override void OnRemoveEntityInternal(LEntity entity)
        {
            if (!_entityTagKeys.TryGetValue(entity.Id, out var keys))
            {
                return;
            }
            _entityTagKeys.Remove(entity.Id);
            for (int i = 0; i < keys.Count; i++)
            {
                if (!_entitiesByTag.TryGetValue(keys[i], out var bucket))
                {
                    continue;
                }
                bucket.Remove(entity.Id);
                if (bucket.Count == 0)
                {
                    _entitiesByTag.Remove(keys[i]);
                }
            }
        }

        protected override void OnDispose()
        {
            _entitiesByTag.Clear();
            _entityTagKeys.Clear();
        }
    }
}
