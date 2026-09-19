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

namespace CupOHappiness.UI
{
    /// <summary>
    /// The parent-side seam used by every uLayout container.
    /// </summary>
    public interface IuLayoutContainer
    {
        RectTransform Rect { get; }
        void SetDirty();
        void RefreshChildCache();
    }

    [Flags]
    public enum LayoutAxis
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2
    }

    public static class LayoutUtil
    {
        private static readonly List<MonoBehaviour> s_behaviours = new();

        /// <summary>
        /// Shared state for a direct child of a uLayout container. Containers use
        /// the same cache so filtering, margins, and sizing metadata stay aligned.
        /// </summary>
        public sealed class ChildInfo
        {
            public int index;
            public RectTransform rect;
            public LayoutItem item;
            public Vector2 size;
            public Margins margins;

            public ChildInfo() {
            }

            public ChildInfo(int index, RectTransform rect, LayoutItem item, bool ignoreScale) {
                this.index = index;
                this.rect = rect;
                this.item = item;
                this.size = rect.rect.size * (ignoreScale ? Vector2.one : rect.localScale);
                this.margins = item ? item.Margin : default;
            }
        }

        public static void RefreshChildCache(Transform parent, List<ChildInfo> children, bool ignoreScale) {
            children.Clear();
            for(int i = 0; i < parent.childCount; i++) {
                RectTransform rect = parent.GetChild(i) as RectTransform;
                if(!rect)
                    continue;

                LayoutItem item = rect.GetComponent<LayoutItem>();
                children.Add(new ChildInfo(i, rect, item, ignoreScale));
            }
        }

        public static bool IsIgnored(ChildInfo child) {
            return ShouldIgnoreLayout(child.rect, child.item);
        }

        public static float ResolveChildSize(ChildInfo child, RectTransform.Axis axis, float availableSize, bool ignoreScale, bool applyRectTransform = true) {
            if(!child.item)
                return axis == RectTransform.Axis.Horizontal ? child.rect.rect.size.x : child.rect.rect.size.y;

            child.margins = child.item.Margin;
            float size = child.item.ResolveAxisSize(axis, availableSize);
            float current = axis == RectTransform.Axis.Horizontal ? child.rect.rect.size.x : child.rect.rect.size.y;
            if(applyRectTransform && !Mathf.Approximately(current, size)) {
                child.rect.SetSizeWithCurrentAnchors(axis, size);
                if(axis == RectTransform.Axis.Horizontal && child.item is LayoutText text)
                    text.HandleGrowSizingX();
            }

            float scaled = size * (ignoreScale ? 1 : (axis == RectTransform.Axis.Horizontal ? child.rect.localScale.x : child.rect.localScale.y));
            child.size = axis == RectTransform.Axis.Horizontal
                ? child.size.SetX(scaled)
                : child.size.SetY(scaled);
            return applyRectTransform
                ? (axis == RectTransform.Axis.Horizontal ? child.rect.rect.size.x : child.rect.rect.size.y)
                : size;
        }

        public static LayoutAxis FindCircularSizingAxes(LayoutItem item, LayoutItem.SizeModes childSizing, bool childIsFloating) {
            if(!item || childIsFloating || !item.isActiveAndEnabled)
                return LayoutAxis.None;

            RectTransform rect = item.transform as RectTransform;
            if(ShouldIgnoreLayout(rect, item))
                return LayoutAxis.None;

            LayoutItem parent = null;
            if(item.transform.parent) {
                foreach(MonoBehaviour behaviour in item.transform.parent.GetComponents<MonoBehaviour>()) {
                    if(behaviour is IuLayoutContainer && behaviour is LayoutItem layoutItem) {
                        parent = layoutItem;
                        break;
                    }
                }
            }
            if(!parent || !parent.isActiveAndEnabled)
                return LayoutAxis.None;

            LayoutAxis axes = LayoutAxis.None;
            LayoutItem.SizeModes parentSizing = parent.SizeMode;
            if(parentSizing.x == SizingMode.FitContent && DependsOnParent(childSizing.x))
                axes |= LayoutAxis.Horizontal;
            if(parentSizing.y == SizingMode.FitContent && DependsOnParent(childSizing.y))
                axes |= LayoutAxis.Vertical;

            return axes;
        }

        public static bool ShouldIgnoreLayout(RectTransform rect, LayoutItem item = null) {
            if(!rect || !rect.gameObject.activeInHierarchy)
                return true;

            if(item && (!item.isActiveAndEnabled || item.IsFloating))
                return true;

            rect.GetComponents(s_behaviours);
            foreach(MonoBehaviour behaviour in s_behaviours) {
                if(behaviour && behaviour.isActiveAndEnabled && behaviour is ILayoutIgnorer ignorer && ignorer.ignoreLayout) {
                    s_behaviours.Clear();
                    return true;
                }
            }
            s_behaviours.Clear();

            return false;
        }

        private static bool DependsOnParent(SizingMode mode) {
            return mode == SizingMode.Grow || mode == SizingMode.Percent;
        }

        public static void DrawCenteredDebugBox(Vector3 pos, float w, float h, Color color) {
            DrawDebugBox(pos - new Vector3(w/2, h/2, 0), w, h, color);
        }

        /// <summary>
        /// Draws a box out of debug rays, positioned at the bottom left corner.
        /// </summary>
        public static void DrawDebugBox(Vector3 pos, float w, float h, Color color) {
            Gizmos.color = color;
            // left
            Gizmos.DrawLine(pos, pos + Vector3.up * h);
            // bottom
            Gizmos.DrawLine(pos, pos + Vector3.right * w);
            // right
            Gizmos.DrawLine(pos + new Vector3(w, h), pos + new Vector3(w, h) + Vector3.down * h);
            // top
            Gizmos.DrawLine(pos + new Vector3(w, h), pos + new Vector3(w, h) + Vector3.left * w);
        }

        public static void DrawDebugBox(Rect rect, float z, Color color) {
            DrawDebugBox((Vector3)rect.position + new Vector3(0, 0, z), rect.width, rect.height, color);
        }
        
        public static Vector2 SetX(this Vector2 v, float x) {
            return new Vector2 (x, v.y);
        }
        
        public static Vector2 SetY(this Vector2 v, float y) {
            return new Vector2 (v.x, y);
        }
    }
    
    [System.Serializable]
    public struct Margins : IEquatable<Margins>
    {
        [Tooltip("Space above the object.")]
        public float top;
        [Tooltip("Space below the object.")]
        public float bottom;
        [Tooltip("Space on the left side of the object.")]
        public float left;
        [Tooltip("Space on the right side of the object.")]
        public float right;

        public bool Equals(Margins other) {
            return top.Equals(other.top) && bottom.Equals(other.bottom) && left.Equals(other.left) && right.Equals(other.right);
        }

        public override bool Equals(object obj) {
            return obj is Margins other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(top, bottom, left, right);
        }
    }
    
    public enum SizingMode
    {
        FitContent,
        Fixed,
        Grow,
        Percent,
    }

    public enum AttachPoint
    {
        LeftTop,
        LeftCenter,
        LeftBottom,
        CenterTop,
        CenterCenter,
        CenterBottom,
        RightTop,
        RightCenter,
        RightBottom,
    }

    [System.Serializable]
    public struct AttachPoints
    {
        [Tooltip("Point on this floating element that will be attached.")]
        public AttachPoint element;
        [Tooltip("Point on the target RectTransform that this element will attach to.")]
        public AttachPoint parent;
    }
}

