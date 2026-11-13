using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadingGame
{
    /// <summary>
    /// HP chip feedback: counting number, HP bar, pooled floating delta labels, low HP pulse and a
    /// border flash on damage. Purely visual; the HP value comes from LoadingManager through ScorePanel.
    /// </summary>
    public class HealthDisplay : MonoBehaviour
    {
        private const string NumberFormat = "<mspace=0.6em>{0}</mspace>";
        private const float DeltaRise = 60f;
        private const float LowHpScale = 1.1f;

        [SerializeField] private VisualTheme theme;
        [SerializeField] private TextMeshProUGUI hpText;
        [Tooltip("Bar fill whose anchorMax.x is driven by hp / max health.")]
        [SerializeField] private RectTransform barFill;
        [SerializeField] private Image chipBorder;
        [Tooltip("Pre-built labels reused round robin for +N / -N popups.")]
        [SerializeField] private TextMeshProUGUI[] deltaLabels;
        [SerializeField] private Vector2 deltaOrigin = new Vector2(90f, 0f);

        private System.Action<float> onCount;
        private System.Action<float> onBar;
        private float shownHp = Constants.max_health;
        private float shownFill = 1f;
        private int nextLabel;
        private int pulseTweenId = -1;
        private CanvasGroup[] deltaGroups;

        private void Awake()
        {
            onCount = SetNumber;
            onBar = SetBar;
            deltaGroups = new CanvasGroup[deltaLabels.Length];
            for (int i = 0; i < deltaLabels.Length; i++)
            {
                deltaGroups[i] = deltaLabels[i].GetComponent<CanvasGroup>();
                deltaGroups[i].alpha = 0f;
            }
        }

        /// <summary>Shows the HP immediately without animation.</summary>
        public void SetImmediate(int hp)
        {
            LeanTween.cancel(gameObject);
            LeanTween.cancel(barFill.gameObject);
            SetNumber(hp);
            SetBar(ToFill(hp));
            UpdateLowHp(hp);
        }

        /// <summary>Animates to a new HP value and pops a delta label when <paramref name="delta"/> is not zero.</summary>
        public void Show(int hp, int delta)
        {
            float duration = theme.timings.hpCount;
            float displayHp = Mathf.Max(0, hp);

            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, onCount, shownHp, displayHp, duration).setEase(LeanTweenType.easeOutCubic);
            LeanTween.cancel(barFill.gameObject);
            LeanTween.value(barFill.gameObject, onBar, shownFill, ToFill(hp), duration).setEase(LeanTweenType.easeOutCubic);

            UpdateLowHp(hp);
            if (delta != 0) PopDelta(delta);
            if (delta < 0) FlashBorder();
        }

        private void PopDelta(int delta)
        {
            TextMeshProUGUI label = deltaLabels[nextLabel];
            CanvasGroup group = deltaGroups[nextLabel];
            nextLabel = (nextLabel + 1) % deltaLabels.Length;

            if (delta > 0) label.SetText("+{0}", delta);
            else label.SetText("{0}", delta);
            label.color = delta > 0 ? theme.success : theme.error;

            RectTransform rect = label.rectTransform;
            float duration = theme.timings.deltaFloat;
            LeanTween.cancel(rect.gameObject);
            rect.anchoredPosition = deltaOrigin;
            group.alpha = 1f;
            LeanTween.move(rect, new Vector3(deltaOrigin.x, deltaOrigin.y + DeltaRise, 0f), duration).setEase(LeanTweenType.easeOutCubic);
            LeanTween.alphaCanvas(group, 0f, duration).setEase(LeanTweenType.easeOutCubic);
        }

        private void FlashBorder()
        {
            LeanTween.cancel(chipBorder.gameObject);
            chipBorder.color = theme.error;
            LeanTween.color(chipBorder.rectTransform, theme.border, theme.timings.hpCount);
        }

        private void UpdateLowHp(int hp)
        {
            bool low = hp < theme.lowHpThreshold;
            hpText.color = low ? theme.error : theme.textPrimary;
            if (low && pulseTweenId < 0)
            {
                pulseTweenId = LeanTween.scale(hpText.gameObject, Vector3.one * LowHpScale, theme.timings.lowHpPulse * 0.5f)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setLoopPingPong()
                    .id;
            }
            else if (!low && pulseTweenId >= 0)
            {
                LeanTween.cancel(hpText.gameObject, pulseTweenId);
                pulseTweenId = -1;
                hpText.transform.localScale = Vector3.one;
            }
        }

        private void SetNumber(float value)
        {
            shownHp = value;
            hpText.SetText(NumberFormat, Mathf.Round(value));
        }

        private void SetBar(float fill)
        {
            shownFill = fill;
            Vector2 max = barFill.anchorMax;
            max.x = fill;
            barFill.anchorMax = max;
        }

        private static float ToFill(int hp)
        {
            return Mathf.Clamp01(hp / (float)Constants.max_health);
        }
    }
}
