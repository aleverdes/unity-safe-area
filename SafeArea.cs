using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace AffenCode
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [FormerlySerializedAs("_rectTransform")]
        public RectTransform RectTransform;
        public CanvasScaler CanvasScaler;
        public SafeAreaPostProcess[] PostProcesses;

        private int _prevScreenWidth;
        private int _prevScreenHeight;
        
        private void Reset()
        {
            RectTransform = GetComponent<RectTransform>();
            CanvasScaler = GetComponentInParent<CanvasScaler>();
            PostProcesses = GetComponents<SafeAreaPostProcess>();
        }

        private void Start()
        {
            Process();
        }

        private void Update()
        {
            if (_prevScreenHeight == Screen.height && _prevScreenWidth == Screen.width)
            {
                return;
            }
            
            Process();
        }

        private void Process()
        {
#if UNITY_WEBGL
            return;
#endif
            
            if (!CanvasScaler)
            {
                CanvasScaler = GetComponentInParent<CanvasScaler>();
            }

            if (!CanvasScaler)
            {
                Debug.LogError("SafeArea required CanvasScaler", this);
                return;
            }
            
            if (CanvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                var screen = new Vector2(Screen.width, Screen.height);
                var referenceResolution = CanvasScaler.referenceResolution;
                var referenceResolutionAspect = Vector2.one;
                referenceResolutionAspect.x = referenceResolution.x / screen.x;
                referenceResolutionAspect.y = referenceResolution.y / screen.y;
                var aspectRatio = Mathf.Lerp(referenceResolutionAspect.x, referenceResolutionAspect.y, CanvasScaler.matchWidthOrHeight);

                RectTransform.anchorMin = Vector2.zero;
                RectTransform.anchorMax = Vector2.zero;
                RectTransform.pivot = Vector2.zero;
                RectTransform.anchoredPosition = aspectRatio * new Vector2(Screen.safeArea.x, Screen.safeArea.y);
                RectTransform.sizeDelta = aspectRatio * new Vector2(Screen.safeArea.width, Screen.safeArea.height);
            }
            else if (CanvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand)
            {
                var canvas = GetComponentInParent<Canvas>();
                var canvasScale = canvas.transform.localScale;
                
                RectTransform.anchorMin = Vector2.zero;
                RectTransform.anchorMax = Vector2.zero;
                RectTransform.pivot = Vector2.zero;
                RectTransform.anchoredPosition = new Vector2(Screen.safeArea.x / canvasScale.x, Screen.safeArea.y / canvasScale.y);
                RectTransform.sizeDelta = new Vector2(Screen.safeArea.width / canvasScale.x, Screen.safeArea.height / canvasScale.y);
            }

            foreach (var postProcess in PostProcesses)
            {
                postProcess.PostProcess(this);
            }

            _prevScreenWidth = Screen.width;
            _prevScreenHeight = Screen.height;
        }
    }
}
