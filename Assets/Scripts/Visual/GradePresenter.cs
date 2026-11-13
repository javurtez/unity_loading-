using TMPro;
using UnityEngine;

namespace LoadingGame
{
    /// <summary>Shows the PERFECT / GREAT / GOOD / OK / MISS label after a hit.</summary>
    public class GradePresenter : MonoBehaviour
    {
        [SerializeField] private VisualTheme theme;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UiShake labelShake;
        [Tooltip("Container shaken on a perfect hit.")]
        [SerializeField] private UiShake stageShake;

        /// <summary>Plays the grade for the given absolute error and returns the tier used.</summary>
        public VisualTheme.GradeTier Show(int absoluteError)
        {
            VisualTheme.GradeTier tier = theme.GetGrade(absoluteError);
            ResetState();
            label.text = tier.label;
            label.color = tier.color;
            Animate(tier);
            return tier;
        }

        /// <summary>Hides the label immediately.</summary>
        public void Hide()
        {
            ResetState();
            canvasGroup.alpha = 0f;
        }

        private void Animate(VisualTheme.GradeTier tier)
        {
            GameObject go = label.gameObject;
            VisualTheme.Timings timings = theme.timings;
            switch (tier.animation)
            {
                case GradeAnimation.PunchWobble:
                    canvasGroup.alpha = 1f;
                    go.transform.localScale = Vector3.zero;
                    LeanTween.scale(go, Vector3.one, tier.duration).setEase(LeanTweenType.easeOutBack).setOvershoot(2.4f);
                    go.transform.localEulerAngles = new Vector3(0f, 0f, timings.wobbleDegrees);
                    LeanTween.rotateZ(go, 0f, tier.duration).setEase(LeanTweenType.easeOutElastic);
                    if (stageShake != null) stageShake.Play(timings.stageShakePixels, timings.stageShake, 3f);
                    break;
                case GradeAnimation.PopBack:
                    PopScale(go, tier.duration, LeanTweenType.easeOutBack);
                    break;
                case GradeAnimation.PopQuad:
                    PopScale(go, tier.duration, LeanTweenType.easeOutQuad);
                    break;
                case GradeAnimation.Fade:
                    FadeIn(tier.duration);
                    break;
                case GradeAnimation.FadeShake:
                    FadeIn(tier.duration);
                    labelShake.Play(timings.missShakePixels, timings.missShake, 3f);
                    break;
            }
        }

        private void PopScale(GameObject go, float duration, LeanTweenType ease)
        {
            canvasGroup.alpha = 1f;
            go.transform.localScale = Vector3.zero;
            LeanTween.scale(go, Vector3.one, duration).setEase(ease);
        }

        private void FadeIn(float duration)
        {
            canvasGroup.alpha = 0f;
            LeanTween.alphaCanvas(canvasGroup, 1f, duration);
        }

        private void ResetState()
        {
            LeanTween.cancel(label.gameObject);
            LeanTween.cancel(canvasGroup.gameObject);
            labelShake.Stop();
            label.transform.localScale = Vector3.one;
            label.transform.localEulerAngles = Vector3.zero;
        }
    }
}
