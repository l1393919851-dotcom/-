using System;

namespace Functional.ECS
{
    /// <summary>
    /// 实体标识。仅包含 Id，组件数据存放在 World 的 ComponentStore 中。
    /// </summary>
    public readonly struct Entity : IEquatable<Entity>
    {
        public static readonly Entity Null = new Entity(-1);

        public int Id { get; }
        public int Version { get; }

        public bool IsValid => Id >= 0;

        public Entity(int id, int version = 0)
        {
            Id = id;
            Version = version;
        }

        public bool Equals(Entity other) => Id == other.Id && Version == other.Version;
        public override bool Equals(object obj) => obj is Entity e && Equals(e);
        public override int GetHashCode() => Id * 397 ^ Version;
        public static bool operator ==(Entity a, Entity b) => a.Equals(b);
        public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
        public override string ToString() => $"Entity({Id}, v{Version})";
    }
}
