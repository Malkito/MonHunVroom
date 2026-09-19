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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChildInfo = CupOHappiness.UI.LayoutUtil.ChildInfo;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CupOHappiness.UI
{
    public class Layout : LayoutItem, IComparable<Layout>, ILayoutGroup, IuLayoutContainer
    {
        /* THINGS THAT CAN CAUSE A LAYOUT UPDATE
            - non-grow child RectTransform changes size
            - number of children change
            - child is enabled/disabled
            - this container changes
        */

        #if UNITY_EDITOR
        public static List<Layout> RefreshedThisFrame = new();
        #endif
        
        [Header("Layout")]
        [Tooltip("Inner space between this container's edges and the area where children are placed.")]
        [SerializeField] protected Margins            m_padding;
        [Tooltip("Main direction used to place children.")]
        [SerializeField] protected LayoutDirection    m_direction;
        [Tooltip("How children are spread along the main axis.")]
        [SerializeField] protected Justification      m_justifyContent;
        [Tooltip("How flex lines are positioned on the cross axis.")]
        [SerializeField] protected Alignment          m_alignContent;
        [Tooltip("How each child is positioned inside its flex line on the cross axis.")]
        [SerializeField] protected Alignment          m_alignItems;
        [Tooltip("Gap between children on the main axis. This is usually set to 0 when using Space Between.")]
        [SerializeField] protected float              m_innerSpacing;
        [Tooltip("If enabled, child scale is ignored when this layout measures and places children.")]
        [SerializeField] protected bool               m_ignoreChildScale;

        [Header("Wrap")]
        [Tooltip("Allows children to break onto extra lines when they overflow the main axis.")]
        [SerializeField] protected FlexWrap           m_wrap = FlexWrap.NoWrap;
        [Tooltip("Gap between wrapped lines on the cross axis.")]
        [SerializeField] protected float              m_crossAxisGap;


        public int ChildCount =>            _children?.Count ?? 0;
        public Vector2Int GrowChildCount => _growChildCount;

        /// <summary>
        /// Wrapping mode used by this container.
        /// </summary>
        public FlexWrap Wrap {
            get => m_wrap;
            set {
                if(m_wrap != value) {
                    m_wrap = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gap between wrapped lines on the cross axis.
        /// </summary>
        public float CrossAxisGap {
            get => m_crossAxisGap;
            set { if(m_crossAxisGap != value) { m_crossAxisGap = value; SetDirty(); } }
        }

        /// <summary>
        /// Main content flow axis.
        /// </summary>
        public LayoutDirection Direction {
            get => m_direction;
            set {
                if(m_direction != value) {
                    m_direction = value;
                    SetDirty();
                }
            }
        }
        public Alignment AlignContent {
            get => m_alignContent;
            set { if(m_alignContent != value) { m_alignContent = value; SetDirty(); } }
        }
        public Alignment AlignItems {
            get => m_alignItems;
            set { if(m_alignItems != value) { m_alignItems = value; SetDirty(); } }
        }
        
        protected readonly List<ChildInfo>    _children = new();
        protected Vector2                     _contentSize;
        private Vector2                     _fitContentSize;
        private readonly List<FlexLine>     _lines = new();
        private int                         _layoutCycle;
        private int                         _lastLoggedCycle = -1;
        private int                         _depth;
        protected Vector2Int                _growChildCount;
        protected int                       _ignoreCount;
        private readonly Vector3[]          _rectCorners = new Vector3[4];

        public int LayoutCycle => _layoutCycle;
        
        #region TypeDef
        public enum Justification
        {
            Start,
            Center,
            End,
            SpaceBetween
        }
        
        public enum Alignment
        {
            Start,
            Center,
            End
        }
        
        public enum LayoutDirection
        {
            Row,
            Column,
            RowReverse,
            ColumnReverse
        }

        /// <summary>
        /// CSS flex-wrap behavior. NoWrap keeps every child on one line like the base Layout.
        /// Wrap and WrapReverse let children break onto extra lines when they overflow the main axis.
        /// </summary>
        public enum FlexWrap
        {
            NoWrap,
            Wrap,
            WrapReverse
        }

        protected class FlexLine
        {
            public readonly List<ChildInfo> items;
            public float primarySize;
            public float crossSize;
            public float crossOffset;

            public FlexLine(List<ChildInfo> items) {
                this.items = items;
                primarySize = 0;
                crossSize = 0;
                crossOffset = 0;
            }
        }

        #endregion

        #region Layout MonoBehavior
        protected override void OnEnable() {
            base.OnEnable();
            Log("enable");

            RefreshChildCache();
        }

        protected override void OnDisable() {
            base.OnDisable();
        }

        private void OnTransformChildrenChanged() {
            RefreshChildCache();
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
        }

        protected override void OnCanvasHierarchyChanged() {
            base.OnCanvasHierarchyChanged();
        }

        protected override void OnDidApplyAnimationProperties() {
            base.OnDidApplyAnimationProperties();
        }

        public override void Update() {
            base.Update();
        }

        public override void SetDirty() {
            base.SetDirty();
        }
        private void OnDrawGizmosSelected() {
            _rect.GetWorldCorners(_rectCorners);

            Matrix4x4 ltw = _rect.localToWorldMatrix;
            
            foreach(Vector3 v in _rectCorners) {
                LayoutUtil.DrawCenteredDebugBox(v, 0.15f, 0.15f, Color.red);
            }

            Rect r = new Rect(_rectCorners[0], _rectCorners[2] - _rectCorners[0]);
            r.position += (Vector2)(ltw * new Vector2(m_padding.left, m_padding.bottom));
            r.size -= (Vector2)(ltw * new Vector2(m_padding.left + m_padding.right, m_padding.top + m_padding.bottom));
            
            LayoutUtil.DrawDebugBox(r, _rect.position.z, Color.green);
        }
        #endregion

        #region ILayoutGroup
        public override void CalculateLayoutInputHorizontal() {
            if(_dirty) {
                _layoutCycle++;
                Log($"Cycle {_layoutCycle} begin: measure horizontal");
            }

            if(_dirty && m_wrap != FlexWrap.NoWrap) {
                BuildLines();
                return;
            }

            if(_dirty) {
                RefreshChildrenForMeasurement();
                #if UNITY_EDITOR
                RefreshedThisFrame.Add(this);
                #endif
                
                Log("CalculateLayoutInputHorizontal");
                
                _growChildCount.x = 0;
                _ignoreCount = 0;
                
                if(_children.Count > 0) {
                    // get number of disabled/ignore children
                    foreach(ChildInfo c in _children) {
                        if(CheckIgnoreElem(c)) {
                            _ignoreCount++;
                        }
                        else {
                            ResolveChildSize(c, RectTransform.Axis.Horizontal, applyRectTransform: false);
                        }
                    }

                    int includedChildCount = _children.Count - _ignoreCount;
                    float primarySize = m_justifyContent == Justification.SpaceBetween
                        ? 0
                        : m_innerSpacing * Mathf.Max(0, includedChildCount - 1);
                    float crossSize = 0;
                    
                    // calculate content size
                    float maxCrossSize = 0;
                    foreach(ChildInfo c in _children) {
                        // skip disabled/ignore items
                        if(CheckIgnoreElem(c))
                            continue;
                        
                        bool grow = false;
                        if(c.item) {
                            grow = c.item.SizeMode.x == SizingMode.Grow;
                            if(grow) {
                                _growChildCount.x++;
                            }
                        }
                        
                        switch(m_direction) {
                            case LayoutDirection.Row:
                            case LayoutDirection.RowReverse:
                                primarySize += (grow ? 0 : c.size.x) + c.margins.left + c.margins.right;
                                break;
                            case LayoutDirection.Column:
                            case LayoutDirection.ColumnReverse:
                                maxCrossSize = Mathf.Max(maxCrossSize, (grow ? 0 : c.size.x) + c.margins.left + c.margins.right);
                                break;
                        }
                        
                        Log($"\"{c.rect.name}\" - x: {(grow ? 0 : c.size.x)}");
                    }
                    crossSize += maxCrossSize;

                    // save content size for later
                    switch(m_direction) {
                        case LayoutDirection.Row:
                        case LayoutDirection.RowReverse:
                            _contentSize.x = primarySize;
                            break;
                        case LayoutDirection.Column:
                        case LayoutDirection.ColumnReverse:
                            _contentSize.x = crossSize;
                            break;
                    }
                    
                    _fitContentSize.x = _contentSize.x + m_padding.left + m_padding.right;
                    SetLayoutInput(
                        RectTransform.Axis.Horizontal,
                        Mathf.Max(m_minSize.x, GetMinimumContentSize(RectTransform.Axis.Horizontal)),
                        m_sizing.x == SizingMode.FitContent ? _fitContentSize.x : Mathf.Max(_rect.rect.width, _fitContentSize.x),
                        m_sizing.x == SizingMode.Grow ? 1 : -1
                    );
                    Log($"measured rect x size: {_fitContentSize.x:f3}");
                }
                else {
                    _contentSize = Vector2.zero;
                    _fitContentSize.x = m_padding.left + m_padding.right;
                    SetLayoutInput(
                        RectTransform.Axis.Horizontal,
                        Mathf.Max(m_minSize.x, GetMinimumContentSize(RectTransform.Axis.Horizontal)),
                        m_sizing.x == SizingMode.FitContent ? _fitContentSize.x : Mathf.Max(_rect.rect.width, _fitContentSize.x),
                        m_sizing.x == SizingMode.Grow ? 1 : -1
                    );
                }
                
                Log($"content x size: {_contentSize.x:f3}");
            }
        }
        
        public override void CalculateLayoutInputVertical() {
            if(_dirty && m_wrap != FlexWrap.NoWrap) {
                BuildLines();
                return;
            }

            if(_dirty) {
                Log("CalculateLayoutInputVertical");
                
                _growChildCount.y = 0;
                
                if(_children.Count > 0) {
                    foreach(ChildInfo c in _children) {
                        if(!CheckIgnoreElem(c)) {
                            ResolveChildSize(c, RectTransform.Axis.Vertical, applyRectTransform: false);
                        }
                    }
                    
                    int includedChildCount = _children.Count - _ignoreCount;
                    float primarySize = m_justifyContent == Justification.SpaceBetween
                        ? 0
                        : m_innerSpacing * Mathf.Max(0, includedChildCount - 1);
                    float crossSize = 0;
                    
                    // calculate content size
                    float maxCrossSize = 0;
                    foreach(ChildInfo c in _children) {
                        // skip disabled/ignore items
                        if(CheckIgnoreElem(c))
                            continue;
                        
                        bool grow = false;
                        if(c.item) {
                            grow = c.item.SizeMode.y == SizingMode.Grow;
                            if(grow) {
                                _growChildCount.y++;
                            }
                        }
                        
                        switch(m_direction) {
                            case LayoutDirection.Row:
                            case LayoutDirection.RowReverse:
                                maxCrossSize = Mathf.Max(maxCrossSize, (grow ? 0 : c.size.y) + c.margins.top + c.margins.bottom);
                                break;
                            case LayoutDirection.Column:
                            case LayoutDirection.ColumnReverse:
                                primarySize += (grow ? 0 : c.size.y) + c.margins.top + c.margins.bottom;
                                break;
                        }
                    }
                    crossSize += maxCrossSize;

                    // save content size for later
                    switch(m_direction) {
                        case LayoutDirection.Row:
                        case LayoutDirection.RowReverse:
                            _contentSize.y = crossSize;
                            break;
                        case LayoutDirection.Column:
                        case LayoutDirection.ColumnReverse:
                            _contentSize.y = primarySize;
                            break;
                    }
                    
                    _fitContentSize.y = _contentSize.y + m_padding.top + m_padding.bottom;
                    SetLayoutInput(
                        RectTransform.Axis.Vertical,
                        Mathf.Max(m_minSize.y, GetMinimumContentSize(RectTransform.Axis.Vertical)),
                        m_sizing.y == SizingMode.FitContent ? _fitContentSize.y : Mathf.Max(_rect.rect.height, _fitContentSize.y),
                        m_sizing.y == SizingMode.Grow ? 1 : -1
                    );
                    Log($"measured rect y size: {_fitContentSize.y:f3}");
                }
                else {
                    _contentSize = Vector2.zero;
                    _fitContentSize.y = m_padding.top + m_padding.bottom;
                    SetLayoutInput(
                        RectTransform.Axis.Vertical,
                        Mathf.Max(m_minSize.y, GetMinimumContentSize(RectTransform.Axis.Vertical)),
                        m_sizing.y == SizingMode.FitContent ? _fitContentSize.y : Mathf.Max(_rect.rect.height, _fitContentSize.y),
                        m_sizing.y == SizingMode.Grow ? 1 : -1
                    );
                }
                
                Log($"content x size: {_contentSize.y:f3}");
            }
        }

        public virtual void SetLayoutHorizontal() {
            if(_dirty && m_wrap != FlexWrap.NoWrap) {
                Log("SetLayoutHorizontal wrapped");
                if(m_sizing.x == SizingMode.FitContent)
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ClampSize(RectTransform.Axis.Horizontal, _fitContentSize.x));
                ResolveDynamicChildrenForFinalParentRect(RectTransform.Axis.Horizontal);
                BuildLines(applyRectTransform: true);
                PlaceLinePrimaryAxis();
                return;
            }

            if(_dirty) {
                Log("SetLayoutHorizontal");
                if(m_sizing.x == SizingMode.FitContent)
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ClampSize(RectTransform.Axis.Horizontal, _fitContentSize.x));
                ResolveDynamicChildrenForFinalParentRect(RectTransform.Axis.Horizontal);
                GrowChildren(RectTransform.Axis.Horizontal);
                HorizontalLayout();
            }
        }
        
        public virtual void SetLayoutVertical() {
            if(_dirty && m_wrap != FlexWrap.NoWrap) {
                Log("SetLayoutVertical wrapped");
                if(m_sizing.y == SizingMode.FitContent)
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ClampSize(RectTransform.Axis.Vertical, _fitContentSize.y));
                ResolveDynamicChildrenForFinalParentRect(RectTransform.Axis.Vertical);
                BuildLines(applyRectTransform: true);
                PlaceLineCrossAxis();
                FlushCycleLog();
                _dirty = false;
                return;
            }

            if(_dirty) {
                Log("SetLayoutVertical");
                if(m_sizing.y == SizingMode.FitContent)
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ClampSize(RectTransform.Axis.Vertical, _fitContentSize.y));
                ResolveDynamicChildrenForFinalParentRect(RectTransform.Axis.Vertical);
                GrowChildren(RectTransform.Axis.Vertical);
                VerticalLayout();
                FlushCycleLog();
            }

            _dirty = false;
        }
        #endregion

        private float GetMinimumContentSize(RectTransform.Axis axis) {
            float minimum = axis == RectTransform.Axis.Horizontal
                ? m_padding.left + m_padding.right
                : m_padding.top + m_padding.bottom;
            float spacing = m_justifyContent == Justification.SpaceBetween ? 0 : m_innerSpacing;
            bool primary = IsPrimaryAxis(axis);
            int included = 0;

            foreach(ChildInfo child in _children) {
                if(CheckIgnoreElem(child))
                    continue;

                float childMinimum = child.item
                    ? (axis == RectTransform.Axis.Horizontal ? child.item.MinSize.x : child.item.MinSize.y)
                    : 0;
                float margins = axis == RectTransform.Axis.Horizontal
                    ? child.margins.left + child.margins.right
                    : child.margins.top + child.margins.bottom;
                float extent = childMinimum + margins;
                if(primary) {
                    minimum += extent;
                    included++;
                }
                else {
                    minimum = Mathf.Max(minimum, (axis == RectTransform.Axis.Horizontal ? m_padding.left + m_padding.right : m_padding.top + m_padding.bottom) + extent);
                }
            }

            if(primary && included > 1)
                minimum += spacing * (included - 1);
            return minimum;
        }

        /// <summary>
        /// Unity calculates all layout inputs before it applies parent rectangles. Dynamic
        /// children therefore cannot be finalized during measurement: their parent may
        /// still be zero-sized. Resolve them here, in the parent-owned control phase,
        /// after Unity has assigned this container's final axis size.
        /// </summary>
        private void ResolveDynamicChildrenForFinalParentRect(RectTransform.Axis axis) {
            bool changed = false;
            foreach(ChildInfo child in _children) {
                if(!child.item || CheckIgnoreElem(child))
                    continue;

                SizingMode mode = axis == RectTransform.Axis.Horizontal ? child.item.SizeMode.x : child.item.SizeMode.y;
                if(mode != SizingMode.Percent && mode != SizingMode.FitContent)
                    continue;

                float before = axis == RectTransform.Axis.Horizontal ? child.rect.rect.width : child.rect.rect.height;
                ResolveChildSize(child, axis, applyRectTransform: true);
                float after = axis == RectTransform.Axis.Horizontal ? child.rect.rect.width : child.rect.rect.height;
                changed |= !Mathf.Approximately(before, after);
            }

            if(changed)
                RefreshResolvedContentSize(axis);
        }

        /// <summary>
        /// Rebuilds only the axis affected by late dynamic resolution. Measurement keeps
        /// its intrinsic responsibilities; this updates the final-flow cache consumed by
        /// grow distribution and alignment without issuing another Canvas rebuild.
        /// </summary>
        private void RefreshResolvedContentSize(RectTransform.Axis axis) {
            bool primary = IsPrimaryAxis(axis);
            int includedChildCount = 0;
            float primarySize = 0;
            float crossSize = 0;

            foreach(ChildInfo child in _children) {
                if(CheckIgnoreElem(child))
                    continue;

                includedChildCount++;
                child.margins = child.item ? child.item.Margin : child.margins;
                float scale = m_ignoreChildScale ? 1 : (axis == RectTransform.Axis.Horizontal ? child.rect.localScale.x : child.rect.localScale.y);
                float size = (axis == RectTransform.Axis.Horizontal ? child.rect.rect.width : child.rect.rect.height) * scale;
                if(axis == RectTransform.Axis.Horizontal)
                    child.size.x = size;
                else
                    child.size.y = size;

                bool grow = child.item && (axis == RectTransform.Axis.Horizontal ? child.item.SizeMode.x : child.item.SizeMode.y) == SizingMode.Grow;
                float margins = axis == RectTransform.Axis.Horizontal
                    ? child.margins.left + child.margins.right
                    : child.margins.top + child.margins.bottom;

                if(primary) {
                    primarySize += (grow ? 0 : size) + margins;
                }
                else {
                    crossSize = Mathf.Max(crossSize, (grow ? 0 : size) + margins);
                }
            }

            if(primary && m_justifyContent != Justification.SpaceBetween)
                primarySize += m_innerSpacing * Mathf.Max(0, includedChildCount - 1);

            float resolvedContent = primary ? primarySize : crossSize;
            if(axis == RectTransform.Axis.Horizontal) {
                _contentSize.x = resolvedContent;
                _fitContentSize.x = resolvedContent + m_padding.left + m_padding.right;
            }
            else {
                _contentSize.y = resolvedContent;
                _fitContentSize.y = resolvedContent + m_padding.top + m_padding.bottom;
            }

            Log($"Resolved final content axis={axis} content={resolvedContent:F3}");
        }
        
        #region Layout Internal
        private void Log(object msg) {
            // Retained as a local tracing hook while development diagnostics are enabled.
            // Console output is emitted once by FlushCycleLog instead of for every
            // measurement, sizing, grow, and placement event.
        }

        private void FlushCycleLog() {
            if(!m_log || _lastLoggedCycle == _layoutCycle)
                return;

            int activeChildren = 0;
            int zeroDynamicX = 0;
            int zeroDynamicY = 0;
            foreach(ChildInfo child in _children) {
                if(CheckIgnoreElem(child))
                    continue;

                activeChildren++;
                if(!child.item)
                    continue;

                if(child.item.SizeMode.x != SizingMode.Fixed && child.rect.rect.width <= 0.001f)
                    zeroDynamicX++;
                if(child.item.SizeMode.y != SizingMode.Fixed && child.rect.rect.height <= 0.001f)
                    zeroDynamicY++;
            }

            Debug.Log(
                $"[uLayout] {name} cycle={_layoutCycle} rect={_rect.rect.size} content={_contentSize} fit={_fitContentSize} " +
                $"flow={m_direction}/{m_wrap} align={m_justifyContent}/{m_alignItems}/{m_alignContent} " +
                $"padding={m_padding} gaps=({m_innerSpacing:F2},{m_crossAxisGap:F2}) " +
                $"children={activeChildren}/{_children.Count} ignored={_ignoreCount} grow={_growChildCount} " +
                $"dynamicZero=({zeroDynamicX},{zeroDynamicY})",
                this
            );
            _lastLoggedCycle = _layoutCycle;
        }

        private string DescribeChild(ChildInfo child) {
            if(!child.rect)
                return "<missing RectTransform>";

            LayoutItem item = child.item;
            string itemState = item
                ? $"mode=({item.SizeMode.x},{item.SizeMode.y}) percent={item.Percentage} min={item.MinSize} max={item.MaxSize}"
                : "no LayoutItem";
            return $"child={child.rect.name} index={child.index} active={child.rect.gameObject.activeInHierarchy} ignored={CheckIgnoreElem(child)} rect={child.rect.rect.size} cached={child.size} scale={child.rect.localScale} margins={child.margins} {itemState}";
        }
        
        protected bool CheckIgnoreElem(ChildInfo ci) {
            return LayoutUtil.IsIgnored(ci);
        }

        protected void SetAnchorX(RectTransform rt, float x) {
            rt.anchorMin = rt.anchorMin.SetX(x);
            rt.anchorMax = rt.anchorMax.SetX(x);
        }
        protected void SetAnchorY(RectTransform rt, float y) {
            rt.anchorMin = rt.anchorMin.SetY(y);
            rt.anchorMax = rt.anchorMax.SetY(y);
        }

        protected void HorizontalLayout() {
            Log($"Horizontal Layout - content size x: {_contentSize.x}");
            
            float offset = 0;
            float leftover;
            float spacing = 0;
            int index = 0;
            switch(m_direction) {
                // ROW -> PRIMARY AXIS
                case LayoutDirection.Row:
                    switch(m_justifyContent) {
                        case Justification.Start:
                            offset += m_padding.left;
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0);

                                offset += c.margins.left;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset);
                                offset += (c.size.x - pivot) + c.margins.right + m_innerSpacing;
                            }
                            break;
                        case Justification.Center:
                            offset -= (_contentSize.x + m_padding.left + m_padding.right) / 2;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0.5f);
                            
                                offset += c.margins.left;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset + m_padding.left);
                                offset += (c.size.x-pivot) + c.margins.right + m_innerSpacing;
                            }
                            break;
                        case Justification.End:
                            offset -= m_padding.right + _contentSize.x;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 1);

                                offset += c.margins.left;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset);
                                offset += (c.size.x - pivot) + c.margins.right + m_innerSpacing;
                            }
                            break;
                        case Justification.SpaceBetween:
                            offset += m_padding.left;
                            leftover = _rect.rect.size.x - _contentSize.x - m_padding.left - m_padding.right;
                            
                            if(_children.Count > 1)
                                spacing = Mathf.Max(0, leftover / (_children.Count-_ignoreCount-1));
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0);
                            
                                if(index != 0) {
                                    offset += spacing;
                                }

                                offset += c.margins.left;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset);
                                offset += (c.size.x - pivot) + c.margins.right;
                                index++;
                            }
                            break;
                    }
                    break;
                // ROW-REVERSE -> PRIMARY AXIS
                case LayoutDirection.RowReverse:
                    switch(m_justifyContent) {
                        case Justification.Start:
                            offset += m_padding.left + _contentSize.x;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0);

                                offset -= c.margins.right;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset -= (c.size.x - pivot);
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset);
                                offset -= pivot + c.margins.left + m_innerSpacing;
                            }
                            break;
                        case Justification.Center:
                            offset += (_contentSize.x + m_padding.left + m_padding.right) / 2;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0.5f);

                                offset -= c.margins.right;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset -= c.size.x - pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset - m_padding.right);
                                offset -= pivot + c.margins.left + m_innerSpacing;
                            }
                            break;
                        case Justification.End:
                            offset += m_padding.right;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 1);

                                offset += c.margins.right;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset += c.size.x - pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(-offset);
                                offset += pivot + c.margins.left + m_innerSpacing;
                            }
                            break;
                        case Justification.SpaceBetween:
                            offset += m_padding.right;
                            leftover = _rect.rect.size.x - _contentSize.x - m_padding.left - m_padding.right;
                            
                            if(_children.Count > 1)
                                spacing = Mathf.Max(0, leftover / (_children.Count-_ignoreCount-1));
                                
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 1);

                                offset += c.margins.right;
                                float pivot = c.size.x * c.rect.pivot.x;
                                offset += c.size.x - pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(-offset);
                                offset += pivot + spacing + c.margins.left;
                            }
                            break;
                    }
                    break;
                // COLUMN/COLUMN-REVERSE -> CROSS AXIS
                case LayoutDirection.Column:
                case LayoutDirection.ColumnReverse:
                    switch(m_alignItems) {
                        case Alignment.Start:
                            offset += m_padding.left;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0);

                                float pivot = c.size.x * c.rect.pivot.x;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset + c.margins.left + pivot);
                            }
                            break;
                        case Alignment.Center:
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 0.5f);

                                float centeringOffset = (c.margins.left - c.margins.right) / 2f;
                                float pivot = c.size.x * c.rect.pivot.x;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(m_padding.left/2 - m_padding.right/2 + centeringOffset - (c.size.x/2 - pivot));
                            }
                            break;
                        case Alignment.End:
                            offset += m_padding.right;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorX(c.rect, 1);

                                float pivot = c.size.x * c.rect.pivot.x;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(-offset - c.margins.right - (c.size.x - pivot));
                            }
                            break;
                    }
                    break;
            }
            
        }

        protected void VerticalLayout() {
            Log($"Vertical Layout - content size y: {_contentSize.y}");
            
            float offset = 0;
            float leftover;
            float spacing = 0;
            int index = 0;
            switch(m_direction) {
                // ROW/ROW-REVERSE -> CROSS AXIS
                case LayoutDirection.Row:
                case LayoutDirection.RowReverse:
                    switch(m_alignItems) {
                        case Alignment.Start:
                            offset += m_padding.top;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 1);

                                float pivot = c.size.y * c.rect.pivot.y;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(-offset - c.margins.top - (c.size.y - pivot));
                            }
                            break;
                        case Alignment.Center:
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0.5f);

                                float centeringOffset = (c.margins.bottom - c.margins.top) / 2f;
                                float pivot = c.size.y * c.rect.pivot.y;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(m_padding.bottom/2 - m_padding.top/2 + centeringOffset - (c.size.y/2 - pivot));
                            }
                            break;
                        case Alignment.End:
                            offset += m_padding.bottom;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0);
                    
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset + c.margins.bottom + (c.size.y * c.rect.pivot.y));
                            }
                            break;
                    }
                    break;
                // COLUMN -> PRIMARY AXIS
                case LayoutDirection.Column:
                    switch(m_justifyContent) {
                        case Justification.Start:
                            offset -= m_padding.top;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 1);

                                offset -= c.margins.top;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset -= c.size.y - pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset);
                                offset -= pivot + c.margins.bottom + m_innerSpacing;
                            }
                            break;
                        case Justification.Center:
                            offset += (_contentSize.y + m_padding.top + m_padding.bottom) / 2;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0.5f);

                                offset -= c.margins.top;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset -= c.size.y - pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset - m_padding.top);
                                offset -= pivot + c.margins.bottom + m_innerSpacing;
                            }
                            break;
                        case Justification.End:
                            offset += _contentSize.y;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0);

                                offset += c.margins.bottom;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset + m_padding.bottom);
                                offset += (c.size.y - pivot) + c.margins.top + m_innerSpacing;
                            }
                            break;
                        case Justification.SpaceBetween:
                            offset += m_padding.top;
                            leftover = _rect.rect.size.y - _contentSize.y - m_padding.top - m_padding.bottom;
                            
                            if(_children.Count > 1)
                                spacing = Mathf.Max(0, leftover / (_children.Count-_ignoreCount-1));
                                
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 1);
                                
                                if(index != 0) {
                                    offset += spacing;
                                }

                                offset += c.margins.top;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset += c.size.y - pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(-offset);
                                offset += pivot + c.margins.bottom;
                            
                                index++;
                            }
                            break;
                    }
                    break;
                // COLUMN-REVERSE -> PRIMARY AXIS
                case LayoutDirection.ColumnReverse:
                    switch(m_justifyContent) {
                        case Justification.Start:
                            offset -= m_padding.top + _contentSize.y;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 1);

                                offset += c.margins.bottom;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset);
                                offset += c.size.y - pivot + c.margins.top + m_innerSpacing;
                            }
                            break;
                        case Justification.Center:
                            offset -= (_contentSize.y + m_padding.top + m_padding.bottom) / 2;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0.5f);

                                offset += c.margins.bottom;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset);
                                offset += c.size.y - pivot + c.margins.top + m_innerSpacing;
                            }
                            break;
                        case Justification.End:
                            offset += m_padding.bottom;
                            
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0);

                                offset += c.margins.bottom;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset);
                                offset += c.size.y - pivot + c.margins.top + m_innerSpacing;
                            }
                            break;
                        case Justification.SpaceBetween:
                            offset += m_padding.bottom;
                            leftover = _rect.rect.size.y - _contentSize.y - m_padding.top - m_padding.bottom;
                            
                            if(_children.Count > 1)
                                spacing = Mathf.Max(0, leftover / (_children.Count-_ignoreCount-1));
                                
                            foreach(ChildInfo c in _children) {
                                // skip disabled/ignore items
                                if(CheckIgnoreElem(c))
                                    continue;
                                
                                SetAnchorY(c.rect, 0);

                                if(index != 0) {
                                    offset += spacing;
                                }
                                
                                offset += c.margins.bottom;
                                float pivot = c.size.y * c.rect.pivot.y;
                                offset += pivot;
                                c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(offset);
                                offset += c.size.y - pivot + c.margins.top;

                                index++;
                            }
                            break;
                    }
                    break;
            }
        }
        
        protected void GrowChildren(RectTransform.Axis axis) {
            switch(axis) {
                case RectTransform.Axis.Horizontal:
                    if(_growChildCount.x > 0) {
                        Log($"growing {_growChildCount.x} children horizontally (rect: {_rect.rect.size.x}, content: {_contentSize.x})");

                        switch(m_direction) {
                            case LayoutDirection.Row:
                            case LayoutDirection.RowReverse:
                                ApplyPrimaryAxisGrow(axis);
                                break;
                            case LayoutDirection.Column:
                            case LayoutDirection.ColumnReverse:
                                ApplyCrossAxisGrow(axis);
                                break;
                        }
                    }
                    break;
                case RectTransform.Axis.Vertical:
                    if(_growChildCount.y > 0) {
                        Log($"growing {_growChildCount.y} children vertically (rect: {_rect.rect.size.y}, content: {_contentSize.y})");

                        switch(m_direction) {
                            case LayoutDirection.Row:
                            case LayoutDirection.RowReverse:
                                ApplyCrossAxisGrow(axis);
                                break;
                            case LayoutDirection.Column:
                            case LayoutDirection.ColumnReverse:
                                ApplyPrimaryAxisGrow(axis);
                                break;
                        }
                    }
                    break;
            }
        }

        protected void ApplyPrimaryAxisGrow(RectTransform.Axis axis) {
            List<ChildInfo> growChildren = GetGrowChildren(axis);
            if(growChildren.Count == 0)
                return;

            float remainingSpace = Mathf.Max(0, GetPrimaryAxisLeftover(axis));
            int unresolvedCount = growChildren.Count;
            bool[] resolved = new bool[growChildren.Count];
            float[] resolvedSizes = new float[growChildren.Count];

            bool changed = true;
            while(changed && unresolvedCount > 0) {
                changed = false;
                float share = unresolvedCount > 0 ? remainingSpace / unresolvedCount : 0;

                for(int i = 0; i < growChildren.Count; i++) {
                    if(resolved[i])
                        continue;

                    ChildInfo child = growChildren[i];
                    float size = child.item.ResolveAxisSize(axis, share);
                    if(!Mathf.Approximately(size, share)) {
                        resolved[i] = true;
                        resolvedSizes[i] = size;
                        remainingSpace -= size;
                        unresolvedCount--;
                        changed = true;
                    }
                }
            }

            float finalShare = unresolvedCount > 0 ? remainingSpace / unresolvedCount : 0;
            for(int i = 0; i < growChildren.Count; i++) {
                if(!resolved[i]) {
                    resolvedSizes[i] = growChildren[i].item.ResolveAxisSize(axis, finalShare);
                }
            }

            for(int i = 0; i < growChildren.Count; i++) {
                ApplyResolvedGrowSize(growChildren[i], axis, resolvedSizes[i], primaryAxis: true);
            }
        }

        protected void ApplyCrossAxisGrow(RectTransform.Axis axis) {
            List<ChildInfo> growChildren = GetGrowChildren(axis);
            foreach(ChildInfo child in growChildren) {
                float available = GetInnerAvailableSize(axis, child);
                float size = child.item.ResolveAxisSize(axis, available);
                ApplyResolvedGrowSize(child, axis, size, primaryAxis: false);
            }
        }

        protected List<ChildInfo> GetGrowChildren(RectTransform.Axis axis) {
            List<ChildInfo> growChildren = new();
            foreach(ChildInfo c in _children) {
                if(!c.item || CheckIgnoreElem(c))
                    continue;

                SizingMode mode = axis == RectTransform.Axis.Horizontal ? c.item.SizeMode.x : c.item.SizeMode.y;
                if(mode == SizingMode.Grow) {
                    growChildren.Add(c);
                }
            }
            return growChildren;
        }

        protected float GetPrimaryAxisLeftover(RectTransform.Axis axis) {
            if(axis == RectTransform.Axis.Horizontal) {
                return _rect.rect.size.x - _contentSize.x - m_padding.left - m_padding.right;
            }

            return _rect.rect.size.y - _contentSize.y - m_padding.top - m_padding.bottom;
        }

        protected void ApplyResolvedGrowSize(ChildInfo child, RectTransform.Axis axis, float size, bool primaryAxis) {
            if(m_log)
                Log($"Grow resolve axis={axis} primary={primaryAxis} requested={size:F3} before {DescribeChild(child)}");

            float scale = m_ignoreChildScale ? 1 : (axis == RectTransform.Axis.Horizontal ? child.rect.localScale.x : child.rect.localScale.y);
            float scaledSize = size * scale;

            if(axis == RectTransform.Axis.Horizontal) {
                child.size.x = scaledSize;
                if(primaryAxis) {
                    _contentSize.x += scaledSize;
                }
                else {
                    _contentSize.x = Mathf.Max(scaledSize + child.margins.left + child.margins.right, _contentSize.x);
                }
            }
            else {
                child.size.y = scaledSize;
                if(primaryAxis) {
                    _contentSize.y += scaledSize;
                }
                else {
                    _contentSize.y = Mathf.Max(scaledSize + child.margins.top + child.margins.bottom, _contentSize.y);
                }
            }

            if(!Mathf.Approximately(axis == RectTransform.Axis.Horizontal ? child.rect.rect.size.x : child.rect.rect.size.y, size)) {
                child.rect.SetSizeWithCurrentAnchors(axis, size);
            }

            if(axis == RectTransform.Axis.Horizontal && child.item is LayoutText t) {
                t.HandleGrowSizingX();
                float updatedScaledHeight = child.rect.rect.size.y * (m_ignoreChildScale ? 1 : child.rect.localScale.y);
                float diff = updatedScaledHeight - child.size.y;
                if(!Mathf.Approximately(diff, 0)) {
                    child.size.y = updatedScaledHeight;
                    GrowSizingXCallback(diff);
                }
            }

            if(m_log)
                Log($"Grow applied axis={axis} after {DescribeChild(child)}");
        }

        protected float GetInnerAvailableSize(RectTransform.Axis axis, ChildInfo child) {
            float available = axis == RectTransform.Axis.Horizontal
                ? _rect.rect.size.x - m_padding.left - m_padding.right
                : _rect.rect.size.y - m_padding.top - m_padding.bottom;

            if(child.item) {
                if(axis == RectTransform.Axis.Horizontal) {
                    available -= child.margins.left + child.margins.right;
                }
                else {
                    available -= child.margins.top + child.margins.bottom;
                }
            }

            return Mathf.Max(0, available);
        }

        protected bool IsPrimaryAxis(RectTransform.Axis axis) {
            return axis switch {
                RectTransform.Axis.Horizontal => m_direction == LayoutDirection.Row || m_direction == LayoutDirection.RowReverse,
                RectTransform.Axis.Vertical => m_direction == LayoutDirection.Column || m_direction == LayoutDirection.ColumnReverse,
                _ => false
            };
        }

        protected float GetPrimaryAxisPercentAvailable(RectTransform.Axis axis) {
            float available = axis == RectTransform.Axis.Horizontal
                ? _rect.rect.size.x - m_padding.left - m_padding.right
                : _rect.rect.size.y - m_padding.top - m_padding.bottom;

            foreach(ChildInfo c in _children) {
                if(CheckIgnoreElem(c))
                    continue;

                Margins margins = c.item ? c.item.Margin : c.margins;
                if(axis == RectTransform.Axis.Horizontal) {
                    available -= margins.left + margins.right;
                }
                else {
                    available -= margins.top + margins.bottom;
                }
            }

            return Mathf.Max(0, available);
        }

        protected void ResolveChildSize(ChildInfo child, RectTransform.Axis axis, bool applyRectTransform = true) {
            if(!child.item || CheckIgnoreElem(child))
                return;

            child.item.WarnCircularSizing(axis);
            child.margins = child.item.Margin;

            SizingMode mode = axis == RectTransform.Axis.Horizontal ? child.item.SizeMode.x : child.item.SizeMode.y;
            if(mode == SizingMode.Grow)
                return;

            float availableSize = GetInnerAvailableSize(axis, child);
            if(mode == SizingMode.Percent && IsPrimaryAxis(axis)) {
                availableSize = GetPrimaryAxisPercentAvailable(axis);
            }

            if(m_log)
                Log($"Child resolve axis={axis} mode={mode} available={availableSize:F3} primary={IsPrimaryAxis(axis)} before {DescribeChild(child)}");
            LayoutUtil.ResolveChildSize(child, axis, availableSize, m_ignoreChildScale, applyRectTransform);
            if(m_log)
                Log($"Child resolved axis={axis} after {DescribeChild(child)}");
        }
        #endregion

        private void BuildLines(bool applyRectTransform = false) {
            RefreshChildrenForMeasurement();
            #if UNITY_EDITOR
            RefreshedThisFrame.Add(this);
            #endif

            bool mainIsX = IsPrimaryAxis(RectTransform.Axis.Horizontal);
            float mainExtent = mainIsX
                ? _rect.rect.width - m_padding.left - m_padding.right
                : _rect.rect.height - m_padding.top - m_padding.bottom;

            Log($"Build lines mainAxis={(mainIsX ? "Horizontal" : "Vertical")} mainExtent={mainExtent:F3}");

            _lines.Clear();
            _growChildCount = Vector2Int.zero;
            _ignoreCount = 0;

            FlexLine line = null;
            float lineUsed = 0;

            foreach(ChildInfo c in _children) {
                if(CheckIgnoreElem(c)) {
                    _ignoreCount++;
                    continue;
                }

                ResolveChildSize(c, RectTransform.Axis.Horizontal, applyRectTransform);
                ResolveChildSize(c, RectTransform.Axis.Vertical, applyRectTransform);

                if(c.item) {
                    if(c.item.SizeMode.x == SizingMode.Grow)
                        _growChildCount.x++;
                    if(c.item.SizeMode.y == SizingMode.Grow)
                        _growChildCount.y++;
                }

                float mainExtentOfItem = MinimumMainExtentForWrapping(c, mainIsX);
                float cost = line == null ? mainExtentOfItem : m_innerSpacing + mainExtentOfItem;

                if(m_log)
                    Log($"Wrap candidate extent={mainExtentOfItem:F3} cost={cost:F3} lineUsed={lineUsed:F3} lineLimit={mainExtent:F3} {DescribeChild(c)}");

                if(line == null || lineUsed + cost > mainExtent + 0.001f) {
                    line = NewLine();
                    lineUsed = 0;
                }

                line.items.Add(c);
                lineUsed += cost;
            }

            float contentPrimary = 0;
            float contentCross = 0;

            for(int i = 0; i < _lines.Count; i++) {
                FinishLine(_lines[i], mainIsX, mainExtent, applyRectTransform);

                if(i > 0)
                    contentCross += m_crossAxisGap;
                contentPrimary = Mathf.Max(contentPrimary, _lines[i].primarySize);
                contentCross += _lines[i].crossSize;
            }

            if(mainIsX) {
                _contentSize.x = contentPrimary;
                _contentSize.y = contentCross;
            }
            else {
                _contentSize.x = contentCross;
                _contentSize.y = contentPrimary;
            }

            _fitContentSize = new Vector2(
                (mainIsX ? contentPrimary : contentCross) + m_padding.left + m_padding.right,
                (mainIsX ? contentCross : contentPrimary) + m_padding.top + m_padding.bottom
            );
            SetLayoutInput(
                RectTransform.Axis.Horizontal,
                m_minSize.x,
                m_sizing.x == SizingMode.FitContent ? _fitContentSize.x : Mathf.Max(_rect.rect.width, _fitContentSize.x),
                m_sizing.x == SizingMode.Grow ? 1 : -1
            );
            SetLayoutInput(
                RectTransform.Axis.Vertical,
                m_minSize.y,
                m_sizing.y == SizingMode.FitContent ? _fitContentSize.y : Mathf.Max(_rect.rect.height, _fitContentSize.y),
                m_sizing.y == SizingMode.Grow ? 1 : -1
            );

            AssignLineOffsets(mainIsX);
        }

        private FlexLine NewLine() {
            FlexLine line = new FlexLine(new List<ChildInfo>());
            _lines.Add(line);
            return line;
        }

        private void AssignLineOffsets(bool mainIsX) {
            float crossExtent = mainIsX
                ? _rect.rect.height - m_padding.top - m_padding.bottom
                : _rect.rect.width - m_padding.left - m_padding.right;
            float used = 0;
            foreach(FlexLine line in _lines)
                used += line.crossSize;
            used += Mathf.Max(0, _lines.Count - 1) * m_crossAxisGap;

            float free = Mathf.Max(0, crossExtent - used);
            float alignOffset = m_alignContent switch {
                Alignment.Center => free / 2,
                Alignment.End => free,
                _ => 0
            };

            if(m_wrap == FlexWrap.WrapReverse) {
                float cursor = (mainIsX ? _rect.rect.height - m_padding.bottom : _rect.rect.width - m_padding.right) - alignOffset;
                for(int li = 0; li < _lines.Count; li++) {
                    cursor -= _lines[li].crossSize;
                    _lines[li].crossOffset = cursor;
                    cursor -= m_crossAxisGap;
                }
            }
            else {
                float cursor = (mainIsX ? m_padding.top : m_padding.left) + alignOffset;
                for(int li = 0; li < _lines.Count; li++) {
                    _lines[li].crossOffset = cursor;
                    cursor += _lines[li].crossSize + m_crossAxisGap;
                }
            }
        }

        private void FinishLine(FlexLine line, bool mainIsX, float mainExtent, bool applyRectTransform) {
            AllocateGrowChildren(line, mainIsX, mainExtent, applyRectTransform);

            float linePrimary = 0;
            float lineCross = 0;
            int index = 0;
            foreach(ChildInfo c in line.items) {
                if(index > 0)
                    linePrimary += m_innerSpacing;
                linePrimary += AllocatedMainExtentOf(c, mainIsX);
                lineCross = Mathf.Max(lineCross, CrossExtentOf(c, mainIsX));
                index++;
            }

            line.primarySize = linePrimary;
            line.crossSize = lineCross;
        }

        /// <summary>
        /// Allocates one line's free main-axis space. A constrained Grow child
        /// is frozen when its min or max changes the proposed share, then the
        /// remaining space is redistributed to the unresolved Grow children.
        /// </summary>
        private void AllocateGrowChildren(FlexLine line, bool mainIsX, float mainExtent, bool applyRectTransform) {
            RectTransform.Axis axis = mainIsX ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
            List<ChildInfo> growChildren = new();
            float fixedExtent = m_innerSpacing * Mathf.Max(0, line.items.Count - 1);

            foreach(ChildInfo child in line.items) {
                if(IsGrowChild(child, mainIsX)) {
                    growChildren.Add(child);
                    fixedExtent += MainMarginsOf(child, mainIsX);
                }
                else {
                    fixedExtent += MainExtentOf(child, mainIsX);
                }
            }

            if(growChildren.Count == 0)
                return;

            float remaining = Mathf.Max(0, mainExtent - fixedExtent);
            int unresolved = growChildren.Count;
            bool[] resolved = new bool[growChildren.Count];
            float[] sizes = new float[growChildren.Count];

            while(unresolved > 0) {
                float share = remaining / unresolved;
                bool changed = false;
                for(int i = 0; i < growChildren.Count; i++) {
                    if(resolved[i])
                        continue;

                    float size = growChildren[i].item.ResolveAxisSize(axis, share);
                    if(!Mathf.Approximately(size, share)) {
                        resolved[i] = true;
                        sizes[i] = size;
                        remaining -= size;
                        unresolved--;
                        changed = true;
                    }
                }

                if(!changed)
                    break;
            }

            float finalShare = unresolved > 0 ? remaining / unresolved : 0;
            for(int i = 0; i < growChildren.Count; i++) {
                if(!resolved[i])
                    sizes[i] = growChildren[i].item.ResolveAxisSize(axis, finalShare);
                ApplyGrowSize(growChildren[i], mainIsX, sizes[i], applyRectTransform);
            }
        }

        private bool IsGrowChild(ChildInfo c, bool mainIsX) {
            return c.item != null && (mainIsX ? c.item.SizeMode.x : c.item.SizeMode.y) == SizingMode.Grow;
        }

        private void ApplyGrowSize(ChildInfo c, bool mainIsX, float size, bool applyRectTransform) {
            if(mainIsX) {
                c.size.x = size * (m_ignoreChildScale ? 1 : c.rect.localScale.x);
                if(applyRectTransform && !Mathf.Approximately(c.rect.rect.size.x, size))
                    c.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                if(applyRectTransform && c.item is LayoutText t)
                    t.HandleGrowSizingX();
            }
            else {
                c.size.y = size * (m_ignoreChildScale ? 1 : c.rect.localScale.y);
                if(applyRectTransform && !Mathf.Approximately(c.rect.rect.size.y, size))
                    c.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
        }

        private float MainExtentOf(ChildInfo c, bool mainIsX) {
            if(IsGrowChild(c, mainIsX))
                return MainMarginsOf(c, mainIsX);

            if(mainIsX)
                return c.size.x + c.margins.left + c.margins.right;
            return c.size.y + c.margins.top + c.margins.bottom;
        }

        private float MinimumMainExtentForWrapping(ChildInfo c, bool mainIsX) {
            if(!IsGrowChild(c, mainIsX))
                return MainExtentOf(c, mainIsX);

            float minimum = mainIsX ? c.item.MinSize.x : c.item.MinSize.y;
            float scale = m_ignoreChildScale ? 1 : (mainIsX ? c.rect.localScale.x : c.rect.localScale.y);
            return minimum * scale + MainMarginsOf(c, mainIsX);
        }

        private float MainMarginsOf(ChildInfo c, bool mainIsX) {
            return mainIsX
                ? c.margins.left + c.margins.right
                : c.margins.top + c.margins.bottom;
        }

        private float AllocatedMainExtentOf(ChildInfo c, bool mainIsX) {
            float size = mainIsX ? c.size.x : c.size.y;
            return IsGrowChild(c, mainIsX) ? size + MainMarginsOf(c, mainIsX) : MainExtentOf(c, mainIsX);
        }

        private float CrossExtentOf(ChildInfo c, bool mainIsX) {
            if(mainIsX)
                return c.size.y + c.margins.top + c.margins.bottom;
            return c.size.x + c.margins.left + c.margins.right;
        }

        private void PlaceLinePrimaryAxis() {
            if(IsPrimaryAxis(RectTransform.Axis.Horizontal))
                PlaceRowPrimaryAxis();
            else
                PlaceColumnPrimaryAxis();
        }

        private void PlaceLineCrossAxis() {
            if(IsPrimaryAxis(RectTransform.Axis.Horizontal))
                PlaceRowCrossAxis();
            else
                PlaceColumnCrossAxis();
        }

        // ROW: primary axis is X. Each line spans the full inner width.
        private void PlaceRowPrimaryAxis() {
            float innerWidth = _rect.rect.width - m_padding.left - m_padding.right;

            foreach(FlexLine line in _lines) {
                bool reverse = m_direction == LayoutDirection.RowReverse;
                float lineSpacing = GetLineSpacing(line, innerWidth);
                float offset = reverse
                    ? _rect.rect.width - m_padding.right - GetJustifiedStartOffset(line.primarySize, innerWidth)
                    : m_padding.left + GetJustifiedStartOffset(line.primarySize, innerWidth);

                foreach(ChildInfo c in line.items) {
                    float pivot = c.size.x * c.rect.pivot.x;
                    if(!reverse) {
                        SetAnchorX(c.rect, 0);
                        offset += c.margins.left + pivot;
                        c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(offset);
                        offset += (c.size.x - pivot) + c.margins.right + lineSpacing;
                    }
                    else {
                        SetAnchorX(c.rect, 1);
                        offset -= c.margins.right;
                        float left = offset - c.size.x;
                        c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(left + pivot - _rect.rect.width);
                        offset = left - c.margins.left - lineSpacing;
                    }
                }
            }
        }

        // ROW: cross axis is Y. Lines stack with the cross axis gap.
        private void PlaceRowCrossAxis() {
            foreach(FlexLine line in _lines) {
                float lineTop = line.crossOffset;
                foreach(ChildInfo c in line.items) {
                    SetAnchorY(c.rect, 1);
                    float topInLine = GetAlignedTopInLine(c, line.crossSize);
                    c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(-(lineTop + topInLine + (1 - c.rect.pivot.y) * c.size.y));
                }
            }
        }

        // COLUMN: primary axis is Y. Each line (column) spans the full inner height.
        private void PlaceColumnPrimaryAxis() {
            float innerHeight = _rect.rect.height - m_padding.top - m_padding.bottom;

            foreach(FlexLine line in _lines) {
                bool reverse = m_direction == LayoutDirection.ColumnReverse;
                float lineSpacing = GetLineSpacing(line, innerHeight);
                float offset = reverse
                    ? _rect.rect.height - m_padding.bottom - GetJustifiedStartOffset(line.primarySize, innerHeight)
                    : m_padding.top + GetJustifiedStartOffset(line.primarySize, innerHeight);

                foreach(ChildInfo c in line.items) {
                    if(!reverse) {
                        SetAnchorY(c.rect, 1);
                        float topInColumn = offset + c.margins.top;
                        c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(-(topInColumn + (1 - c.rect.pivot.y) * c.size.y));
                        offset += c.size.y + c.margins.top + c.margins.bottom + lineSpacing;
                    }
                    else {
                        SetAnchorY(c.rect, 0);
                        offset -= c.margins.bottom;
                        float bottom = offset - c.size.y;
                        float anchoredY = _rect.rect.height - bottom + c.rect.pivot.y * c.size.y;
                        c.rect.anchoredPosition = c.rect.anchoredPosition.SetY(anchoredY);
                        offset = bottom - c.margins.top - lineSpacing;
                    }
                }
            }
        }

        // COLUMN: cross axis is X. Columns stack with the cross axis gap.
        private void PlaceColumnCrossAxis() {
            foreach(FlexLine line in _lines) {
                float columnLeft = line.crossOffset;
                foreach(ChildInfo c in line.items) {
                    SetAnchorX(c.rect, 0);
                    float leftInColumn = GetAlignedLeftInColumn(c, line.crossSize);
                    c.rect.anchoredPosition = c.rect.anchoredPosition.SetX(columnLeft + leftInColumn + c.rect.pivot.x * c.size.x);
                }
            }
        }

        private float GetJustifiedStartOffset(float linePrimary, float innerExtent) {
            switch(m_justifyContent) {
                case Justification.Center:
                    return Mathf.Max(0, (innerExtent - linePrimary) / 2);
                case Justification.End:
                    return Mathf.Max(0, innerExtent - linePrimary);
                default:
                    return 0;
            }
        }

        private float GetLineSpacing(FlexLine line, float innerExtent) {
            if(m_justifyContent != Justification.SpaceBetween || line.items.Count < 2)
                return m_innerSpacing;

            return m_innerSpacing + Mathf.Max(0, innerExtent - line.primarySize) / (line.items.Count - 1);
        }

        /// <summary>
        /// Top offset of an item inside its line for row layouts.
        /// </summary>
        private float GetAlignedTopInLine(ChildInfo c, float lineCross) {
            float extent = c.size.y + c.margins.top + c.margins.bottom;
            switch(m_alignItems) {
                case Alignment.Center:
                    return (lineCross - extent) / 2 + c.margins.top;
                case Alignment.End:
                    return lineCross - (c.margins.bottom + c.size.y);
                default:
                    return c.margins.top;
            }
        }

        /// <summary>
        /// Left offset of an item inside its column for column layouts.
        /// </summary>
        private float GetAlignedLeftInColumn(ChildInfo c, float lineCross) {
            float extent = c.size.x + c.margins.left + c.margins.right;
            switch(m_alignItems) {
                case Alignment.Center:
                    return (lineCross - extent) / 2 + c.margins.left;
                case Alignment.End:
                    return lineCross - (c.margins.right + c.size.x);
                default:
                    return c.margins.left;
            }
        }


        public virtual void GrowSizingXCallback(float yDiff) {
            Log($"X Grow Callback ({yDiff})");

            foreach(ChildInfo c in _children) {
                if(CheckIgnoreElem(c))
                    continue;

                c.margins = c.item ? c.item.Margin : c.margins;
                c.size.y = c.rect.rect.size.y * (m_ignoreChildScale ? 1 : c.rect.localScale.y);
            }
            
            float oldSize = _contentSize.y;
            float oldHeight = _rect.rect.size.y;
            
            // recalculate content size
            switch(m_direction) {
                case LayoutDirection.Row:
                case LayoutDirection.RowReverse:
                    _contentSize.y = 0;
                    foreach(ChildInfo c in _children) {
                        if(CheckIgnoreElem(c) || (c.item && c.item.SizeMode.y == SizingMode.Grow))
                            continue;

                        _contentSize.y = Mathf.Max(_contentSize.y, c.size.y + c.margins.top + c.margins.bottom);
                    }
                    break;
                case LayoutDirection.Column:
                case LayoutDirection.ColumnReverse:
                    _contentSize.y += yDiff;
                    break;
            }
            bool sizeChanged = !Mathf.Approximately(_contentSize.y, oldSize);
            
            if(m_sizing.y == SizingMode.FitContent && sizeChanged) {
                _rect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    ClampSize(RectTransform.Axis.Vertical, m_padding.top + m_padding.bottom + _contentSize.y)
                );
            }
            
            Log($"old content: {oldSize}, old height: {oldHeight}\nnew content: {_contentSize.y}, new height: {_rect.rect.height}");
            
            if(_parent is Layout parentLayout) {
                parentLayout.GrowSizingXCallback(yDiff);
            }
            else if(HasParentContainer) {
                _parent.SetDirty();
            }

            if(!_dirty && sizeChanged) {
                Log("forcing vertical layout update from x grow callback");
                GrowChildren(RectTransform.Axis.Vertical);
                VerticalLayout();
            }
        }
        
        public int CompareTo(Layout other) {
            if(_depth < other._depth) {
                return 1;
            }
            if(_depth == other._depth) {
                return 0;
            }
            
            return -1;
        }
        
        /// <summary>
        /// Refreshes the direct-child snapshot during the measurement pass.
        /// Lifecycle callbacks still request a rebuild, but measurement is the
        /// source of truth when hierarchy or child configuration changed.
        /// </summary>
        protected void RefreshChildrenForMeasurement() {
            LayoutUtil.RefreshChildCache(transform, _children, m_ignoreChildScale);
        }

        public void RefreshChildCache() {
            RefreshChildrenForMeasurement();
            _dirty = true;
            Log($"Refreshing child cache - {_children.Count} RectTransform children detected");
            if(isActiveAndEnabled)
                LayoutRebuilder.MarkLayoutForRebuild(_rect);
        }
    }
}

