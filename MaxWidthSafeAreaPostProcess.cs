namespace AffenCode
{
    public class MaxWidthSafeAreaPostProcess : SafeAreaPostProcess
    {
        public float MaxWidthForHeight = 2f;

        public override void PostProcess(SafeArea safeArea)
        {
            var position = safeArea.RectTransform.anchoredPosition;
            var size = safeArea.RectTransform.sizeDelta;

            if (size.x <= MaxWidthForHeight * size.y)
            {
                return;
            }

            var width = MaxWidthForHeight * size.y;
            var widthDelta = size.x - width;

            position.x += 0.5f * widthDelta;
            size.x = width;

            safeArea.RectTransform.anchoredPosition = position;
            safeArea.RectTransform.sizeDelta = size;
        }
    }
}
