using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIFramework.Widgets
{
    /// <summary>
    /// 循环滚动列表。通过复用少量 cell 来显示大量数据，避免大列表性能问题。
    /// 当前为垂直方向、单列布局实现。如需横向/多列，可扩展。
    /// 
    /// 用法：
    ///   挂在 ScrollView 的 Content 上 → 设置 itemPrefab → 调用 SetData(count, OnUpdate)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class LoopScrollView : MonoBehaviour
    {
        /// <summary>列表项预制体。必须是 RectTransform，高度由 ItemHeight 决定。</summary>
        public RectTransform ItemPrefab;

        /// <summary>每个 cell 的高度（像素）。</summary>
        public float ItemHeight = 100f;

        /// <summary>cell 之间的间距。</summary>
        public float Spacing = 10f;

        /// <summary>顶部和底部的内边距。</summary>
        public float PaddingTop = 0f;
        public float PaddingBottom = 0f;

        private ScrollRect _scrollRect;
        private RectTransform _content;
        private RectTransform _viewport;
        private readonly List<RectTransform> _cells = new List<RectTransform>();
        private readonly List<int> _cellIndices = new List<int>();
        private int _totalCount;
        private Action<int, RectTransform> _onUpdateCell;

        /// <summary>
        /// 设置数据：项总数 + 刷新单项的回调。
        /// </summary>
        /// <param name="count">数据总条数。</param>
        /// <param name="onUpdate">回调（index, cell）：把数据写入 cell 显示。</param>
        public void SetData(int count, Action<int, RectTransform> onUpdate)
        {
            _totalCount = count;
            _onUpdateCell = onUpdate;
            EnsureScrollRect();
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, ComputeContentHeight());
            _content.anchoredPosition = Vector2.zero;
            BuildCellsIfNeeded();
            Refresh();
        }

        /// <summary>
        /// 强制刷新当前可见的 cell（数据发生局部变化时调用）。
        /// </summary>
        public void Refresh()
        {
            if (_onUpdateCell == null) return;
            float scrollY = -_content.anchoredPosition.y;
            int firstIndex = Mathf.FloorToInt((scrollY - PaddingTop) / (ItemHeight + Spacing));
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(_totalCount - _cells.Count, 0));
            for (int i = 0; i < _cells.Count; i++)
            {
                int dataIndex = firstIndex + i;
                _cellIndices[i] = dataIndex;
                if (dataIndex < 0 || dataIndex >= _totalCount)
                {
                    _cells[i].gameObject.SetActive(false);
                    continue;
                }
                _cells[i].gameObject.SetActive(true);
                _cells[i].anchoredPosition = new Vector2(0, -(PaddingTop + dataIndex * (ItemHeight + Spacing)));
                try { _onUpdateCell(dataIndex, _cells[i]); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 滚动到指定下标位置。
        /// </summary>
        public void ScrollTo(int index)
        {
            index = Mathf.Clamp(index, 0, _totalCount - 1);
            float y = PaddingTop + index * (ItemHeight + Spacing);
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, y);
            Refresh();
        }

        /// <summary>
        /// 计算 Content 总高度。
        /// </summary>
        private float ComputeContentHeight()
        {
            if (_totalCount <= 0) return 0;
            return PaddingTop + PaddingBottom + _totalCount * ItemHeight + (_totalCount - 1) * Spacing;
        }

        /// <summary>
        /// 拿到 ScrollRect 引用并监听 OnValueChanged。
        /// </summary>
        private void EnsureScrollRect()
        {
            if (_scrollRect != null) return;
            _scrollRect = GetComponentInParent<ScrollRect>();
            if (_scrollRect == null)
            {
                UnityEngine.Debug.LogError("[LoopScrollView] 必须挂在 ScrollRect 的 content 下");
                return;
            }
            _content = _scrollRect.content;
            _viewport = _scrollRect.viewport != null ? _scrollRect.viewport : _scrollRect.transform as RectTransform;
            _scrollRect.onValueChanged.AddListener(_ => Refresh());

            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1);
        }

        /// <summary>
        /// 根据可视区域计算需要的 cell 数量，按需实例化。
        /// </summary>
        private void BuildCellsIfNeeded()
        {
            if (ItemPrefab == null) return;
            float viewportHeight = _viewport.rect.height;
            int needCount = Mathf.CeilToInt(viewportHeight / (ItemHeight + Spacing)) + 2;
            while (_cells.Count < needCount)
            {
                var cell = Instantiate(ItemPrefab, _content);
                cell.anchorMin = new Vector2(0, 1);
                cell.anchorMax = new Vector2(1, 1);
                cell.pivot = new Vector2(0.5f, 1);
                cell.sizeDelta = new Vector2(0, ItemHeight);
                cell.gameObject.SetActive(true);
                _cells.Add(cell);
                _cellIndices.Add(-1);
            }
        }
    }
}
