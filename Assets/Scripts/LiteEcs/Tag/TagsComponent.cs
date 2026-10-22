using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 创建参数里带初始标签时实现这个接口，TagsComponent.Init 会读取。
    /// </summary>
    public interface ITagsCreateParam
    {
        IReadOnlyList<string> InitialTags { get; }
    }

    /// <summary>
    /// 实体标签组件，包一层 LTagContainer。实体类型（如 pet / building）、运行时状态（如 focused）都可以用标签表达。
    /// </summary>
    public class TagsComponent : LComponent
    {
        private readonly LTagContainer _tags = new LTagContainer();

        public override void Init(object createParam)
        {
            if (createParam is ITagsCreateParam param)
            {
                _tags.AddTags(param.InitialTags);
            }
        }

        public void AddTag(string tag)
        {
            _tags.Add(tag);
        }

        /// <summary>
        /// 引用计数减一，到 0 才真正去掉。
        /// </summary>
        public void RemoveTag(string tag)
        {
            _tags.Remove(tag);
        }

        public bool HasTag(string tag)
        {
            return _tags.Has(tag);
        }

        public bool HasAnyTag(IReadOnlyList<string> tags)
        {
            return _tags.HasAny(tags);
        }

        public bool HasAllTags(IReadOnlyList<string> tags)
        {
            return _tags.HasAll(tags);
        }

        public override void OnRecycle()
        {
            _tags.ClearAll();
            base.OnRecycle();
        }
    }
}
