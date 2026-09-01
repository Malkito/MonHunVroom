/*
    Copyright (c) 2026 Alex Howe

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

namespace CupOHappiness.UI
{
    public class Layout : LayoutItem, IComparable<Layout>, ILayoutGroup
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
        [SerializeField] private Margins            m_padding;
        [Tooltip("Main direction used to place children.")]
        [SerializeField] private LayoutDirection    m_direction;
        [Tooltip("How children are spread along the main axis.")]
        [SerializeField] private Justification      m_justifyContent;
        [Tooltip("How children line up on the cross axis.")]
        [SerializeField] private Alignment          m_alignContent;
        [Tooltip("Gap between children on the main axis. This is usually set to 0 when using Space Between.")]
        [SerializeField] private float              m_innerSpacing;
        [Tooltip("If enabled, child scale is ignored when this layout measures and places children.")]
        [SerializeField] private bool               m_ignoreChildScale;

        public int ChildCount =>            _children?.Count ?? 0;
        public Vector2Int GrowChildCount => _growChildCount;
        
        private readonly List<ChildInfo>    _children = new();
        private Vector2                     _contentSize;
        private int                         _depth;
        private Vector2Int                  _growChildCount;
        private int                         _ignoreCount;
        private readonly Vector3[]          _rectCorners = new Vector3[4];
        
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

        private class ChildInfo
        {
            public int index;
            public RectTransform rect;
            public LayoutItem li;
            public bool isLayout;
            public Vector2 size;
            public Margins margins;
            public Vector2 percentage;
            public Vector2 minSize;
            public Vector2 maxSize;
            public bool isFloating;
            public AttachPoints attachPoints;
            public Vector2 offset;
            public LayoutItem.FloatingAttachTo attachTo;
            public RectTransform attachTarget;
            public bool enabled;
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
                            ResolveChildSize(c, RectTransform.Axis.Horizontal);
                            c.size = c.size.SetX(c.rect.rect.size.x * (m_ignoreChildScale ? 1 : c.rect.localScale.x));
                        }
                    }

                    float primarySize = m_justifyContent == Justification.SpaceBetween ? 0 : m_innerSpacing * (_children.Count-_ignoreCount-1);
                    float crossSize = 0;
                    
                    // calculate content size
                    float maxCrossSize = 0;
                    foreach(ChildInfo c in _children) {
                        // skip disabled/ignore items
                        if(CheckIgnoreElem(c))
                            continue;
                        
                        bool grow = false;
                        if(c.li) {
                            grow = c.li.SizeMode.x == SizingMode.Grow;
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
                    
                    // apply fit sizing X
                    if(m_sizing.x == SizingMode.FitContent) {
                        switch(m_direction) {
                            case LayoutDirection.Row:
                            case LayoutDirection.RowReverse:
                                _rect.SetSizeWithCurrentAnchors(
                                    RectTransform.Axis.Horizontal,
                                    ClampSize(RectTransform.Axis.Horizontal, primarySize + m_padding.left + m_padding.right)
                                );
                                break;
                            case LayoutDirection.Column:
                            case LayoutDirection.ColumnReverse:
                                _rect.SetSizeWithCurrentAnchors(
                                    RectTransform.Axis.Horizontal,
                                    ClampSize(RectTransform.Axis.Horizontal, crossSize + m_padding.left + m_padding.right)
                                );
                                break;
                        }
                    }
                    
                    Log($"calculated rect x size: {_rect.rect.size.x:f3}");
                }
                else {
                    _contentSize = Vector2.zero;
                }
                
                Log($"content x size: {_contentSize.x:f3}");
            }
        }
        
        public override void CalculateLayoutInputVertical() {
            if(_dirty) {
                Log("CalculateLayoutInputVertical");
                
                _growChildCount.y = 0;
                
                if(_children.Count > 0) {
                    foreach(ChildInfo c in _children) {
                        if(!CheckIgnoreElem(c)) {
                            ResolveChildSize(c, RectTransform.Axis.Vertical);
                            c.size = c.size.SetY(c.rect.rect.size.y * (m_ignoreChildScale ? 1 : c.rect.localScale.y));
                        }
                    }
                    
                    float primarySize = m_justifyContent == Justification.SpaceBetween ? 0 : m_innerSpacing * (_children.Count-_ignoreCount-1);
                    float crossSize = 0;
                    
                    // calculate content size
                    float maxCrossSize = 0;
                    foreach(ChildInfo c in _children) {
                        // skip disabled/ignore items
                        if(CheckIgnoreElem(c))
                            continue;
                        
                        bool grow = false;
                        if(c.li) {
                            grow = c.li.SizeMode.y == SizingMode.Grow;
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
                    
                    // apply fit sizing X
                    if(m_sizing.y == SizingMode.FitContent) {
                        switch(m_direction) {
                            case LayoutDirection.Row:
                            case LayoutDirection.RowReverse:
                                _rect.SetSizeWithCurrentAnchors(
                                    RectTransform.Axis.Vertical,
                                    ClampSize(RectTransform.Axis.Vertical, crossSize + m_padding.top + m_padding.bottom)
                                );
                                break;
                            case LayoutDirection.Column:
                            case LayoutDirection.ColumnReverse:
                                _rect.SetSizeWithCurrentAnchors(
                                    RectTransform.Axis.Vertical,
                                    ClampSize(RectTransform.Axis.Vertical, primarySize + m_padding.top + m_padding.bottom)
                                );
                                break;
                        }
                    }
                    
                    Log($"calculated rect y size: {_rect.rect.size.y:f3}");
                }
                else {
                    _contentSize = Vector2.zero;
                }
                
                Log($"content x size: {_contentSize.y:f3}");
            }
        }

        public void SetLayoutHorizontal() {
            if(_dirty) {
                Log("SetLayoutHorizontal");
                GrowChildren(RectTransform.Axis.Horizontal);
                HorizontalLayout();
            }
        }
        
        public void SetLayoutVertical() {
            if(_dirty) {
                Log("SetLayoutVertical");
                GrowChildren(RectTransform.Axis.Vertical);
                VerticalLayout();
            }

            _dirty = false;
        }
        #endregion
        
        #region Layout Internal
        private void Log(object msg) {
            if(m_log) Debug.Log($"[{_frame}] [L:{gameObject.name}]: {msg}");
        }
        
        private bool CheckIgnoreElem(ChildInfo ci) {
            return !ci.enabled || ci.isFloating;
        }

        private void SetAnchorX(RectTransform rt, float x) {
            rt.anchorMin = rt.anchorMin.SetX(x);
            rt.anchorMax = rt.anchorMax.SetX(x);
        }
        private void SetAnchorY(RectTransform rt, float y) {
            rt.anchorMin = rt.anchorMin.SetY(y);
            rt.anchorMax = rt.anchorMax.SetY(y);
        }

        private void HorizontalLayout() {
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
                                spacing = leftover / (_children.Count-_ignoreCount-1);
                            
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
                                spacing = leftover / (_children.Count-_ignoreCount-1);
                                
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
                    switch(m_alignContent) {
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

        private void VerticalLayout() {
            Log($"Vertical Layout - content size y: {_contentSize.y}");
            
            float offset = 0;
            float leftover;
            float spacing = 0;
            int index = 0;
            switch(m_direction) {
                // ROW/ROW-REVERSE -> CROSS AXIS
                case LayoutDirection.Row:
                case LayoutDirection.RowReverse:
                    switch(m_alignContent) {
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
                                spacing = leftover / (_children.Count-_ignoreCount-1);
                                
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
                                spacing = leftover / (_children.Count-_ignoreCount-1);
                                
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
        
        private void GrowChildren(RectTransform.Axis axis) {
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

        private void ApplyPrimaryAxisGrow(RectTransform.Axis axis) {
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
                    float size = child.li.ResolveAxisSize(axis, share);
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
                    resolvedSizes[i] = growChildren[i].li.ResolveAxisSize(axis, finalShare);
                }
            }

            for(int i = 0; i < growChildren.Count; i++) {
                ApplyResolvedGrowSize(growChildren[i], axis, resolvedSizes[i], primaryAxis: true);
            }
        }

        private void ApplyCrossAxisGrow(RectTransform.Axis axis) {
            List<ChildInfo> growChildren = GetGrowChildren(axis);
            foreach(ChildInfo child in growChildren) {
                float available = GetInnerAvailableSize(axis, child);
                float size = child.li.ResolveAxisSize(axis, available);
                ApplyResolvedGrowSize(child, axis, size, primaryAxis: false);
            }
        }

        private List<ChildInfo> GetGrowChildren(RectTransform.Axis axis) {
            List<ChildInfo> growChildren = new();
            foreach(ChildInfo c in _children) {
                if(!c.li || CheckIgnoreElem(c))
                    continue;

                SizingMode mode = axis == RectTransform.Axis.Horizontal ? c.li.SizeMode.x : c.li.SizeMode.y;
                if(mode == SizingMode.Grow) {
                    growChildren.Add(c);
                }
            }
            return growChildren;
        }

        private float GetPrimaryAxisLeftover(RectTransform.Axis axis) {
            if(axis == RectTransform.Axis.Horizontal) {
                return _rect.rect.size.x - _contentSize.x - m_padding.left - m_padding.right;
            }

            return _rect.rect.size.y - _contentSize.y - m_padding.top - m_padding.bottom;
        }

        private void ApplyResolvedGrowSize(ChildInfo child, RectTransform.Axis axis, float size, bool primaryAxis) {
            Log($"growing \"{child.li.name}\" {(axis == RectTransform.Axis.Horizontal ? "x" : "y")} axis ({size})");

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

            if(axis == RectTransform.Axis.Horizontal && child.li is LayoutText t) {
                t.HandleGrowSizingX();
                float updatedScaledHeight = child.rect.rect.size.y * (m_ignoreChildScale ? 1 : child.rect.localScale.y);
                float diff = updatedScaledHeight - child.size.y;
                if(!Mathf.Approximately(diff, 0)) {
                    child.size.y = updatedScaledHeight;
                    GrowSizingXCallback(diff);
                }
            }
        }

        private float GetInnerAvailableSize(RectTransform.Axis axis, ChildInfo child) {
            float available = axis == RectTransform.Axis.Horizontal
                ? _rect.rect.size.x - m_padding.left - m_padding.right
                : _rect.rect.size.y - m_padding.top - m_padding.bottom;

            if(child.li) {
                if(axis == RectTransform.Axis.Horizontal) {
                    available -= child.margins.left + child.margins.right;
                }
                else {
                    available -= child.margins.top + child.margins.bottom;
                }
            }

            return Mathf.Max(0, available);
        }

        private bool IsPrimaryAxis(RectTransform.Axis axis) {
            return axis switch {
                RectTransform.Axis.Horizontal => m_direction == LayoutDirection.Row || m_direction == LayoutDirection.RowReverse,
                RectTransform.Axis.Vertical => m_direction == LayoutDirection.Column || m_direction == LayoutDirection.ColumnReverse,
                _ => false
            };
        }

        private float GetPrimaryAxisPercentAvailable(RectTransform.Axis axis) {
            float available = axis == RectTransform.Axis.Horizontal
                ? _rect.rect.size.x - m_padding.left - m_padding.right
                : _rect.rect.size.y - m_padding.top - m_padding.bottom;

            foreach(ChildInfo c in _children) {
                if(CheckIgnoreElem(c))
                    continue;

                Margins margins = c.li ? c.li.Margin : c.margins;
                if(axis == RectTransform.Axis.Horizontal) {
                    available -= margins.left + margins.right;
                }
                else {
                    available -= margins.top + margins.bottom;
                }
            }

            return Mathf.Max(0, available);
        }

        private void ResolveChildSize(ChildInfo child, RectTransform.Axis axis) {
            if(!child.li || CheckIgnoreElem(child))
                return;

            child.margins = child.li.Margin;

            SizingMode mode = axis == RectTransform.Axis.Horizontal ? child.li.SizeMode.x : child.li.SizeMode.y;
            if(mode == SizingMode.Grow)
                return;

            float availableSize = GetInnerAvailableSize(axis, child);
            if(mode == SizingMode.Percent && IsPrimaryAxis(axis)) {
                availableSize = GetPrimaryAxisPercentAvailable(axis);
            }

            float resolvedSize = child.li.ResolveAxisSize(axis, availableSize);
            float currentSize = axis == RectTransform.Axis.Horizontal ? child.rect.rect.size.x : child.rect.rect.size.y;

            if(!Mathf.Approximately(currentSize, resolvedSize)) {
                child.rect.SetSizeWithCurrentAnchors(axis, resolvedSize);

                if(axis == RectTransform.Axis.Horizontal && child.li is LayoutText t) {
                    t.HandleGrowSizingX();
                }
            }

            float scaledSize = resolvedSize * (m_ignoreChildScale ? 1 : (axis == RectTransform.Axis.Horizontal ? child.rect.localScale.x : child.rect.localScale.y));
            if(axis == RectTransform.Axis.Horizontal) {
                child.size = child.size.SetX(scaledSize);
            }
            else {
                child.size = child.size.SetY(scaledSize);
            }
        }
        #endregion

        public void GrowSizingXCallback(float yDiff) {
            Log($"X Grow Callback ({yDiff})");

            // remove grow items from calculated content size
            foreach(ChildInfo c in _children) {
                if(CheckIgnoreElem(c))
                    continue;

                if(c.li && c.li.SizeMode.y == SizingMode.Grow) {
                    _contentSize.y -= c.rect.rect.size.y;
                }
                else {
                    c.size.y = c.rect.rect.size.y;
                }
            }
            
            float oldSize = _contentSize.y;
            float oldHeight = _rect.rect.size.y;
            
            // recalculate content size
            switch(m_direction) {
                case LayoutDirection.Row:
                case LayoutDirection.RowReverse:
                    _contentSize.y = 0;
                    foreach(ChildInfo c in _children) {
                        if(CheckIgnoreElem(c) || (c.li && c.li.SizeMode.y == SizingMode.Grow))
                            continue;

                        _contentSize.y = Mathf.Max(_contentSize.y, c.size.y);
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
            
            if(_parent)
                _parent.GrowSizingXCallback(yDiff);

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
        
        public void RefreshChildCache() {
            _children.Clear();
            
            int childCount = transform.childCount;
            Log($"Refreshing child cache - {childCount} children detected");
            
            for(int i = 0; i < childCount; i++) {
                RectTransform rt = transform.GetChild(i).GetComponent<RectTransform>();
                
                Log($"Adding child \"{rt.name}\" - size: {rt.rect.size}");
                
                LayoutItem li = rt.GetComponent<LayoutItem>();
                
                _children.Add(
                    new ChildInfo {
                        index = i,
                        rect = rt,
                        li = li,
                        size = rt.rect.size * (m_ignoreChildScale ? Vector2.one : rt.localScale),
                        margins = li ? li.Margin : default,
                        percentage = li ? li.Percentage : Vector2.one,
                        minSize = li ? li.MinSize : Vector2.zero,
                        maxSize = li ? li.MaxSize : new Vector2(float.PositiveInfinity, float.PositiveInfinity),
                        isFloating = li && li.IsFloating,
                        attachTo = li ? li.AttachTo : default,
                        attachPoints = li ? li.AttachPointsConfig : default,
                        offset = li ? li.Offset : default,
                        attachTarget = li ? li.AttachTarget : null,
                        enabled = rt.gameObject.activeInHierarchy
                    }
                );
            }
            
            LayoutRebuilder.MarkLayoutForRebuild(_rect);
        }
    }
}
