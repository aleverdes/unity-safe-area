using UnityEngine;
namespace AffenCode
{
    /// <summary>
    /// Base class for SafeArea post-processing components
    /// Post-processors allow you to modify the calculated safe area position and size
    /// after the main positioning strategy has been applied.
    /// </summary>
    public abstract class SafeAreaPostProcess : MonoBehaviour
    {
        /// <summary>
        /// Called after the main SafeArea positioning has been calculated
        /// Use this to modify the RectTransform's anchoredPosition and sizeDelta
        /// </summary>
        /// <param name="safeArea">The SafeArea component that triggered this post-process</param>
        public abstract void PostProcess(SafeArea safeArea);

        /// <summary>
        /// Optional validation method called in editor
        /// Return false if the post-processor configuration is invalid
        /// </summary>
        protected virtual bool ValidateConfiguration()
        {
            return true;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!ValidateConfiguration())
            {
                Debug.LogWarning($"Invalid configuration in {GetType().Name} on {gameObject.name}", this);
            }
        }
#endif
    }
}
