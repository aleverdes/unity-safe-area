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

        [ContextMenu("Process")]
        private void Process()
        {
            var referenceResolutionAspect = Vector2.one;
            var aspectRatio = 1f;

            _canvasScaler = GetComponentInParent<CanvasScaler>();
            if (_canvasScaler && _canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                var screen = new Vector2(Screen.width, Screen.height);
                var referenceResolution = _canvasScaler.referenceResolution;
                referenceResolutionAspect.x = referenceResolution.x / screen.x;
                referenceResolutionAspect.y = referenceResolution.y / screen.y;
                aspectRatio = Mathf.Lerp(referenceResolutionAspect.x, referenceResolutionAspect.y, _canvasScaler.matchWidthOrHeight);
            }

            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.zero;
            _rectTransform.pivot = Vector2.zero;
            _rectTransform.anchoredPosition = aspectRatio * new Vector2(Screen.safeArea.x, Screen.safeArea.y);
            _rectTransform.sizeDelta = aspectRatio * new Vector2(Screen.safeArea.width, Screen.safeArea.height);
        }
    }
}
