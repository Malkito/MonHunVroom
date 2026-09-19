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
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CupOHappiness.UI
{
    [
        ExecuteAlways,
        RequireComponent(typeof(RectTransform))
    ]
    public class LayoutItem : MonoBehaviour, ILayoutElement
    {
        [Tooltip("Writes layout debug messages to the Console. Leave this off unless you are diagnosing a layout problem.")]
        [SerializeField] protected bool m_log;
        
        [Header("Layout Item")]
        [Tooltip("Extra space outside this object. The parent Layout includes these values when placing children.")]
        [SerializeField] protected Margins m_margin;
        [Tooltip("Chooses how this object gets its width and height. Fixed keeps the current RectTransform size, Grow fills available space, Percent uses part of the parent, and Fit Content sizes to content when supported.")]
        [SerializeField] protected SizeModes m_sizing = new SizeModes { x = SizingMode.Fixed, y = SizingMode.Fixed };
        [Tooltip("Used when Size Mode is Percent. Values are from 0 to 1, where 0.5 means 50% of the available size.")]
        [SerializeField] protected Vector2 m_percentage = Vector2.one;
        [Tooltip("Smallest width and height this layout system is allowed to set.")]
        [SerializeField] protected Vector2 m_minSize = Vector2.zero;
        [Tooltip("Largest width and height this layout system is allowed to set.")]
        [SerializeField] protected Vector2 m_maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        [Header("Floating")]
        [Tooltip("If enabled, this object is positioned as a floating element and does not take up space in normal layout flow.")]
        [SerializeField] protected bool m_isFloating = false;
        [Tooltip("Chooses what this floating element attaches to. None means this object stays outside normal layout flow but keeps its own manual RectTransform position.")]
        [SerializeField] protected FloatingAttachTo m_attachTo;
        [Tooltip("Sets which point on this element connects to which point on the target.")]
        [SerializeField] protected AttachPoints m_attachPoints;
        [Tooltip("Adds an extra local offset after the floating attachment point is calculated.")]
        [SerializeField] protected Vector2 m_offset = Vector2.zero;
        [Tooltip("Target RectTransform used when Attach To is set to RectTransform.")]
        [SerializeField] protected RectTransform m_attachTarget;
        [Tooltip("Expands the floating element equally on both sides after its main size is calculated. This is useful for hit areas, highlights, or outlines.")]
        [SerializeField] protected Vector2 m_floatingExpand = Vector2.zero;
        [Tooltip("Moves this floating element to the end of its sibling list so Unity draws it on top of its siblings.")]
        [SerializeField] protected bool m_bringToFront = true;

        protected float _minWidth;
        protected float _preferredWidth;
        protected float _flexibleWidth;
        protected float _minHeight;
        protected float _preferredHeight;
        protected float _flexibleHeight;
        protected int _layoutPriority;

        public float minWidth => _minWidth;
        public float preferredWidth => _preferredWidth;
        public float flexibleWidth => _flexibleWidth;
        public float minHeight => _minHeight;
        public float preferredHeight => _preferredHeight;
        public float flexibleHeight => _flexibleHeight;
        public int layoutPriority => _layoutPriority;
        
        public Margins Margin {
            get => m_margin;
            set { if(!m_margin.Equals(value)) { m_margin = value; SetDirty(); } }
        }
        public Vector2 Percentage {
            get => m_percentage;
            set { if(m_percentage != value) { m_percentage = value; SetDirty(); } }
        }
        public Vector2 MinSize {
            get => m_minSize;
            set { if(m_minSize != value) { m_minSize = value; SetDirty(); } }
        }
        public Vector2 MaxSize {
            get => m_maxSize;
            set { if(m_maxSize != value) { m_maxSize = value; SetDirty(); } }
        }
        public bool IsFloating {
            get => m_isFloating;
            set { if(m_isFloating != value) { m_isFloating = value; SetDirty(); } }
        }
        public FloatingAttachTo AttachTo {
            get => m_attachTo;
            set { if(m_attachTo != value) { m_attachTo = value; SetDirty(); } }
        }
        public AttachPoints AttachPointsConfig {
            get => m_attachPoints;
            set { m_attachPoints = value; SetDirty(); }
        }
        public Vector2 Offset {
            get => m_offset;
            set { if(m_offset != value) { m_offset = value; SetDirty(); } }
        }
        public RectTransform AttachTarget {
            get => m_attachTarget;
            set { if(m_attachTarget != value) { m_attachTarget = value; SetDirty(); } }
        }
        public Vector2 FloatingExpand {
            get => m_floatingExpand;
            set { if(m_floatingExpand != value) { m_floatingExpand = value; SetDirty(); } }
        }
        public bool BringToFront {
            get => m_bringToFront;
            set { if(m_bringToFront != value) { m_bringToFront = value; SetDirty(); } }
        }
        public RectTransform Rect => _rect;
        public DrivenTransformProperties TrackerProps {
            get => _trackerProps;
            set => _trackerProps = value;
        }
        public SizeModes SizeMode {
            get => m_sizing;
            set { if(!m_sizing.Equals(value)) { m_sizing = value; SetDirty(); } }
        }
        
        protected RectTransform _rect;
        protected DrivenRectTransformTracker _tracker;
        protected DrivenTransformProperties _trackerProps;
        protected RectTransform _parentRect;
        protected IuLayoutContainer _parent;
        protected bool HasParentContainer => _parent is MonoBehaviour parentBehaviour && parentBehaviour;
        protected bool _dirty = true;
        protected int _frame;

        /// <summary>
        /// Publishes the measurement contract consumed by Unity layout groups.
        /// A flexible value of -1 means that this axis does not expand.
        /// </summary>
        protected void SetLayoutInput(RectTransform.Axis axis, float minimum, float preferred, float flexible = -1) {
            minimum = Mathf.Max(0, SanitizeMeasurement(minimum));
            preferred = Mathf.Max(minimum, SanitizeMeasurement(preferred));
            flexible = flexible < 0 ? -1 : SanitizeMeasurement(flexible);

            if(axis == RectTransform.Axis.Horizontal) {
                _minWidth = minimum;
                _preferredWidth = preferred;
                _flexibleWidth = flexible;
            }
            else {
                _minHeight = minimum;
                _preferredHeight = preferred;
                _flexibleHeight = flexible;
            }

            Log($"Publish input axis={axis} minimum={minimum:F3} preferred={preferred:F3} flexible={flexible:F3}");
        }

        private static float SanitizeMeasurement(float value) {
            return float.IsNaN(value) || float.IsNegativeInfinity(value) ? 0 : value == float.PositiveInfinity ? 0 : value;
        }
        private bool _warnedPercentX;
        private bool _warnedPercentY;
        private bool _warnedMissingFloatingTarget;
        private bool _warnedSelfFloatingTarget;
        private bool _warnedConstraintX;
        private bool _warnedConstraintY;
        private bool _warnedCircularX;
        private bool _warnedCircularY;
        private bool _applyingFloatingLayout;
        private bool _hasAppliedFloatingExpand;
        private Vector2 _lastFloatingBaseSize;
        private Vector2 _lastExpandedFloatingSize;
        private RectTransform _lastFloatingTarget;
        private Rect _lastFloatingTargetRect;
        private Matrix4x4 _lastFloatingTargetLocalToWorld;
        private Rect _lastFloatingParentRect;
        private Matrix4x4 _lastFloatingParentWorldToLocal;
        private bool _hasFloatingTargetState;
        private readonly Vector3[] _floatingTargetCorners = new Vector3[4];
        #if UNITY_EDITOR
        private bool _deferRectTransformWrites;
        private static bool s_globalValidationLock;
        #endif
        
        [Serializable]
        public struct SizeModes
        {
            [Tooltip("How this object gets its width.")]
            public SizingMode x;
            [Tooltip("How this object gets its height.")]
            public SizingMode y;
        }

        public enum FloatingAttachTo
        {
            None,
            Parent,
            Root,
            RectTransform
        }

        #region LayoutItem MonoBehavior
        protected virtual void Awake() {
            Log("awake");
            
            EnsureReferences();
            CacheParentReferences();
        }

        protected virtual void OnEnable() {
            EnsureReferences();
            CacheParentReferences();
            _trackerProps = DrivenTransformProperties.None;
            SetDirty();
        }

        protected virtual void OnDisable() {
            _tracker.Clear();
            NotifyParentLayoutChanged(refreshCache: true);
        }

        protected virtual void OnTransformParentChanged() {
            EnsureReferences();
            CacheParentReferences();
            SetDirty();
        }

        protected virtual void OnRectTransformDimensionsChange() {
            if(_applyingFloatingLayout)
                return;

            EnsureReferences();
            Log("RectTransform dimensions changed");
            SetDirty();
        }

        protected virtual void OnCanvasHierarchyChanged() {
            EnsureReferences();
            SetDirty();
        }

        protected virtual void OnDidApplyAnimationProperties() {
            SetDirty();
        }

        protected virtual void OnValidate() {
            #if UNITY_EDITOR
            s_globalValidationLock = true;
            try {
            #endif
                EnsureReferences();
                SanitizeConfiguration(logWarnings: false);
                CacheParentReferences();
            #if UNITY_EDITOR
                _deferRectTransformWrites = true;
            #endif
                SetDirty();
            #if UNITY_EDITOR
                QueueEditorRefresh();
            } finally {
                s_globalValidationLock = false;
            }
            #endif
        }

        public virtual void Update() {
            EnsureReferences();
            _frame = Time.frameCount;

            RefreshFloatingLayoutForTargetChanges();
            
            #if UNITY_EDITOR
            _tracker.Clear();
            _trackerProps = DrivenTransformProperties.None;
            
            SetDrivenProperties();
            
            _tracker.Add(this, _rect, _trackerProps);
            #endif
        }
        #endregion

        private void Log(object msg) {
            // Layout containers publish one summary per completed cycle. Leaf-level
            // lifecycle events are too frequent to be useful in the Console and made
            // one layout rebuild appear as dozens of unrelated messages.
        }

        protected string GetDebugState() {
            Vector2 rectSize = _rect ? _rect.rect.size : Vector2.zero;
            Vector2 parentSize = _parentRect ? _parentRect.rect.size : Vector2.zero;
            Vector3 scale = _rect ? _rect.localScale : Vector3.one;
            int cycle = this is Layout layout ? layout.LayoutCycle : _parent is Layout parentLayout ? parentLayout.LayoutCycle : -1;
            string targetName = m_attachTarget ? m_attachTarget.name : "none";
            return $"dirty={_dirty} rect={rectSize} parent={parentSize} scale={scale} " +
                $"cycle={cycle} " +
                $"sizing=({m_sizing.x},{m_sizing.y}) percent={m_percentage} min={m_minSize} max={m_maxSize} margin={m_margin} " +
                $"floating={m_isFloating}/{m_attachTo} target={targetName}";
        }
        
        protected virtual void SetDrivenProperties() {
            if((m_sizing.x == SizingMode.FitContent && transform.childCount > 0) || m_sizing.x == SizingMode.Grow || m_sizing.x == SizingMode.Percent)
                _trackerProps |= DrivenTransformProperties.SizeDeltaX;
            if((m_sizing.y == SizingMode.FitContent && transform.childCount > 0) || m_sizing.y == SizingMode.Grow || m_sizing.y == SizingMode.Percent)
                _trackerProps |= DrivenTransformProperties.SizeDeltaY;

            if(HasParentContainer && !m_isFloating) {
                _trackerProps |= DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Anchors;
            }

            if(m_isFloating) {
                _trackerProps |= DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Anchors;
            }
        }

        public virtual void SetDirty() {
            EnsureReferences();
            if(!_rect)
                return;

            _dirty = true;
            Log("SetDirty");
            NotifyParentLayoutChanged();

            if(!HasParentContainer) {
                HandleStandaloneSizing();
            }

            if(CanWriteRectTransformNow()) {
                if(m_isFloating) {
                    ApplyFloatingLayout();
                }
                else {
                    RemoveFloatingExpand();
                }
            }

            if(isActiveAndEnabled) {
                RectTransform rebuildTarget = HasParentContainer ? _parent.Rect : _rect;
                LayoutRebuilder.MarkLayoutForRebuild(rebuildTarget);
            }
        }

        public virtual void CalculateLayoutInputHorizontal() {
            Log("CalculateLayoutInputHorizontal");
            SetLayoutInput(RectTransform.Axis.Horizontal, m_minSize.x, _rect ? _rect.rect.width : 0);
        }
        public virtual void CalculateLayoutInputVertical() {
            Log("CalculateLayoutInputVertical");
            SetLayoutInput(RectTransform.Axis.Vertical, m_minSize.y, _rect ? _rect.rect.height : 0);
        }

        public float ResolveAxisSize(RectTransform.Axis axis, float availableSize) {
            SanitizeConfiguration(logWarnings: true);

            SizingMode mode = axis == RectTransform.Axis.Horizontal ? m_sizing.x : m_sizing.y;
            float currentSize = axis == RectTransform.Axis.Horizontal ? _rect.rect.size.x : _rect.rect.size.y;
            float percent = axis == RectTransform.Axis.Horizontal ? GetSanitizedPercentX() : GetSanitizedPercentY();

            float size = mode switch {
                SizingMode.Grow => availableSize,
                SizingMode.Percent => availableSize * percent,
                SizingMode.FitContent => axis == RectTransform.Axis.Horizontal
                    ? (_preferredWidth > 0 ? _preferredWidth : currentSize)
                    : (_preferredHeight > 0 ? _preferredHeight : currentSize),
                _ => currentSize
            };

            float resolved = ClampSize(axis, size);
            Log($"Resolve axis={axis} mode={mode} available={availableSize:F3} current={currentSize:F3} percent={percent:F3} raw={size:F3} resolved={resolved:F3}");
            return resolved;
        }

        protected float ClampSize(RectTransform.Axis axis, float size) {
            float min = axis == RectTransform.Axis.Horizontal ? m_minSize.x : m_minSize.y;
            float max = axis == RectTransform.Axis.Horizontal ? m_maxSize.x : m_maxSize.y;
            return Mathf.Clamp(size, min, max);
        }

        protected virtual void ApplyFloatingLayout() {
            if(_applyingFloatingLayout)
                return;

            if(m_attachTo == FloatingAttachTo.None) {
                _warnedMissingFloatingTarget = false;
                RemoveFloatingExpand();
                return;
            }

            EnsureReferences();
            if(!_rect)
                return;

            RectTransform target = ResolveFloatingTarget();
            RectTransform parentRect = _rect.parent as RectTransform;
            if(!parentRect)
                return;

            if(!target) {
                WarnMissingFloatingTarget();
                return;
            }

            _warnedMissingFloatingTarget = false;
            _applyingFloatingLayout = true;
            try {
                Vector2 targetSize = GetTargetSizeInParentSpace(target, parentRect);
                bool resolvedWidth = m_sizing.x == SizingMode.Grow || m_sizing.x == SizingMode.Percent;
                bool resolvedHeight = m_sizing.y == SizingMode.Grow || m_sizing.y == SizingMode.Percent;
                if(resolvedWidth) {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ResolveAxisSize(RectTransform.Axis.Horizontal, targetSize.x));
                }
                if(resolvedHeight) {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ResolveAxisSize(RectTransform.Axis.Vertical, targetSize.y));
                }

                ApplyFloatingExpand(resolvedWidth, resolvedHeight);

                Vector2 parentAttach = GetAttachPointNormalized(m_attachPoints.parent);
                Vector2 elementAttach = GetAttachPointNormalized(m_attachPoints.element);
                Vector2 anchor = GetAnchorForTargetPoint(target, parentRect, parentAttach);
                Vector2 pivotOffset = new(
                    (_rect.pivot.x - elementAttach.x) * _rect.rect.width,
                    (_rect.pivot.y - elementAttach.y) * _rect.rect.height
                );

                _rect.anchorMin = anchor;
                _rect.anchorMax = anchor;
                _rect.anchoredPosition = m_offset + pivotOffset;

                Log($"Floating apply target={target.name} targetRect={target.rect} targetSizeInParent={targetSize} parentRect={parentRect.rect} anchor={anchor} pivotOffset={pivotOffset} offset={m_offset} resultSize={_rect.rect.size} resultPosition={_rect.anchoredPosition}");

                if(m_bringToFront && _rect.parent && CanWriteRectTransformNow()) {
                    _rect.SetAsLastSibling();
                }

                CacheFloatingTargetState(target, parentRect);
            }
            finally {
                _applyingFloatingLayout = false;
            }
        }

        protected RectTransform ResolveFloatingTarget() {
            EnsureReferences();
            if(!_rect)
                return null;

            RectTransform target = m_attachTo switch {
                FloatingAttachTo.Parent => _rect.parent as RectTransform,
                FloatingAttachTo.Root => ResolveRootRectTransform(),
                FloatingAttachTo.RectTransform => m_attachTarget,
                _ => null
            };

            if(target == _rect) {
                if(!_warnedSelfFloatingTarget) {
                    Debug.LogWarning($"[uLayout] Floating target cannot reference itself on \"{name}\".", this);
                    _warnedSelfFloatingTarget = true;
                }
                return null;
            }

            _warnedSelfFloatingTarget = false;
            return target;
        }

        protected RectTransform ResolveRootRectTransform() {
            EnsureReferences();
            if(!_rect)
                return null;

            Canvas canvas = GetComponentInParent<Canvas>();
            if(canvas && canvas.rootCanvas) {
                RectTransform rootCanvasRect = canvas.rootCanvas.transform as RectTransform;
                if(rootCanvasRect) {
                    return rootCanvasRect;
                }
            }

            RectTransform current = _rect;
            RectTransform last = _rect;
            while(current) {
                last = current;
                current = current.parent as RectTransform;
            }
            return last;
        }

        protected static Vector2 GetAttachPointNormalized(AttachPoint point) {
            return point switch {
                AttachPoint.LeftTop => new Vector2(0, 1),
                AttachPoint.LeftCenter => new Vector2(0, 0.5f),
                AttachPoint.LeftBottom => new Vector2(0, 0),
                AttachPoint.CenterTop => new Vector2(0.5f, 1),
                AttachPoint.CenterCenter => new Vector2(0.5f, 0.5f),
                AttachPoint.CenterBottom => new Vector2(0.5f, 0),
                AttachPoint.RightTop => new Vector2(1, 1),
                AttachPoint.RightCenter => new Vector2(1, 0.5f),
                AttachPoint.RightBottom => new Vector2(1, 0),
                _ => new Vector2(0.5f, 0.5f)
            };
        }

        protected static Vector2 GetAnchorForTargetPoint(RectTransform target, RectTransform parentRect, Vector2 targetNormalizedPoint) {
            Vector3 worldPoint = target.TransformPoint(new Vector3(
                Mathf.Lerp(target.rect.xMin, target.rect.xMax, targetNormalizedPoint.x),
                Mathf.Lerp(target.rect.yMin, target.rect.yMax, targetNormalizedPoint.y),
                0
            ));
            Vector2 localPoint = parentRect.InverseTransformPoint(worldPoint);

            return new Vector2(
                Mathf.InverseLerp(parentRect.rect.xMin, parentRect.rect.xMax, localPoint.x),
                Mathf.InverseLerp(parentRect.rect.yMin, parentRect.rect.yMax, localPoint.y)
            );
        }

        /// <summary>
        /// Keeps a floating item synchronized when its attachment target changes without
        /// changing this item's own RectTransform (for example, a sibling target moving).
        /// The comparison is deliberately limited to floating items, which already have
        /// an Update call for editor transform tracking.
        /// </summary>
        private void RefreshFloatingLayoutForTargetChanges() {
            if(!m_isFloating || m_attachTo == FloatingAttachTo.None || _applyingFloatingLayout)
                return;

            RectTransform target = ResolveFloatingTarget();
            RectTransform parentRect = _rect ? _rect.parent as RectTransform : null;
            if(!target || !parentRect)
                return;

            if(!_hasFloatingTargetState
                || target != _lastFloatingTarget
                || target.rect != _lastFloatingTargetRect
                || target.localToWorldMatrix != _lastFloatingTargetLocalToWorld
                || parentRect.rect != _lastFloatingParentRect
                || parentRect.worldToLocalMatrix != _lastFloatingParentWorldToLocal) {
                ApplyFloatingLayout();
            }
        }

        private void CacheFloatingTargetState(RectTransform target, RectTransform parentRect) {
            _lastFloatingTarget = target;
            _lastFloatingTargetRect = target.rect;
            _lastFloatingTargetLocalToWorld = target.localToWorldMatrix;
            _lastFloatingParentRect = parentRect.rect;
            _lastFloatingParentWorldToLocal = parentRect.worldToLocalMatrix;
            _hasFloatingTargetState = true;
        }

        private Vector2 GetTargetSizeInParentSpace(RectTransform target, RectTransform parentRect) {
            target.GetWorldCorners(_floatingTargetCorners);

            Vector3 min = parentRect.InverseTransformPoint(_floatingTargetCorners[0]);
            Vector3 max = min;
            for(int i = 1; i < _floatingTargetCorners.Length; i++) {
                Vector3 point = parentRect.InverseTransformPoint(_floatingTargetCorners[i]);
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }

            Vector3 size = max - min;
            return new Vector2(size.x, size.y);
        }

        internal void WarnCircularSizing(RectTransform.Axis axis) {
            LayoutAxis circular = LayoutUtil.FindCircularSizingAxes(
                this,
                m_sizing,
                m_isFloating
            );
            LayoutAxis expected = axis == RectTransform.Axis.Horizontal ? LayoutAxis.Horizontal : LayoutAxis.Vertical;
            bool circularAxis = (circular & expected) != 0;
            if(axis == RectTransform.Axis.Horizontal) {
                if(!circularAxis || _warnedCircularX)
                    return;
                _warnedCircularX = true;
            }
            else {
                if(!circularAxis || _warnedCircularY)
                    return;
                _warnedCircularY = true;
            }

            string axisLabel = axis == RectTransform.Axis.Horizontal ? "horizontal" : "vertical";
            Debug.LogWarning($"[uLayout] Circular {axisLabel} sizing detected for \"{name}\". Using the current safe size for this pass.", this);
        }

        protected void CacheParentReferences() {
            EnsureReferences();
            _parentRect = transform.parent ? transform.parent.GetComponent<RectTransform>() : null;
            _parent = null;

            if(!transform.parent)
                return;

            foreach(MonoBehaviour behaviour in transform.parent.GetComponents<MonoBehaviour>()) {
                if(behaviour is IuLayoutContainer container) {
                    _parent = container;
                    break;
                }
            }
        }

        protected void NotifyParentLayoutChanged(bool refreshCache = false) {
            if(HasParentContainer) {
                if(refreshCache) {
                    _parent.RefreshChildCache();
                }
                else {
                    _parent.SetDirty();
                }
            }
        }

        protected void HandleStandaloneSizing() {
            EnsureReferences();
            if(HasParentContainer || !_parentRect)
                return;

            if(!CanWriteRectTransformNow())
                return;

            if(m_sizing.x == SizingMode.Grow || m_sizing.x == SizingMode.Percent) {
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ResolveAxisSize(RectTransform.Axis.Horizontal, _parentRect.rect.size.x));
            }
            if(m_sizing.y == SizingMode.Grow || m_sizing.y == SizingMode.Percent) {
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ResolveAxisSize(RectTransform.Axis.Vertical, _parentRect.rect.size.y));
            }
        }

        protected void ApplyFloatingExpand(bool widthWasResolved, bool heightWasResolved) {
            Vector2 currentSize = _rect.rect.size;
            Vector2 baseSize = currentSize;

            if(_hasAppliedFloatingExpand && Approximately(currentSize, _lastExpandedFloatingSize)) {
                if(!widthWasResolved) {
                    baseSize.x = _lastFloatingBaseSize.x;
                }
                if(!heightWasResolved) {
                    baseSize.y = _lastFloatingBaseSize.y;
                }
            }

            Vector2 expand = new(Mathf.Max(0, m_floatingExpand.x), Mathf.Max(0, m_floatingExpand.y));
            Vector2 expandedSize = new(
                ClampSize(RectTransform.Axis.Horizontal, baseSize.x + expand.x * 2),
                ClampSize(RectTransform.Axis.Vertical, baseSize.y + expand.y * 2)
            );

            _lastFloatingBaseSize = baseSize;
            _lastExpandedFloatingSize = expandedSize;
            _hasAppliedFloatingExpand = !Approximately(baseSize, expandedSize);

            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, expandedSize.x);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, expandedSize.y);
        }

        private void RemoveFloatingExpand() {
            if(!_hasAppliedFloatingExpand || !_rect)
                return;

            if(Approximately(_rect.rect.size, _lastExpandedFloatingSize)) {
                _applyingFloatingLayout = true;
                try {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _lastFloatingBaseSize.x);
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _lastFloatingBaseSize.y);
                }
                finally {
                    _applyingFloatingLayout = false;
                }
            }

            _hasAppliedFloatingExpand = false;
            _lastFloatingBaseSize = Vector2.zero;
            _lastExpandedFloatingSize = Vector2.zero;
        }

        private static bool Approximately(Vector2 a, Vector2 b) {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }

        protected void SanitizeConfiguration(bool logWarnings) {
            if(logWarnings) {
                GetSanitizedPercentX();
                GetSanitizedPercentY();
            }
            else {
                m_percentage = new Vector2(Mathf.Clamp01(m_percentage.x), Mathf.Clamp01(m_percentage.y));
            }

            m_minSize = new Vector2(Mathf.Max(0, m_minSize.x), Mathf.Max(0, m_minSize.y));

            float maxX = float.IsPositiveInfinity(m_maxSize.x) ? float.PositiveInfinity : Mathf.Max(0, m_maxSize.x);
            float maxY = float.IsPositiveInfinity(m_maxSize.y) ? float.PositiveInfinity : Mathf.Max(0, m_maxSize.y);
            m_maxSize = new Vector2(maxX, maxY);

            if(m_maxSize.x < m_minSize.x) {
                if(logWarnings && !_warnedConstraintX) {
                    Debug.LogWarning($"[uLayout] Max width on \"{name}\" was lower than min width. Promoting max width to {m_minSize.x}.", this);
                    _warnedConstraintX = true;
                }
                m_maxSize = m_maxSize.SetX(m_minSize.x);
            }
            else {
                _warnedConstraintX = false;
            }
            if(m_maxSize.y < m_minSize.y) {
                if(logWarnings && !_warnedConstraintY) {
                    Debug.LogWarning($"[uLayout] Max height on \"{name}\" was lower than min height. Promoting max height to {m_minSize.y}.", this);
                    _warnedConstraintY = true;
                }
                m_maxSize = m_maxSize.SetY(m_minSize.y);
            }
            else {
                _warnedConstraintY = false;
            }
        }

        protected float GetSanitizedPercentX() {
            return GetSanitizedPercentAxis(m_percentage.x, RectTransform.Axis.Horizontal);
        }

        protected float GetSanitizedPercentY() {
            return GetSanitizedPercentAxis(m_percentage.y, RectTransform.Axis.Vertical);
        }

        private float GetSanitizedPercentAxis(float value, RectTransform.Axis axis) {
            float clamped = Mathf.Clamp01(value);

            if(!Mathf.Approximately(value, clamped)) {
                bool warned = axis == RectTransform.Axis.Horizontal ? _warnedPercentX : _warnedPercentY;
                if(!warned) {
                    string axisLabel = axis == RectTransform.Axis.Horizontal ? "x" : "y";
                    Debug.LogWarning($"[uLayout] Percent sizing on axis {axisLabel} for \"{name}\" must be between 0 and 1. Clamping {value} to {clamped}.", this);
                    if(axis == RectTransform.Axis.Horizontal) {
                        _warnedPercentX = true;
                    }
                    else {
                        _warnedPercentY = true;
                    }
                }
            }
            else {
                if(axis == RectTransform.Axis.Horizontal) {
                    _warnedPercentX = false;
                }
                else {
                    _warnedPercentY = false;
                }
            }

            if(axis == RectTransform.Axis.Horizontal) {
                m_percentage = m_percentage.SetX(clamped);
            }
            else {
                m_percentage = m_percentage.SetY(clamped);
            }

            return clamped;
        }

        private void WarnMissingFloatingTarget() {
            if(_warnedMissingFloatingTarget)
                return;

            string targetLabel = m_attachTo switch {
                FloatingAttachTo.Root => "root canvas",
                FloatingAttachTo.Parent => "parent RectTransform",
                FloatingAttachTo.RectTransform => "assigned RectTransform",
                _ => "floating target"
            };

            Debug.LogWarning($"[uLayout] Could not resolve {targetLabel} for floating item \"{name}\".", this);
            _warnedMissingFloatingTarget = true;
        }

        protected bool CanWriteRectTransformNow() {
            #if UNITY_EDITOR
            if(_deferRectTransformWrites || s_globalValidationLock) {
                return false;
            }
            #endif

            return true;
        }

        #if UNITY_EDITOR
        protected void QueueEditorRefresh() {
            if(Application.isPlaying)
                return;

            EditorApplication.delayCall += DeferredEditorRefresh;
        }

        private void DeferredEditorRefresh() {
            if(!this)
                return;

            EnsureReferences();
            if(!_rect)
                return;

            _deferRectTransformWrites = false;
            SetDirty();

            if(HasParentContainer) {
                LayoutRebuilder.MarkLayoutForRebuild(_parent.Rect);
            }
            else if(_rect) {
                LayoutRebuilder.MarkLayoutForRebuild(_rect);
            }
        }
        #endif

        protected void EnsureReferences() {
            if(!_rect) {
                _rect = GetComponent<RectTransform>();
            }

            if(_tracker.Equals(default(DrivenRectTransformTracker))) {
                _tracker = new DrivenRectTransformTracker();
            }
        }
    }
}

