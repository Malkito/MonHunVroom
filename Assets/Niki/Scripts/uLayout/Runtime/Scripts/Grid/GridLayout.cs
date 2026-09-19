/*
    Copyright (c) 2026 Nikita Mitrovich

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ChildInfo = CupOHappiness.UI.LayoutUtil.ChildInfo;

namespace CupOHappiness.UI
{
    /// <summary>
    /// Defines how GridLayout creates columns or rows.
    /// </summary>
    public enum GridTrackMode
    {
        /// <summary>
        /// Uses a fixed number of tracks. Empty tracks stay visible.
        /// </summary>
        FixedCount,
        /// <summary>
        /// Packs as many tracks as fit, then collapses tracks that hold no item.
        /// </summary>
        AutoFit,
        /// <summary>
        /// Packs as many tracks as fit and keeps the empty tracks.
        /// </summary>
        AutoFill
    }

    /// <summary>
    /// Settings for one set of grid columns or rows.
    /// </summary>
    [System.Serializable]
    public struct GridTrackDef
    {
        [Tooltip("How this grid makes columns or rows.")]
        public GridTrackMode mode;
        [Tooltip("Track count used when Mode is Fixed Count.")]
        public int count;
        [Tooltip("Smallest allowed column or row size, in pixels.")]
        public float minTrackSize;
        [Tooltip("Set to 0 to keep tracks at their minimum size. Set above 0 to stretch tracks evenly and use free space.")]
        [FormerlySerializedAs("maxFr")]
        public float fillExtraSpace;

        public static bool operator ==(GridTrackDef a, GridTrackDef b) {
            return a.mode == b.mode
                && a.count == b.count
                && Mathf.Approximately(a.minTrackSize, b.minTrackSize)
                && Mathf.Approximately(a.fillExtraSpace, b.fillExtraSpace);
        }

        public static bool operator !=(GridTrackDef a, GridTrackDef b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            return obj is GridTrackDef other && this == other;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = mode.ToString().GetHashCode();
                hash = hash * 31 + count;
                hash = hash * 31 + minTrackSize.GetHashCode();
                hash = hash * 31 + fillExtraSpace.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// Places direct children in a two dimensional grid of tracks.
    /// Columns come from the column track line and rows follow item flow,
    /// while every child keeps its uLayout sizing and margin rules.
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class GridLayout : LayoutItem, ILayoutGroup, IuLayoutContainer
    {
        [Header("Grid Tracks")]
        [Tooltip("Settings for grid columns.")]
        [SerializeField] private GridTrackDef m_columnTracks = new()
        {
            mode = GridTrackMode.AutoFit,
            count = 3,
            minTrackSize = 120f,
            fillExtraSpace = 1f
        };

        [Tooltip("Settings for grid rows. Fixed Count preserves empty rows and adds rows when child flow needs them.")]
        [SerializeField] private GridTrackDef m_rowTracks = new()
        {
            mode = GridTrackMode.FixedCount,
            count = 1,
            minTrackSize = 120f,
            fillExtraSpace = 1f
        };

        [Header("Spacing & Padding")]
        [Tooltip("Inner space between the grid edge and its tracks.")]
        [SerializeField] private Margins m_padding;
        [Tooltip("Gap between tracks. X is the column gap, Y is the row gap.")]
        [SerializeField] private Vector2 m_gap;

        [Header("Child Alignment")]
        [Tooltip("Horizontal position for children that do not fill their grid cell.")]
        [SerializeField] private Layout.Alignment m_childAlignmentX = Layout.Alignment.Start;
        [Tooltip("Vertical position for children that do not fill their grid cell.")]
        [SerializeField] private Layout.Alignment m_childAlignmentY = Layout.Alignment.Start;

        private readonly List<ChildInfo> _gridChildren = new();
        private int _resolvedColumns = 1;
        private int _resolvedRows = 1;
        private float _columnWidth;
        private float _rowHeight;
        private GridTrackPlan _columnPlan;
        private GridTrackPlan _rowPlan;
        private Vector2 _fitContentSize;

        /// <summary>
        /// Column count resolved for the current children and sizing modes.
        /// </summary>
        public int ResolvedColumns => _resolvedColumns;

        /// <summary>
        /// Row count resolved for the current children and sizing modes.
        /// </summary>
        public int ResolvedRows => _resolvedRows;

        /// <summary>
        /// Children placed in the grid, excluding floating and ignored children.
        /// </summary>
        public int ActiveChildCount => _gridChildren.Count;

        /// <summary>
        /// Resolved column track width.
        /// </summary>
        public float ColumnWidth => _columnWidth;

        /// <summary>
        /// Resolved row track height.
        /// </summary>
        public float RowHeight => _rowHeight;

        public GridTrackDef ColumnTracks {
            get => m_columnTracks;
            set { if(m_columnTracks != value) { m_columnTracks = value; SetDirty(); } }
        }
        public GridTrackDef RowTracks {
            get => m_rowTracks;
            set { if(m_rowTracks != value) { m_rowTracks = value; SetDirty(); } }
        }
        public Margins Padding {
            get => m_padding;
            set {
                bool same = m_padding.left == value.left
                    && m_padding.right == value.right
                    && m_padding.top == value.top
                    && m_padding.bottom == value.bottom;
                if(!same) {
                    m_padding = value;
                    SetDirty();
                }
            }
        }
        public Vector2 Gap {
            get => m_gap;
            set { Vector2 next = MaxVector(Vector2.zero, value); if(m_gap != next) { m_gap = next; SetDirty(); } }
        }
        public Layout.Alignment ChildAlignmentX {
            get => m_childAlignmentX;
            set { if(m_childAlignmentX != value) { m_childAlignmentX = value; SetDirty(); } }
        }
        public Layout.Alignment ChildAlignmentY {
            get => m_childAlignmentY;
            set { if(m_childAlignmentY != value) { m_childAlignmentY = value; SetDirty(); } }
        }

        protected override void OnEnable() {
            base.OnEnable();
            RefreshChildCache();
        }

        protected override void OnDisable() {
            _gridChildren.Clear();
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            if(ChildCacheChanged())
                RefreshChildCache();
        }

        protected override void OnValidate() {
            SanitizeGridConfiguration();
            base.OnValidate();
        }

        private void OnTransformChildrenChanged() {
            RefreshChildCache();
        }

        public override void CalculateLayoutInputHorizontal() {
            if(!_dirty)
                return;

            RefreshChildren();
            ResolveTrackPlans();

            _fitContentSize.x = m_padding.left + m_padding.right + _columnPlan.TotalSize;
            UpdateLayoutElementValues();
        }

        public override void CalculateLayoutInputVertical() {
            if(!_dirty)
                return;

            ResolveTrackPlans();

            _fitContentSize.y = m_padding.top + m_padding.bottom + _rowPlan.TotalSize;
            UpdateLayoutElementValues();
        }

        public void SetLayoutHorizontal() {
            if(!_dirty)
                return;

            if(m_sizing.x == SizingMode.FitContent) {
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ClampSize(RectTransform.Axis.Horizontal, _fitContentSize.x));
                ResolveTrackPlans();
            }

            for(int i = 0; i < _gridChildren.Count; i++) {
                ChildInfo child = _gridChildren[i];
                int column = i % _columnPlan.Count;
                Margins margin = child.item ? child.item.Margin : default;
                float size = ResolveChildSize(child, RectTransform.Axis.Horizontal);
                float position = m_padding.left + _columnPlan.Positions[column] + margin.left
                    + ResolveAlignmentOffset(_columnWidth, margin.left, margin.right, size, m_childAlignmentX);

                SetChildHorizontal(child.rect, position, size);
            }
        }

        public void SetLayoutVertical() {
            if(_dirty) {
                if(m_sizing.y == SizingMode.FitContent) {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ClampSize(RectTransform.Axis.Vertical, _fitContentSize.y));
                    ResolveTrackPlans();
                }

                for(int i = 0; i < _gridChildren.Count; i++) {
                    ChildInfo child = _gridChildren[i];
                    int row = i / _columnPlan.Count;
                    Margins margin = child.item ? child.item.Margin : default;
                    float size = ResolveChildSize(child, RectTransform.Axis.Vertical);
                    float position = m_padding.top + _rowPlan.Positions[row] + margin.top
                        + ResolveAlignmentOffset(_rowHeight, margin.top, margin.bottom, size, m_childAlignmentY);

                    SetChildVertical(child.rect, position, size);
                }
            }

            _dirty = false;
        }

        public void RefreshChildCache() {
            RefreshChildren();
            SetDirty();
        }

        private void RefreshChildren() {
            LayoutUtil.RefreshChildCache(transform, _gridChildren, ignoreScale: false);
            _gridChildren.RemoveAll(LayoutUtil.IsIgnored);
        }

        private bool ChildCacheChanged() {
            int activeIndex = 0;
            for(int i = 0; i < transform.childCount; i++) {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if(!child || LayoutUtil.ShouldIgnoreLayout(child, child.GetComponent<LayoutItem>()))
                    continue;

                if(activeIndex >= _gridChildren.Count || _gridChildren[activeIndex].rect != child)
                    return true;
                activeIndex++;
            }

            return activeIndex != _gridChildren.Count;
        }

        private void ResolveTrackPlans() {
            int count = _gridChildren.Count;
            float availableWidth = _rect.rect.width - m_padding.left - m_padding.right;
            float availableHeight = _rect.rect.height - m_padding.top - m_padding.bottom;
            bool fitWidth = m_sizing.x == SizingMode.FitContent;
            bool fitHeight = m_sizing.y == SizingMode.FitContent;

            m_columnTracks = GridTrackResolver.Sanitize(m_columnTracks);
            m_rowTracks = GridTrackResolver.Sanitize(m_rowTracks);
            _columnPlan = GridTrackResolver.Resolve(m_columnTracks, availableWidth, m_gap.x, count, fitWidth);
            int rowDemand = count == 0 ? 0 : Mathf.CeilToInt(count / (float)_columnPlan.Count);
            GridTrackDef rowTracks = m_rowTracks;
            // Columns define the fixed wrapping count. Rows retain their
            // authored empty tracks, then extend to accommodate child flow.
            // Without this, children past a fixed row count index past the
            // resolved row plan during vertical placement.
            if(rowTracks.mode == GridTrackMode.FixedCount)
                rowTracks.count = Mathf.Max(rowTracks.count, rowDemand);
            _rowPlan = GridTrackResolver.Resolve(rowTracks, availableHeight, m_gap.y, rowDemand, fitHeight);

            _resolvedColumns = _columnPlan.Count;
            _resolvedRows = _rowPlan.Count;
            _columnWidth = _columnPlan.UniformSize;
            _rowHeight = _rowPlan.UniformSize;
        }

        /// <summary>
        /// Resolves a child's size on one axis against its track.
        /// Grow and Percent children use the uLayout sizing rules against the
        /// track size minus their own margins. Fixed and FitContent children
        /// keep their own size.
        /// </summary>
        private float ResolveChildSize(ChildInfo child, RectTransform.Axis axis) {
            LayoutItem item = child.item;
            if(!item)
                return axis == RectTransform.Axis.Horizontal ? child.rect.rect.size.x : child.rect.rect.size.y;

            float available = axis == RectTransform.Axis.Horizontal
                ? Mathf.Max(0, _columnWidth - child.margins.left - child.margins.right)
                : Mathf.Max(0, _rowHeight - child.margins.top - child.margins.bottom);

            return LayoutUtil.ResolveChildSize(child, axis, available, ignoreScale: false);
        }

        private static float ResolveAlignmentOffset(
            float trackSize,
            float leadingMargin,
            float trailingMargin,
            float childSize,
            Layout.Alignment alignment) {
            float freeSpace = Mathf.Max(0, trackSize - leadingMargin - trailingMargin - childSize);
            return alignment switch {
                Layout.Alignment.Center => freeSpace * 0.5f,
                Layout.Alignment.End => freeSpace,
                _ => 0,
            };
        }

        private static void SetChildHorizontal(RectTransform child, float position, float size) {
            child.anchorMin = child.anchorMin.SetX(0);
            child.anchorMax = child.anchorMax.SetX(0);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            child.anchoredPosition = child.anchoredPosition.SetX(position + size * child.pivot.x);
        }

        private static void SetChildVertical(RectTransform child, float position, float size) {
            child.anchorMin = child.anchorMin.SetY(1);
            child.anchorMax = child.anchorMax.SetY(1);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            child.anchoredPosition = child.anchoredPosition.SetY(-position - size * (1 - child.pivot.y));
        }

        private void UpdateLayoutElementValues() {
            float minimumWidth = m_padding.left + m_padding.right
                + _resolvedColumns * Mathf.Max(0, m_columnTracks.minTrackSize)
                + Mathf.Max(0, _resolvedColumns - 1) * m_gap.x;
            float minimumHeight = m_padding.top + m_padding.bottom
                + _resolvedRows * Mathf.Max(0, m_rowTracks.minTrackSize)
                + Mathf.Max(0, _resolvedRows - 1) * m_gap.y;

            SetLayoutInput(
                RectTransform.Axis.Horizontal,
                minimumWidth,
                m_sizing.x == SizingMode.FitContent ? _fitContentSize.x : Mathf.Max(_rect.rect.width, minimumWidth),
                m_sizing.x == SizingMode.Grow ? 1 : -1
            );
            SetLayoutInput(
                RectTransform.Axis.Vertical,
                minimumHeight,
                m_sizing.y == SizingMode.FitContent ? _fitContentSize.y : Mathf.Max(_rect.rect.height, minimumHeight),
                m_sizing.y == SizingMode.Grow ? 1 : -1
            );
        }

        private void SanitizeGridConfiguration() {
            m_columnTracks = GridTrackResolver.Sanitize(m_columnTracks);
            m_rowTracks = GridTrackResolver.Sanitize(m_rowTracks);
            m_gap = MaxVector(Vector2.zero, m_gap);
        }

        private static Vector2 MaxVector(Vector2 a, Vector2 b) {
            return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }
    }
}

