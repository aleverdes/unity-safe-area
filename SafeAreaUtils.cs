using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AffenCode
{
    /// <summary>
    /// Utility methods for working with SafeArea components
    /// </summary>
    public static class SafeAreaUtils
    {
        /// <summary>
        /// Get all SafeArea components in the scene
        /// </summary>
        public static SafeArea[] FindAllSafeAreas()
        {
            return Object.FindObjectsOfType<SafeArea>();
        }

        /// <summary>
        /// Get all SafeArea components under a specific transform
        /// </summary>
        public static SafeArea[] FindSafeAreasInChildren(Transform parent)
        {
            return parent.GetComponentsInChildren<SafeArea>(true);
        }

        /// <summary>
        /// Force refresh all SafeArea components in the scene
        /// </summary>
        public static void RefreshAllSafeAreas()
        {
            foreach (var safeArea in FindAllSafeAreas())
            {
                safeArea.ForceRefresh();
            }
        }

        /// <summary>
        /// Set the same strategy for multiple SafeArea components
        /// </summary>
        public static void SetStrategyForAll(SafeArea.PositioningStrategy strategy, params SafeArea[] safeAreas)
        {
            foreach (var safeArea in safeAreas)
            {
                if (safeArea)
                {
                    safeArea.Strategy = strategy;
                    safeArea.UpdateSafeArea();
                }
            }
        }

        /// <summary>
        /// Enable/disable animations for multiple SafeArea components
        /// </summary>
        public static void SetAnimationForAll(bool enabled, params SafeArea[] safeAreas)
        {
            foreach (var safeArea in safeAreas)
            {
                if (safeArea)
                {
                    safeArea.AnimateChanges = enabled;
                }
            }
        }

        /// <summary>
        /// Create a SafeArea component with common settings for UI panels
        /// </summary>
        public static SafeArea CreateSafeAreaPanel(GameObject target, SafeArea.PositioningStrategy strategy = SafeArea.PositioningStrategy.Stretch)
        {
            var safeArea = target.AddComponent<SafeArea>();
            safeArea.Strategy = strategy;
            safeArea.Padding = 10f;

            // Add a background for visibility
            var image = target.GetComponent<Image>();
            if (!image)
            {
                image = target.AddComponent<Image>();
                image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }

            return safeArea;
        }

        /// <summary>
        /// Get device-specific safe area information
        /// </summary>
        public static DeviceSafeAreaInfo GetDeviceInfo()
        {
            return new DeviceSafeAreaInfo
            {
                ScreenSize = new Vector2(Screen.width, Screen.height),
                SafeArea = Screen.safeArea,
                HasNotch = Screen.safeArea != new Rect(0, 0, Screen.width, Screen.height),
                AspectRatio = (float)Screen.width / Screen.height,
                IsLandscape = Screen.width > Screen.height
            };
        }

        /// <summary>
        /// Check if the current device has a notch or similar safe area
        /// </summary>
        public static bool HasNotch()
        {
#if UNITY_IOS
            // iOS devices with notches
            return Screen.safeArea != new Rect(0, 0, Screen.width, Screen.height);
#elif UNITY_ANDROID
            // Android devices with notches/punch holes
            return Screen.safeArea != new Rect(0, 0, Screen.width, Screen.height);
#else
            return false;
#endif
        }

        /// <summary>
        /// Get recommended safe area strategy for current device
        /// </summary>
        public static SafeArea.PositioningStrategy GetRecommendedStrategy()
        {
            if (HasNotch())
            {
                return SafeArea.PositioningStrategy.IndividualSides;
            }
            else
            {
                return SafeArea.PositioningStrategy.Stretch;
            }
        }

        /// <summary>
        /// Setup safe area for common UI layouts
        /// </summary>
        public static class LayoutPresets
        {
            /// <summary>
            /// Setup for full-screen background that respects safe area
            /// </summary>
            public static SafeArea SetupBackgroundPanel(GameObject panel)
            {
                var safeArea = CreateSafeAreaPanel(panel, SafeArea.PositioningStrategy.Stretch);
                var image = panel.GetComponent<Image>();
                if (image)
                {
                    image.color = Color.white;
                }
                return safeArea;
            }

            /// <summary>
            /// Setup for game UI that should fit within safe area
            /// </summary>
            public static SafeArea SetupGameUIPanel(GameObject panel)
            {
                var safeArea = CreateSafeAreaPanel(panel, SafeArea.PositioningStrategy.Fit);

                // Add aspect ratio constraints
                var aspectRatioPP = panel.AddComponent<AspectRatioSafeAreaPostProcess>();
                aspectRatioPP.MaxAspectRatio = 2.1f;
                aspectRatioPP.MinAspectRatio = 0.5f;
                aspectRatioPP.Mode = AspectRatioSafeAreaPostProcess.AspectRatioMode.ScaleToFit;

                return safeArea;
            }

            /// <summary>
            /// Setup for HUD elements that need custom positioning
            /// </summary>
            public static SafeArea SetupHUDPanel(GameObject panel)
            {
                var safeArea = CreateSafeAreaPanel(panel, SafeArea.PositioningStrategy.IndividualSides);

                // Configure sides - typically keep top safe for notches
                safeArea.TopSide = new SafeArea.SafeAreaSide(true, 20f);
                safeArea.BottomSide = new SafeArea.SafeAreaSide(false, 0f); // Ignore bottom
                safeArea.LeftSide = new SafeArea.SafeAreaSide(false, 0f);
                safeArea.RightSide = new SafeArea.SafeAreaSide(false, 0f);

                return safeArea;
            }

            /// <summary>
            /// Setup for dialog/modal windows
            /// </summary>
            public static SafeArea SetupDialogPanel(GameObject panel)
            {
                var safeArea = CreateSafeAreaPanel(panel, SafeArea.PositioningStrategy.CustomMargins);

                // Add padding from all edges
                safeArea.LeftMargin = 20f;
                safeArea.RightMargin = 20f;
                safeArea.TopMargin = 40f;
                safeArea.BottomMargin = 20f;

                // Add padding post-processor for additional spacing
                var padding = panel.AddComponent<PaddingSafeAreaPostProcess>();
                padding.SetUniformPadding(30f);

                return safeArea;
            }
        }
    }

    /// <summary>
    /// Information about device safe area
    /// </summary>
    public struct DeviceSafeAreaInfo
    {
        public Vector2 ScreenSize;
        public Rect SafeArea;
        public bool HasNotch;
        public float AspectRatio;
        public bool IsLandscape;

        public Vector4 Margins => new Vector4(
            SafeArea.x,
            SafeArea.y,
            ScreenSize.x - SafeArea.xMax,
            ScreenSize.y - SafeArea.yMax
        );

        public override string ToString()
        {
            return $"Device Info: {ScreenSize.x}×{ScreenSize.y}, Safe Area: {SafeArea}, Has Notch: {HasNotch}";
        }
    }
}
