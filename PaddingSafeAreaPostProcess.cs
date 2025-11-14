using UnityEngine;
namespace AffenCode
{
    /// <summary>
    /// Post-processor that adds configurable padding around the safe area
    /// Useful for creating consistent spacing or avoiding screen edges
    /// </summary>
    public class PaddingSafeAreaPostProcess : SafeAreaPostProcess
    {
        [Tooltip("Padding to add on all sides")]
        public float UniformPadding = 10f;

        [Tooltip("Left padding")]
        public float LeftPadding = 0f;

        [Tooltip("Right padding")]
        public float RightPadding = 0f;

        [Tooltip("Top padding")]
        public float TopPadding = 0f;

        [Tooltip("Bottom padding")]
        public float BottomPadding = 0f;

        [Tooltip("Whether padding values are in pixels or relative to safe area size (0-1)")]
        public PaddingMode Mode = PaddingMode.Pixels;

        [Tooltip("Minimum padding values (applied after scaling)")]
        public Vector4 MinPadding = Vector4.zero;

        [Tooltip("Maximum padding values (applied after scaling)")]
        public Vector4 MaxPadding = new Vector4(100f, 100f, 100f, 100f);

        public enum PaddingMode
        {
            /// <summary>
            /// Padding values are in absolute pixels
            /// </summary>
            Pixels,

            /// <summary>
            /// Padding values are relative to safe area size (0-1 range)
            /// </summary>
            Relative,

            /// <summary>
            /// Padding values are relative to screen size (0-1 range)
            /// </summary>
            ScreenRelative
        }

        public override void PostProcess(SafeArea safeArea)
        {
            var position = safeArea.RectTransform.anchoredPosition;
            var size = safeArea.RectTransform.sizeDelta;

            Vector4 padding = CalculatePadding(safeArea, size);

            // Apply minimum and maximum constraints
            padding = Vector4.Max(padding, MinPadding);
            padding = Vector4.Min(padding, MaxPadding);

            // Apply padding
            position.x += padding.x;
            position.y += padding.y;
            size.x -= (padding.x + padding.z); // left + right
            size.y -= (padding.y + padding.w); // top + bottom

            // Ensure we don't go negative
            size.x = Mathf.Max(0f, size.x);
            size.y = Mathf.Max(0f, size.y);

            safeArea.RectTransform.anchoredPosition = position;
            safeArea.RectTransform.sizeDelta = size;
        }

        private Vector4 CalculatePadding(SafeArea safeArea, Vector2 currentSize)
        {
            float left, right, top, bottom;

            switch (Mode)
            {
                case PaddingMode.Pixels:
                    left = UniformPadding + LeftPadding;
                    right = UniformPadding + RightPadding;
                    top = UniformPadding + TopPadding;
                    bottom = UniformPadding + BottomPadding;
                    break;

                case PaddingMode.Relative:
                    left = (UniformPadding + LeftPadding) * currentSize.x;
                    right = (UniformPadding + RightPadding) * currentSize.x;
                    top = (UniformPadding + TopPadding) * currentSize.y;
                    bottom = (UniformPadding + BottomPadding) * currentSize.y;
                    break;

                case PaddingMode.ScreenRelative:
                    var canvas = safeArea.GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        var canvasRect = canvas.GetComponent<RectTransform>().rect;
                        left = (UniformPadding + LeftPadding) * canvasRect.width;
                        right = (UniformPadding + RightPadding) * canvasRect.width;
                        top = (UniformPadding + TopPadding) * canvasRect.height;
                        bottom = (UniformPadding + BottomPadding) * canvasRect.height;
                    }
                    else
                    {
                        // Fallback to pixels if no canvas found
                        left = UniformPadding + LeftPadding;
                        right = UniformPadding + RightPadding;
                        top = UniformPadding + TopPadding;
                        bottom = UniformPadding + BottomPadding;
                    }
                    break;

                default:
                    left = right = top = bottom = UniformPadding;
                    break;
            }

            return new Vector4(left, top, right, bottom);
        }

        /// <summary>
        /// Set uniform padding on all sides
        /// </summary>
        public void SetUniformPadding(float padding)
        {
            UniformPadding = padding;
            LeftPadding = RightPadding = TopPadding = BottomPadding = 0f;
        }

        /// <summary>
        /// Set individual padding values
        /// </summary>
        public void SetPadding(float left, float top, float right, float bottom)
        {
            UniformPadding = 0f;
            LeftPadding = left;
            TopPadding = top;
            RightPadding = right;
            BottomPadding = bottom;
        }
    }
}
