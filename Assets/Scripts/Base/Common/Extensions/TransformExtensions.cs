using UnityEngine;

namespace Base.Common
{
    /// <summary>
    /// Transform / RectTransform 常用扩展方法，简化 UI 操作。
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// 重置 Transform 的本地位置、旋转、缩放为默认值（位置零，旋转零，缩放一）。
        /// </summary>
        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// 将 RectTransform 填满父节点（anchor 全 0-1，offset 全 0）。
        /// 常用于把弹窗、遮罩等铺满整个屏幕。
        /// </summary>
        public static void StretchFull(this RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        /// <summary>
        /// 递归查找子节点（支持多层嵌套，按名字匹配）。
        /// </summary>
        public static Transform FindDeep(this Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var result = parent.GetChild(i).FindDeep(name);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>
        /// 销毁所有子节点（运行时使用 Destroy，编辑器下使用 DestroyImmediate）。
        /// </summary>
        public static void DestroyAllChildren(this Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i).gameObject;
                if (Application.isPlaying) Object.Destroy(child);
                else Object.DestroyImmediate(child);
            }
        }
    }
}
