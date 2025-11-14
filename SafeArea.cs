using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AffenCode
{
    /// <summary>
    /// Advanced Safe Area component for Unity UGUI that automatically adjusts RectTransform
    /// to respect device safe areas (notches, rounded corners, etc.) with flexible positioning strategies.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class SafeArea : UIBehaviour
    {
        /// <summary>
        /// Safe area positioning strategy
        /// </summary>
        public enum PositioningStrategy
        {
            /// <summary>
            /// Stretch to fill entire safe area
            /// </summary>
            Stretch,

            /// <summary>
            /// Fit within safe area maintaining aspect ratio
            /// </summary>
            Fit,

            /// <summary>
            /// Custom margins from screen edges
            /// </summary>
            CustomMargins,

            /// <summary>
            /// Control each side independently
            /// </summary>
            IndividualSides
        }

        /// <summary>
        /// Safe area side configuration
        /// </summary>
        [Serializable]
        public struct SafeAreaSide
        {
            public bool enabled;
            public float margin;

            public SafeAreaSide(bool enabled = true, float margin = 0f)
            {
                this.enabled = enabled;
                this.margin = margin;
            }
        }

        [Header("Core Settings")]
        [Tooltip("RectTransform to adjust (automatically set to this component)")]
        public RectTransform RectTransform;

        [Tooltip("CanvasScaler reference (automatically detected if not set)")]
        public CanvasScaler CanvasScaler;

        [Header("Positioning Strategy")]
        [Tooltip("How to position the safe area")]
        public PositioningStrategy Strategy = PositioningStrategy.Stretch;

        [Tooltip("Additional padding around the safe area")]
        public float Padding = 0f;

        [Header("Custom Margins (when Strategy is CustomMargins)")]
        [Tooltip("Left margin from screen edge")]
        public float LeftMargin = 0f;

        [Tooltip("Right margin from screen edge")]
        public float RightMargin = 0f;

        [Tooltip("Top margin from screen edge")]
        public float TopMargin = 0f;

        [Tooltip("Bottom margin from screen edge")]
        public float BottomMargin = 0f;

        [Header("Individual Sides (when Strategy is IndividualSides)")]
        [Tooltip("Left side configuration")]
        public SafeAreaSide LeftSide = new SafeAreaSide(true, 0f);

        [Tooltip("Right side configuration")]
        public SafeAreaSide RightSide = new SafeAreaSide(true, 0f);

        [Tooltip("Top side configuration")]
        public SafeAreaSide TopSide = new SafeAreaSide(true, 0f);

        [Tooltip("Bottom side configuration")]
        public SafeAreaSide BottomSide = new SafeAreaSide(true, 0f);

        [Header("Animation")]
        [Tooltip("Enable smooth transitions when safe area changes")]
        public bool AnimateChanges = false;

        [Tooltip("Animation duration in seconds")]
        public float AnimationDuration = 0.3f;

        [Tooltip("Animation easing curve")]
        public AnimationCurve AnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Post Processing")]
        [Tooltip("Post-process components to apply after positioning")]
        public SafeAreaPostProcess[] PostProcesses;

        // Cached values
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private Canvas _canvas;
        private Vector3 _targetPosition;
        private Vector2 _targetSize;
        private float _animationTime;
        private bool _isAnimating;
        private Vector3 _startPosition;
        private Vector2 _startSize;

        // Editor preview
        [NonSerialized] public bool ShowPreviewInEditor = true;
        [NonSerialized] public Color PreviewColor = new Color(0f, 1f, 0f, 0.2f);

        /// <summary>
        /// Current safe area in canvas coordinates
        /// </summary>
        public Rect CanvasSafeArea { get; private set; }

        /// <summary>
        /// Whether the safe area has changed since last update
        /// </summary>
        public bool HasSafeAreaChanged { get; private set; }

        protected override void Reset()
        {
            base.Reset();
            InitializeComponents();
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateSafeArea();
        }

        protected override void Start()
        {
            base.Start();
            UpdateSafeArea();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (isActiveAndEnabled)
            {
                UpdateSafeArea();
            }
        }

        private void Update()
        {
            // Check for screen size changes
            if (_lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
            {
                UpdateSafeArea();
                return;
            }

            // Check for safe area changes (more expensive, so check less frequently)
            var currentSafeArea = Screen.safeArea;
            if (!Mathf.Approximately(_lastSafeArea.x, currentSafeArea.x) ||
                !Mathf.Approximately(_lastSafeArea.y, currentSafeArea.y) ||
                !Mathf.Approximately(_lastSafeArea.width, currentSafeArea.width) ||
                !Mathf.Approximately(_lastSafeArea.height, currentSafeArea.height))
            {
                UpdateSafeArea();
                return;
            }

            // Handle animation
            if (_isAnimating)
            {
                _animationTime += Time.deltaTime;
                float t = Mathf.Clamp01(_animationTime / AnimationDuration);
                float easedT = AnimationCurve.Evaluate(t);

                RectTransform.anchoredPosition = Vector3.Lerp(_startPosition, _targetPosition, easedT);
                RectTransform.sizeDelta = Vector2.Lerp(_startSize, _targetSize, easedT);

                if (t >= 1f)
                {
                    _isAnimating = false;
                    RectTransform.anchoredPosition = _targetPosition;
                    RectTransform.sizeDelta = _targetSize;
                }
            }
        }

        private void InitializeComponents()
        {
            if (!RectTransform)
                RectTransform = GetComponent<RectTransform>();

            if (!CanvasScaler)
                CanvasScaler = GetComponentInParent<CanvasScaler>();

            if (!CanvasScaler)
            {
                Debug.LogWarning("SafeArea: No CanvasScaler found in parent hierarchy. " +
                               "Safe area positioning may not work correctly.", this);
            }

            _canvas = GetComponentInParent<Canvas>();
            PostProcesses = GetComponents<SafeAreaPostProcess>();
        }

        /// <summary>
        /// Force update the safe area positioning
        /// </summary>
        public void UpdateSafeArea()
        {
#if UNITY_WEBGL
            // WebGL doesn't have reliable safe area information
            return;
#endif

            if (!RectTransform || !CanvasScaler)
            {
                InitializeComponents();
                if (!RectTransform || !CanvasScaler)
                    return;
            }

            var screenSafeArea = Screen.safeArea;
            CanvasSafeArea = ConvertScreenRectToCanvas(screenSafeArea);

            HasSafeAreaChanged = _lastSafeArea != screenSafeArea;
            _lastSafeArea = screenSafeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            CalculateTargetTransform();
            ApplyPostProcessing();

            if (AnimateChanges && HasSafeAreaChanged && Application.isPlaying)
            {
                StartAnimation();
            }
            else
            {
                RectTransform.anchoredPosition = _targetPosition;
                RectTransform.sizeDelta = _targetSize;
            }
        }

        private Rect ConvertScreenRectToCanvas(Rect screenRect)
        {
            if (!CanvasScaler || !_canvas)
                return screenRect;

            float scaleFactor = GetCanvasScaleFactor();
            Vector2 canvasSize = _canvas.GetComponent<RectTransform>().rect.size;

            // Convert screen coordinates to canvas coordinates
            Vector2 canvasMin = new Vector2(
                (screenRect.xMin / Screen.width) * canvasSize.x,
                (screenRect.yMin / Screen.height) * canvasSize.y
            );

            Vector2 canvasMax = new Vector2(
                (screenRect.xMax / Screen.width) * canvasSize.x,
                (screenRect.yMax / Screen.height) * canvasSize.y
            );

            return new Rect(canvasMin.x, canvasMin.y, canvasMax.x - canvasMin.x, canvasMax.y - canvasMin.y);
        }

        private float GetCanvasScaleFactor()
        {
            if (!CanvasScaler || !_canvas)
                return 1f;

            switch (CanvasScaler.screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    {
                        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
                        Vector2 referenceResolution = CanvasScaler.referenceResolution;

                        float widthRatio = screenSize.x / referenceResolution.x;
                        float heightRatio = screenSize.y / referenceResolution.y;

                        return Mathf.Lerp(widthRatio, heightRatio, CanvasScaler.matchWidthOrHeight);
                    }

                case CanvasScaler.ScreenMatchMode.Expand:
                    return Mathf.Min(Screen.width / CanvasScaler.referenceResolution.x,
                                   Screen.height / CanvasScaler.referenceResolution.y);

                case CanvasScaler.ScreenMatchMode.Shrink:
                    return Mathf.Max(Screen.width / CanvasScaler.referenceResolution.x,
                                   Screen.height / CanvasScaler.referenceResolution.y);

                default:
                    return 1f;
            }
        }

        private void CalculateTargetTransform()
        {
            Rect safeArea = CanvasSafeArea;

            switch (Strategy)
            {
                case PositioningStrategy.Stretch:
                    ApplyStretchStrategy(safeArea);
                    break;

                case PositioningStrategy.Fit:
                    ApplyFitStrategy(safeArea);
                    break;

                case PositioningStrategy.CustomMargins:
                    ApplyCustomMarginsStrategy(safeArea);
                    break;

                case PositioningStrategy.IndividualSides:
                    ApplyIndividualSidesStrategy(safeArea);
                    break;
            }

            // Apply padding
            if (Padding > 0f)
            {
                _targetPosition += new Vector3(Padding, Padding, 0f);
                _targetSize -= new Vector2(Padding * 2f, Padding * 2f);
            }
        }

        private void ApplyStretchStrategy(Rect safeArea)
        {
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.pivot = Vector2.zero;

            _targetPosition = new Vector3(safeArea.x, safeArea.y, 0f);
            _targetSize = new Vector2(safeArea.width, safeArea.height);
        }

        private void ApplyFitStrategy(Rect safeArea)
        {
            RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            RectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Center within safe area and fit to smallest dimension
            float aspectRatio = safeArea.width / safeArea.height;
            Vector2 canvasSize = _canvas.GetComponent<RectTransform>().rect.size;

            if (aspectRatio > canvasSize.x / canvasSize.y)
            {
                // Fit to width
                _targetSize = new Vector2(safeArea.width, safeArea.width / aspectRatio);
            }
            else
            {
                // Fit to height
                _targetSize = new Vector2(safeArea.height * aspectRatio, safeArea.height);
            }

            _targetPosition = new Vector3(
                safeArea.center.x - _targetSize.x * 0.5f,
                safeArea.center.y - _targetSize.y * 0.5f,
                0f
            );
        }

        private void ApplyCustomMarginsStrategy(Rect safeArea)
        {
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.pivot = Vector2.zero;

            float scaleFactor = GetCanvasScaleFactor();
            Vector2 canvasSize = _canvas.GetComponent<RectTransform>().rect.size;

            float left = LeftMargin * scaleFactor;
            float right = RightMargin * scaleFactor;
            float top = TopMargin * scaleFactor;
            float bottom = BottomMargin * scaleFactor;

            _targetPosition = new Vector3(left, bottom, 0f);
            _targetSize = new Vector2(canvasSize.x - left - right, canvasSize.y - top - bottom);
        }

        private void ApplyIndividualSidesStrategy(Rect safeArea)
        {
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.pivot = Vector2.zero;

            float scaleFactor = GetCanvasScaleFactor();
            Vector2 canvasSize = _canvas.GetComponent<RectTransform>().rect.size;

            float left = LeftSide.enabled ? safeArea.x + LeftSide.margin * scaleFactor : 0f;
            float right = RightSide.enabled ? canvasSize.x - (safeArea.x + safeArea.width) + RightSide.margin * scaleFactor : 0f;
            float top = TopSide.enabled ? canvasSize.y - (safeArea.y + safeArea.height) + TopSide.margin * scaleFactor : 0f;
            float bottom = BottomSide.enabled ? safeArea.y + BottomSide.margin * scaleFactor : 0f;

            _targetPosition = new Vector3(left, bottom, 0f);
            _targetSize = new Vector2(canvasSize.x - left - right, canvasSize.y - top - bottom);
        }

        private void ApplyPostProcessing()
        {
            if (PostProcesses == null)
                return;

            foreach (var postProcess in PostProcesses)
            {
                if (postProcess && postProcess.enabled)
                {
                    postProcess.PostProcess(this);
                }
            }
        }

        private void StartAnimation()
        {
            _startPosition = RectTransform.anchoredPosition;
            _startSize = RectTransform.sizeDelta;
            _animationTime = 0f;
            _isAnimating = true;
        }

        /// <summary>
        /// Get the current safe area margins in pixels
        /// </summary>
        public Vector4 GetSafeAreaMargins()
        {
            return new Vector4(
                CanvasSafeArea.x,
                CanvasSafeArea.y,
                _canvas.GetComponent<RectTransform>().rect.width - CanvasSafeArea.xMax,
                _canvas.GetComponent<RectTransform>().rect.height - CanvasSafeArea.yMax
            );
        }

        /// <summary>
        /// Check if a point is within the safe area
        /// </summary>
        public bool IsPointInSafeArea(Vector2 point)
        {
            return CanvasSafeArea.Contains(point);
        }

        /// <summary>
        /// Force refresh of all safe area calculations
        /// </summary>
        public void ForceRefresh()
        {
            _lastSafeArea = Rect.zero;
            _lastScreenSize = Vector2Int.zero;
            UpdateSafeArea();
        }
    }
}
