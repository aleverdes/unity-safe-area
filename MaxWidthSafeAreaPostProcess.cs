using UnityEngine;
namespace AffenCode
{
    /// <summary>
    /// Post-processor that limits the maximum width relative to height
    /// Useful for maintaining consistent proportions and preventing overly wide layouts
    /// </summary>
    public class MaxWidthSafeAreaPostProcess : SafeAreaPostProcess
    {
        [Tooltip("Maximum width as a multiple of height (e.g., 2.0 = width can be at most 2x height)")]
        public float MaxWidthRatio = 2.0f;

        [Tooltip("How to handle width exceeding the maximum: Scale or Center")]
        public WidthLimitMode Mode = WidthLimitMode.Center;

        [Tooltip("Minimum allowed width (as ratio of height)")]
        public float MinWidthRatio = 0.5f;

        public enum WidthLimitMode
        {
            /// <summary>
            /// Center the content horizontally when limiting width
            /// </summary>
            Center,

            /// <summary>
            /// Scale down the entire content to fit
            /// </summary>
            Scale,

            /// <summary>
            /// Crop from the left side
            /// </summary>
            CropLeft,

            /// <summary>
            /// Crop from the right side
            /// </summary>
            CropRight
        }

        public override void PostProcess(SafeArea safeArea)
        {
            var position = safeArea.RectTransform.anchoredPosition;
            var size = safeArea.RectTransform.sizeDelta;

            float currentRatio = size.x / size.y;
            float targetRatio = Mathf.Clamp(currentRatio, MinWidthRatio, MaxWidthRatio);

            if (Mathf.Approximately(currentRatio, targetRatio))
                return;

            float targetWidth = targetRatio * size.y;
            float widthDelta = size.x - targetWidth;

            switch (Mode)
            {
                case WidthLimitMode.Center:
                    position.x += widthDelta * 0.5f;
                    break;

                case WidthLimitMode.Scale:
                    // Scale both width and height to maintain aspect ratio
                    float scale = targetWidth / size.x;
                    size.y *= scale;
                    position.y += (size.y - safeArea.RectTransform.sizeDelta.y) * 0.5f;
                    break;

                case WidthLimitMode.CropLeft:
                    position.x += widthDelta;
                    break;

                case WidthLimitMode.CropRight:
                    // No position change needed, just reduce width
                    break;
            }

            size.x = targetWidth;

            safeArea.RectTransform.anchoredPosition = position;
            safeArea.RectTransform.sizeDelta = size;
        }
    }
}
