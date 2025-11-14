using UnityEngine;
namespace AffenCode
{
    /// <summary>
    /// Post-processor that enforces a maximum aspect ratio on the safe area
    /// Useful for maintaining consistent proportions across different devices
    /// </summary>
    public class AspectRatioSafeAreaPostProcess : SafeAreaPostProcess
    {
        [Tooltip("Maximum allowed aspect ratio (width/height). Values above this will be clamped.")]
        public float MaxAspectRatio = 2.1f;

        [Tooltip("Minimum allowed aspect ratio (width/height). Values below this will be clamped.")]
        public float MinAspectRatio = 0.5f;

        [Tooltip("How to handle aspect ratio clamping: Scale to fit or add letterboxing")]
        public AspectRatioMode Mode = AspectRatioMode.ScaleToFit;

        public enum AspectRatioMode
        {
            /// <summary>
            /// Scale the content to fit within the aspect ratio constraints
            /// </summary>
            ScaleToFit,

            /// <summary>
            /// Add black bars (letterboxing/pillarboxing) to maintain aspect ratio
            /// </summary>
            Letterbox,

            /// <summary>
            /// Crop the content to fit the aspect ratio
            /// </summary>
            Crop
        }

        public override void PostProcess(SafeArea safeArea)
        {
            var position = safeArea.RectTransform.anchoredPosition;
            var size = safeArea.RectTransform.sizeDelta;

            float currentAspectRatio = size.x / size.y;

            // Clamp aspect ratio
            float targetAspectRatio = Mathf.Clamp(currentAspectRatio, MinAspectRatio, MaxAspectRatio);

            if (Mathf.Approximately(currentAspectRatio, targetAspectRatio))
                return;

            switch (Mode)
            {
                case AspectRatioMode.ScaleToFit:
                    ApplyScaleToFit(ref position, ref size, currentAspectRatio, targetAspectRatio);
                    break;

                case AspectRatioMode.Letterbox:
                    ApplyLetterbox(ref position, ref size, currentAspectRatio, targetAspectRatio);
                    break;

                case AspectRatioMode.Crop:
                    ApplyCrop(ref position, ref size, currentAspectRatio, targetAspectRatio);
                    break;
            }

            safeArea.RectTransform.anchoredPosition = position;
            safeArea.RectTransform.sizeDelta = size;
        }

        private void ApplyScaleToFit(ref Vector3 position, ref Vector2 size, float currentRatio, float targetRatio)
        {
            if (currentRatio > targetRatio)
            {
                // Too wide, scale down width
                float newWidth = size.y * targetRatio;
                float widthDiff = size.x - newWidth;
                position.x += widthDiff * 0.5f;
                size.x = newWidth;
            }
            else
            {
                // Too tall, scale down height
                float newHeight = size.x / targetRatio;
                float heightDiff = size.y - newHeight;
                position.y += heightDiff * 0.5f;
                size.y = newHeight;
            }
        }

        private void ApplyLetterbox(ref Vector3 position, ref Vector2 size, float currentRatio, float targetRatio)
        {
            // Letterboxing maintains the original size but adjusts position
            // This is typically handled by the positioning strategy, so we just ensure the aspect ratio
            if (currentRatio > targetRatio)
            {
                // Add vertical letterboxing by adjusting height
                float targetHeight = size.x / targetRatio;
                float heightDiff = size.y - targetHeight;
                position.y += heightDiff * 0.5f;
                size.y = targetHeight;
            }
            else
            {
                // Add horizontal letterboxing by adjusting width
                float targetWidth = size.y * targetRatio;
                float widthDiff = size.x - targetWidth;
                position.x += widthDiff * 0.5f;
                size.x = targetWidth;
            }
        }

        private void ApplyCrop(ref Vector3 position, ref Vector2 size, float currentRatio, float targetRatio)
        {
            // Cropping maintains position but may extend beyond safe area
            if (currentRatio > targetRatio)
            {
                // Crop horizontally
                float targetWidth = size.y * targetRatio;
                float widthDiff = size.x - targetWidth;
                position.x += widthDiff * 0.5f;
                size.x = targetWidth;
            }
            else
            {
                // Crop vertically
                float targetHeight = size.x / targetRatio;
                float heightDiff = size.y - targetHeight;
                position.y += heightDiff * 0.5f;
                size.y = targetHeight;
            }
        }
    }
}
