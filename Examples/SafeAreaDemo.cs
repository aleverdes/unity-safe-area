using UnityEngine;
using UnityEngine.UI;
using AffenCode;

namespace AffenCode.Examples
{
    /// <summary>
    /// Demonstration script showing various SafeArea features and use cases
    /// </summary>
    public class SafeAreaDemo : MonoBehaviour
    {
        [Header("UI References")]
        public Text infoText;
        public Button strategyButton;
        public Button animationToggle;
        public Slider animationSpeedSlider;

        [Header("Demo Objects")]
        public GameObject[] demoPanels;

        private SafeArea safeArea;
        private int currentStrategyIndex = 0;
        private readonly SafeArea.PositioningStrategy[] strategies = {
            SafeArea.PositioningStrategy.Stretch,
            SafeArea.PositioningStrategy.Fit,
            SafeArea.PositioningStrategy.CustomMargins,
            SafeArea.PositioningStrategy.IndividualSides
        };

        private void Start()
        {
            safeArea = GetComponent<SafeArea>();

            // Setup UI
            if (strategyButton)
            {
                strategyButton.onClick.AddListener(CycleStrategy);
                UpdateStrategyButtonText();
            }

            if (animationToggle)
            {
                animationToggle.onClick.AddListener(ToggleAnimation);
                UpdateAnimationButtonText();
            }

            if (animationSpeedSlider)
            {
                animationSpeedSlider.value = safeArea.AnimationDuration;
                animationSpeedSlider.onValueChanged.AddListener(OnAnimationSpeedChanged);
            }

            UpdateInfoText();
        }

        private void Update()
        {
            // Update info display
            if (infoText && safeArea.HasSafeAreaChanged)
            {
                UpdateInfoText();
            }
        }

        private void CycleStrategy()
        {
            currentStrategyIndex = (currentStrategyIndex + 1) % strategies.Length;
            safeArea.Strategy = strategies[currentStrategyIndex];
            safeArea.UpdateSafeArea();

            UpdateStrategyButtonText();
            UpdateInfoText();
        }

        private void ToggleAnimation()
        {
            safeArea.AnimateChanges = !safeArea.AnimateChanges;
            UpdateAnimationButtonText();
        }

        private void OnAnimationSpeedChanged(float value)
        {
            safeArea.AnimationDuration = value;
        }

        private void UpdateStrategyButtonText()
        {
            if (strategyButton && strategyButton.GetComponentInChildren<Text>())
            {
                strategyButton.GetComponentInChildren<Text>().text = $"Strategy: {safeArea.Strategy}";
            }
        }

        private void UpdateAnimationButtonText()
        {
            if (animationToggle && animationToggle.GetComponentInChildren<Text>())
            {
                animationToggle.GetComponentInChildren<Text>().text =
                    safeArea.AnimateChanges ? "Animation: ON" : "Animation: OFF";
            }
        }

        private void UpdateInfoText()
        {
            if (!infoText) return;

            var margins = safeArea.GetSafeAreaMargins();
            infoText.text = $"Safe Area Demo\n\n" +
                           $"Strategy: {safeArea.Strategy}\n" +
                           $"Safe Area: {safeArea.CanvasSafeArea.width:F0}×{safeArea.CanvasSafeArea.height:F0}\n" +
                           $"Position: ({safeArea.CanvasSafeArea.x:F0}, {safeArea.CanvasSafeArea.y:F0})\n" +
                           $"Margins: L:{margins.x:F0} T:{margins.y:F0} R:{margins.z:F0} B:{margins.w:F0}\n" +
                           $"Animation: {(safeArea.AnimateChanges ? "ON" : "OFF")}\n" +
                           $"Canvas Scale: {GetCanvasScale():F2}";
        }

        private float GetCanvasScale()
        {
            var canvas = GetComponentInParent<Canvas>();
            return canvas ? canvas.transform.localScale.x : 1f;
        }

        /// <summary>
        /// Programmatic setup example
        /// </summary>
        public static void SetupDemoPanel(GameObject panel)
        {
            var safeArea = panel.AddComponent<SafeArea>();

            // Configure for stretch mode
            safeArea.Strategy = SafeArea.PositioningStrategy.Stretch;
            safeArea.Padding = 10f;

            // Add post-processors
            var padding = panel.AddComponent<PaddingSafeAreaPostProcess>();
            padding.SetUniformPadding(20f);

            var aspectRatio = panel.AddComponent<AspectRatioSafeAreaPostProcess>();
            aspectRatio.MaxAspectRatio = 2.0f;
            aspectRatio.Mode = AspectRatioSafeAreaPostProcess.AspectRatioMode.ScaleToFit;

            safeArea.UpdateSafeArea();
        }
    }
}
