using System.Collections.Generic;
using UnityEngine;

namespace Functional.BehaviorTree
{
    /// <summary>
    /// 行为树运行时上下文。可挂载黑板数据与 Agent 引用。
    /// </summary>
    public class BTContext
    {
        /// <summary>行为树绑定的 GameObject（AI 实体）。</summary>
        public GameObject Agent { get; set; }

        /// <summary>黑板：跨节点共享的键值数据。</summary>
        public Dictionary<string, object> Blackboard { get; } = new Dictionary<string, object>();

        /// <summary>增量时间（由 Runner 每帧写入）。</summary>
        public float DeltaTime { get; set; }

        /// <summary>
        /// 从黑板取值。
        /// </summary>
        public T Get<T>(string key, T defaultValue = default)
        {
            if (Blackboard.TryGetValue(key, out var val) && val is T t) return t;
            return defaultValue;
        }

        /// <summary>
        /// 写入黑板。
        /// </summary>
        public void Set<T>(string key, T value) => Blackboard[key] = value;
    }
}
