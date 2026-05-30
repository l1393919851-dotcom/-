using UnityEngine;
using UnityEngine.UI;
using Base.Common;

namespace Base.UIFramework.Input
{
    /// <summary>
    /// 输入屏蔽器。在动画播放、网络等待时屏蔽全屏点击，避免重复操作。
    /// 内部使用一张透明的全屏 Image 拦截事件，挂在 UI 的 Top 层。
    /// 
    /// 用法：
    ///   InputBlocker.Instance.Block();   // 屏蔽（可多次嵌套）
    ///   InputBlocker.Instance.Unblock(); // 解除一次
    ///   InputBlocker.Instance.ForceUnblock(); // 强制清空所有屏蔽计数
    /// </summary>
    public class InputBlocker : MonoSingleton<InputBlocker>
    {
        private GameObject _mask;
        private int _refCount;

        /// <summary>
        /// 当前是否处于屏蔽状态。
        /// </summary>
        public bool IsBlocking => _refCount > 0;

        /// <summary>
        /// 请求一次屏蔽。
        /// </summary>
        public void Block()
        {
            EnsureMask();
            _refCount++;
            _mask.SetActive(true);
        }

        /// <summary>
        /// 解除一次屏蔽。计数归零后隐藏遮罩。
        /// </summary>
        public void Unblock()
        {
            if (_refCount > 0) _refCount--;
            if (_refCount == 0 && _mask != null) _mask.SetActive(false);
        }

        /// <summary>
        /// 强制清空所有屏蔽（紧急情况下使用，如崩溃后恢复）。
        /// </summary>
        public void ForceUnblock()
        {
            _refCount = 0;
            if (_mask != null) _mask.SetActive(false);
        }

        /// <summary>
        /// 创建（或复用）遮罩 GameObject，挂在 UI 的 Top 层。
        /// </summary>
        private void EnsureMask()
        {
            if (_mask != null) return;
            var layer = UIManager.Instance.Root.GetLayer(UILayer.Top);
            _mask = new GameObject("[InputBlocker]", typeof(RectTransform), typeof(Image));
            _mask.transform.SetParent(layer, false);
            var rt = _mask.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = _mask.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0f);
            img.raycastTarget = true;
            _mask.SetActive(false);
        }
    }
}
