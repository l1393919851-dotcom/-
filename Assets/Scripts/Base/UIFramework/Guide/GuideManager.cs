using System;
using System.Collections.Generic;
using UnityEngine;
using Base.Common;

namespace Base.UIFramework.Guide
{
    /// <summary>
    /// 引导步骤数据。
    /// </summary>
    public class GuideStep
    {
        /// <summary>步骤 id。</summary>
        public int Id;
        /// <summary>高亮目标 RectTransform。</summary>
        public RectTransform Target;
        /// <summary>提示文本。</summary>
        public string Tip;
        /// <summary>镂空区域的内边距。</summary>
        public float Padding = 10f;
        /// <summary>点击高亮区域后的回调（一般用于推进到下一步）。</summary>
        public Action OnTargetClick;
    }

    /// <summary>
    /// 引导管理器。
    /// - 按队列推进引导步骤
    /// - 每步用 GuideMask 高亮一个目标
    /// - 玩家点击目标后自动推进到下一步
    /// 
    /// 用法：
    ///   var steps = new List<GuideStep> { new GuideStep{ ... }, ... };
    ///   GuideManager.Instance.StartGuide(steps);
    /// </summary>
    public class GuideManager : MonoSingleton<GuideManager>
    {
        private GuideMask _mask;
        private GameObject _guideRoot;
        private Queue<GuideStep> _queue;
        private GuideStep _current;

        /// <summary>当前是否在引导中。</summary>
        public bool IsGuiding => _current != null;

        /// <summary>
        /// 开始一段引导。
        /// </summary>
        public void StartGuide(List<GuideStep> steps)
        {
            if (steps == null || steps.Count == 0) return;
            EnsureMask();
            _queue = new Queue<GuideStep>(steps);
            NextStep();
        }

        /// <summary>
        /// 强制结束当前引导。
        /// </summary>
        public void StopGuide()
        {
            _queue?.Clear();
            _current = null;
            if (_guideRoot != null) _guideRoot.SetActive(false);
        }

        /// <summary>
        /// 推进到下一步引导。
        /// </summary>
        public void NextStep()
        {
            if (_queue == null || _queue.Count == 0)
            {
                StopGuide();
                return;
            }
            _current = _queue.Dequeue();
            _guideRoot.SetActive(true);
            _mask.SetHole(_current.Target, _current.Padding);
        }

        /// <summary>
        /// 创建（或复用）引导遮罩节点，挂到 Top 层。
        /// </summary>
        private void EnsureMask()
        {
            if (_mask != null) return;
            var layer = UIManager.Instance.Root.GetLayer(UILayer.Top);
            _guideRoot = new GameObject("[Guide]", typeof(RectTransform));
            _guideRoot.transform.SetParent(layer, false);
            var rt = _guideRoot.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _mask = _guideRoot.AddComponent<GuideMask>();
            _mask.raycastTarget = true;
            _guideRoot.SetActive(false);
        }

        /// <summary>
        /// 提供给业务调用：在目标按钮被点击时调用，推进引导并触发回调。
        /// </summary>
        public void OnTargetClicked()
        {
            if (_current == null) return;
            try { _current.OnTargetClick?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
            NextStep();
        }
    }
}
