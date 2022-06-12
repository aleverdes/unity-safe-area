using System;
using UnityEngine;
using UnityEngine.UI;
namespace AffenCode
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        private CanvasScaler _canvasScaler;
        
        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            Process();
        }

        private void Process()
        {
            var referenceResolutionAspect = Vector2.one;
            var aspectRatio = 1f;

            _canvasScaler = GetComponentInParent<CanvasScaler>();

            if (!_canvasScaler)
            {
                Debug.LogError("SafeArea required CanvasScaler");
                return;
            }
            
            if (_canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                var screen = new Vector2(Screen.width, Screen.height);
                var referenceResolution = _canvasScaler.referenceResolution;
                referenceResolutionAspect.x = referenceResolution.x / screen.x;
                referenceResolutionAspect.y = referenceResolution.y / screen.y;
                aspectRatio = Mathf.Lerp(referenceResolutionAspect.x, referenceResolutionAspect.y, _canvasScaler.matchWidthOrHeight);

                _rectTransform.anchorMin = Vector2.zero;
                _rectTransform.anchorMax = Vector2.zero;
                _rectTransform.pivot = Vector2.zero;
                _rectTransform.anchoredPosition = aspectRatio * new Vector2(Screen.safeArea.x, Screen.safeArea.y);
                _rectTransform.sizeDelta = aspectRatio * new Vector2(Screen.safeArea.width, Screen.safeArea.height);
            }
            else if (_canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand)
            {
                var canvas = GetComponentInParent<Canvas>();
                var canvasScale = canvas.transform.localScale;
                
                var screen = new Vector2(Screen.width, Screen.height);
                var referenceResolution = _canvasScaler.referenceResolution;
                var safeArea = Screen.safeArea;
                
                _rectTransform.anchorMin = Vector2.zero;
                _rectTransform.anchorMax = Vector2.zero;
                _rectTransform.pivot = Vector2.zero;
                _rectTransform.anchoredPosition = new Vector2(Screen.safeArea.x / canvasScale.x, Screen.safeArea.y / canvasScale.y);
                _rectTransform.sizeDelta = new Vector2(Screen.safeArea.width / canvasScale.x, Screen.safeArea.height / canvasScale.y);
            }
        }
    }
}
