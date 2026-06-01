using System.Collections.Generic;
using UnityEngine;

namespace Functional.SoundManage
{
    /// <summary>
    /// AudioSource 对象池，用于 SFX 多实例播放。
    /// </summary>
    internal class AudioSourcePool
    {
        private readonly Transform _root;
        private readonly Queue<AudioSource> _pool = new Queue<AudioSource>();
        private readonly List<AudioSource> _active = new List<AudioSource>();

        public AudioSourcePool(Transform root) => _root = root;

        /// <summary>
        /// 获取一个空闲 AudioSource。
        /// </summary>
        public AudioSource Get()
        {
            AudioSource src;
            if (_pool.Count > 0)
            {
                src = _pool.Dequeue();
            }
            else
            {
                var go = new GameObject("SFX");
                go.transform.SetParent(_root);
                src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
            }
            src.gameObject.SetActive(true);
            _active.Add(src);
            return src;
        }

        /// <summary>
        /// 回收已播放完毕的 AudioSource。
        /// </summary>
        public void RecycleFinished()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var src = _active[i];
                if (!src.isPlaying)
                {
                    src.gameObject.SetActive(false);
                    _pool.Enqueue(src);
                    _active.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 停止并回收所有活跃音源。
        /// </summary>
        public void StopAll()
        {
            foreach (var src in _active)
            {
                src.Stop();
                src.gameObject.SetActive(false);
                _pool.Enqueue(src);
            }
            _active.Clear();
        }
    }
}
