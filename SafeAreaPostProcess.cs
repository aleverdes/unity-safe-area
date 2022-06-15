using UnityEngine;
namespace AffenCode
{
    public abstract class SafeAreaPostProcess : MonoBehaviour
    {
        public abstract void PostProcess(SafeArea safeArea);
    }
}
