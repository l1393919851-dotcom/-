using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Base.UIFramework.Guide
{
    /// <summary>
    /// 引导遮罩。带镂空的半透明黑色遮罩。
    /// 玩家只能点击到"洞"里的目标，其他区域被屏蔽。
    /// 实现方式：使用 UI 自定义 raycastTarget，把目标 RectTransform 的世界矩形挖空。
    /// </summary>
    public class GuideMask : MaskableGraphic, ICanvasRaycastFilter
    {
        /// <summary>背景颜色（一般半透明黑）。</summary>
        public Color MaskColor = new Color(0, 0, 0, 0.7f);

        /// <summary>镂空区域的世界矩形（由 GuideManager 计算并传入）。</summary>
        private Rect _holeWorldRect;

        private bool _hasHole;

        /// <summary>
        /// 设置镂空目标。镂空区域内的点击会"穿透"到下层，区域外则被遮罩拦截。
        /// </summary>
        public void SetHole(RectTransform target, float padding = 0f)
        {
            if (target == null)
            {
                _hasHole = false;
                SetVerticesDirty();
                return;
            }
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var min = corners[0];
            var max = corners[2];
            _holeWorldRect = new Rect(min.x - padding, min.y - padding,
                (max.x - min.x) + padding * 2, (max.y - min.y) + padding * 2);
            _hasHole = true;
            SetVerticesDirty();
        }

        /// <summary>
        /// 关闭镂空：恢复全屏遮罩。
        /// </summary>
        public void ClearHole()
        {
            _hasHole = false;
            SetVerticesDirty();
        }

        /// <summary>
        /// 重写 raycast：洞内不响应（让事件传到下面的真实按钮）。
        /// </summary>
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!_hasHole) return true;
            return !_holeWorldRect.Contains(screenPoint);
        }

        /// <summary>
        /// 重写网格生成：画出"四块边框组成的遮罩"，中间留洞。
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            color = MaskColor;
            var canvasRect = rectTransform.rect;
            var canvasMin = (Vector2)rectTransform.TransformPoint(new Vector2(canvasRect.xMin, canvasRect.yMin));
            var canvasMax = (Vector2)rectTransform.TransformPoint(new Vector2(canvasRect.xMax, canvasRect.yMax));

            if (!_hasHole)
            {
                AddQuad(vh, canvasMin, canvasMax);
                return;
            }
            var holeMin = new Vector2(
                Mathf.Max(_holeWorldRect.xMin, canvasMin.x),
                Mathf.Max(_holeWorldRect.yMin, canvasMin.y));
            var holeMax = new Vector2(
                Mathf.Min(_holeWorldRect.xMax, canvasMax.x),
                Mathf.Min(_holeWorldRect.yMax, canvasMax.y));

            AddQuad(vh, new Vector2(canvasMin.x, canvasMin.y), new Vector2(canvasMax.x, holeMin.y));
            AddQuad(vh, new Vector2(canvasMin.x, holeMax.y), new Vector2(canvasMax.x, canvasMax.y));
            AddQuad(vh, new Vector2(canvasMin.x, holeMin.y), new Vector2(holeMin.x, holeMax.y));
            AddQuad(vh, new Vector2(holeMax.x, holeMin.y), new Vector2(canvasMax.x, holeMax.y));
        }

        /// <summary>
        /// 把世界坐标的矩形转成本地顶点并加入网格。
        /// </summary>
        private void AddQuad(VertexHelper vh, Vector2 worldMin, Vector2 worldMax)
        {
            if (worldMin.x >= worldMax.x || worldMin.y >= worldMax.y) return;
            var lMin = rectTransform.InverseTransformPoint(worldMin);
            var lMax = rectTransform.InverseTransformPoint(worldMax);
            int startIdx = vh.currentVertCount;
            vh.AddVert(new Vector3(lMin.x, lMin.y), color, Vector2.zero);
            vh.AddVert(new Vector3(lMin.x, lMax.y), color, Vector2.up);
            vh.AddVert(new Vector3(lMax.x, lMax.y), color, Vector2.one);
            vh.AddVert(new Vector3(lMax.x, lMin.y), color, Vector2.right);
            vh.AddTriangle(startIdx, startIdx + 1, startIdx + 2);
            vh.AddTriangle(startIdx, startIdx + 2, startIdx + 3);
        }
    }
}
