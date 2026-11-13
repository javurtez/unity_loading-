using UnityEngine;
using UnityEngine.UI;

namespace LoadingGame
{
    /// <summary>
    /// A small marker that points at a percentage on a filled Image. Radial fills rotate the marker
    /// around the shape center; vertical and horizontal fills slide it along the shape edge.
    /// Sits on a zero-size pivot centered on the shape, with the mark Image as its child.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PercentTick : MonoBehaviour
    {
        [SerializeField] private RectTransform pivot;
        [SerializeField] private RectTransform mark;
        [SerializeField] private Image markImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private int pulseTweenId = -1;

        /// <summary>Whether the alpha pulse is running.</summary>
        public bool IsPulsing => pulseTweenId >= 0;

        /// <summary>Points the tick at a percentage of the given fill image.</summary>
        public void Place(Image fill, float percentage, VisualTheme theme)
        {
            float t = Mathf.Clamp01(percentage / 100f);
            Vector2 size = GetDrawnSize(fill);
            switch (fill.fillMethod)
            {
                case Image.FillMethod.Vertical:
                    PlaceVertical(size, t, fill.fillOrigin == (int)Image.OriginVertical.Top, theme.tickEdgeOffset);
                    break;
                case Image.FillMethod.Horizontal:
                    PlaceHorizontal(size, t, fill.fillOrigin == (int)Image.OriginHorizontal.Right, theme.tickEdgeOffset);
                    break;
                default:
                    PlaceRadial(t, fill.fillOrigin, fill.fillClockwise, theme.tickRadius);
                    break;
            }
            MatchFillRotation(fill.rectTransform);
        }

        /// <summary>Shows the tick at full alpha with the given color.</summary>
        public void Show(Color color)
        {
            StopPulse();
            markImage.color = color;
            canvasGroup.alpha = 1f;
        }

        /// <summary>Changes the mark color without changing visibility.</summary>
        public void Tint(Color color)
        {
            markImage.color = color;
        }

        /// <summary>Hides the tick and stops any pulse.</summary>
        public void Hide()
        {
            StopPulse();
            canvasGroup.alpha = 0f;
        }

        /// <summary>Starts a looping alpha pulse between 0.6 and 1.</summary>
        public void StartPulse(float halfPeriod)
        {
            if (IsPulsing) return;
            canvasGroup.alpha = 1f;
            pulseTweenId = LeanTween.alphaCanvas(canvasGroup, 0.6f, halfPeriod)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong()
                .id;
        }

        /// <summary>Stops the pulse and restores full alpha.</summary>
        public void StopPulse()
        {
            if (!IsPulsing) return;
            LeanTween.cancel(canvasGroup.gameObject, pulseTweenId);
            pulseTweenId = -1;
            canvasGroup.alpha = 1f;
        }

        private void PlaceRadial(float t, int origin, bool clockwise, float radius)
        {
            // The mark rests straight below the pivot. Origin360 order is Bottom, Right, Top, Left,
            // so each origin step is a 90 degree counter-clockwise turn of that rest position.
            float baseAngle = origin * 90f;
            float sweep = t * 360f * (clockwise ? -1f : 1f);
            pivot.localEulerAngles = new Vector3(0f, 0f, baseAngle + sweep);
            mark.anchoredPosition = new Vector2(0f, -radius);
            mark.localEulerAngles = Vector3.zero;
        }

        private void PlaceVertical(Vector2 size, float t, bool fromTop, float offset)
        {
            float y = fromTop ? size.y * (0.5f - t) : size.y * (t - 0.5f);
            pivot.localEulerAngles = Vector3.zero;
            mark.anchoredPosition = new Vector2(size.x * 0.5f + offset, y);
            mark.localEulerAngles = new Vector3(0f, 0f, 90f);
        }

        private void PlaceHorizontal(Vector2 size, float t, bool fromRight, float offset)
        {
            float x = fromRight ? size.x * (0.5f - t) : size.x * (t - 0.5f);
            pivot.localEulerAngles = Vector3.zero;
            mark.anchoredPosition = new Vector2(x, -(size.y * 0.5f + offset));
            mark.localEulerAngles = Vector3.zero;
        }

        /// <summary>
        /// Placement above is computed in the fill's own space. Shape prefabs may be rotated (Box 2 turns a
        /// horizontal fill into an upward one), so rotate the pivot by the fill's on-screen rotation.
        /// </summary>
        private void MatchFillRotation(RectTransform fillRect)
        {
            float parentZ = pivot.parent != null ? pivot.parent.eulerAngles.z : 0f;
            float offset = fillRect.eulerAngles.z - parentZ;
            Vector3 euler = pivot.localEulerAngles;
            euler.z += offset;
            pivot.localEulerAngles = euler;
        }

        private static Vector2 GetDrawnSize(Image fill)
        {
            Vector2 rectSize = fill.rectTransform.rect.size;
            if (!fill.preserveAspect || fill.sprite == null) return rectSize;

            Vector2 spriteSize = fill.sprite.rect.size;
            float scale = Mathf.Min(rectSize.x / spriteSize.x, rectSize.y / spriteSize.y);
            return spriteSize * scale;
        }
    }
}
