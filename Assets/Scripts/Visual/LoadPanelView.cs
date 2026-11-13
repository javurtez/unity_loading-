using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadingGame
{
    /// <summary>
    /// Visual feedback for LoadPanel: title and idle demo, target and hit ticks, tap juice, the result
    /// row, grades and the game over card. LoadPanel forwards its existing calls here; gameplay state is
    /// only read, never changed.
    /// </summary>
    public class LoadPanelView : MonoBehaviour
    {
        [SerializeField] private VisualTheme theme;

        [Header("Title")]
        [SerializeField] private CanvasGroup titleGroup;
        [Tooltip("Decorative center line, shown on the title screen only. Optional.")]
        [SerializeField] private CanvasGroup hairlineGroup;

        [Header("Shape")]
        [SerializeField] private Image glowImage;
        [SerializeField] private PercentTick targetTick;
        [SerializeField] private PercentTick hitTick;
        [SerializeField] private CanvasGroup flashGroup;
        [SerializeField] private Image flashImage;

        [Header("Loading")]
        [SerializeField] private RectTransform targetNumber;
        [SerializeField] private CanvasGroup hintGroup;

        [Header("Result")]
        [SerializeField] private CanvasGroup hitRowGroup;
        [SerializeField] private TextMeshProUGUI fromText;
        [SerializeField] private TextMeshProUGUI deltaText;
        [SerializeField] private GradePresenter grade;
        [SerializeField] private GameOverPresenter gameOver;

        private const float HintAlpha = 0.5f;
        private const float GameOverShapeAlpha = 0.4f;

        private LoadSlot idleSlot;
        private int idleTweenId = -1;
        private System.Action<float> onIdleUpdate;

        private LoadSlot currentSlot;
        private int currentTarget;
        private bool pulseArmed;

        private void Awake()
        {
            onIdleUpdate = SetIdleFill;
        }

        private void OnEnable()
        {
            GameManager.GameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameManager.GameOver -= HandleGameOver;
        }

        /// <summary>Shows the title state with a looping demo fill on <paramref name="slot"/>.</summary>
        public void BeginIdle(LoadSlot slot)
        {
            titleGroup.alpha = 1f;
            HideRoundElements();
            targetTick.Hide();
            idleSlot = slot;
            slot.Open();
            ApplySlotTint(slot, true);
            idleTweenId = LeanTween.value(slot.gameObject, onIdleUpdate, 0f, 1f, theme.timings.idleDemoFill)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopClamp()
                .id;
        }

        /// <summary>Called when LoadPanel switches to a new shape for the round.</summary>
        public void OnSlotChanged(LoadSlot slot)
        {
            EndIdle(slot);
            currentSlot = slot;
            if (slot.CanvasGroup != null)
            {
                LeanTween.cancel(slot.CanvasGroup.gameObject);
                slot.CanvasGroup.alpha = 1f;
            }
            ApplySlotTint(slot, false);
            flashImage.sprite = slot.loadPercentageImage.sprite;
            flashImage.preserveAspect = slot.loadPercentageImage.preserveAspect;
        }

        /// <summary>Called when a new target is shown. Places the target tick and plays the round start pop.</summary>
        public void OnTargetSet(int target)
        {
            currentTarget = target;
            if (currentSlot == null) return;

            Color tickColor = theme.GetFamily(currentSlot.ColorFamily).target;
            targetTick.Place(currentSlot.loadPercentageImage, target, theme);
            targetTick.Show(tickColor);
            pulseArmed = true;

            GameObject shape = currentSlot.gameObject;
            LeanTween.cancel(shape);
            shape.transform.localScale = Vector3.one * 0.85f;
            LeanTween.scale(shape, Vector3.one, theme.timings.roundStartPop).setEase(LeanTweenType.easeOutBack);

            UiTweens.Punch(targetNumber, 1.3f, theme.timings.targetPunch);
        }

        /// <summary>Called every loading step with the current fill percentage. Allocation free.</summary>
        public void OnLoading(float percentage)
        {
            if (!pulseArmed || percentage < currentTarget - theme.tickPulseLead) return;
            pulseArmed = false;
            targetTick.StartPulse(theme.timings.tickPulse);
        }

        /// <summary>Called when a round is reset before loading starts.</summary>
        public void OnRoundReset()
        {
            HideRoundElements();
            bool showHint = LoadingManager.Instance.level <= theme.hintMaxLevel;
            LeanTween.cancel(hintGroup.gameObject);
            hintGroup.alpha = showHint ? HintAlpha : 0f;
        }

        /// <summary>Called after the player stops the fill.</summary>
        public void OnHit(int target, int hit)
        {
            pulseArmed = false;
            int signedDelta = hit - target;
            int absoluteError = signedDelta < 0 ? -signedDelta : signedDelta;

            LeanTween.alphaCanvas(hintGroup, 0f, 0.15f);
            PlayTapJuice();

            VisualTheme.GradeTier tier = grade.Show(absoluteError);
            targetTick.Show(tier.countsAsGoodHit ? theme.success : theme.error);
            hitTick.Place(currentSlot.loadPercentageImage, hit, theme);
            hitTick.Show(theme.textPrimary);

            ShowHitRow(target, signedDelta, tier.color);
        }

        private void HandleGameOver()
        {
            LoadingManager manager = LoadingManager.Instance;
            bool isNewBest = manager.level > ScorePanel.Instance.PreviousBest;
            gameOver.Play(manager.level, isNewBest, DimForGameOver);
            if (isNewBest) ScorePanel.Instance.PlayNewBest();

            LoadPanel panel = LoadPanel.Instance;
            UiTweens.PopInButton(panel.retryButton, theme.timings.buttonAppear,
                theme.timings.gameOverDelay + theme.timings.retryDelay);
        }

        private void DimForGameOver()
        {
            grade.Hide();
            hitRowGroup.alpha = 0f;
            if (currentSlot == null || currentSlot.CanvasGroup == null) return;
            LeanTween.alphaCanvas(currentSlot.CanvasGroup, GameOverShapeAlpha, theme.timings.gameOverDim);
        }

        private void EndIdle(LoadSlot nextSlot)
        {
            if (idleSlot == null) return;
            if (idleTweenId >= 0) LeanTween.cancel(idleSlot.gameObject, idleTweenId);
            idleTweenId = -1;
            idleSlot.loadPercentageImage.fillAmount = 0f;
            if (idleSlot != nextSlot) idleSlot.Close();
            idleSlot = null;
            LeanTween.alphaCanvas(titleGroup, 0f, 0.2f);
            if (hairlineGroup != null) LeanTween.alphaCanvas(hairlineGroup, 0f, 0.2f);
        }

        private void SetIdleFill(float value)
        {
            if (idleSlot != null) idleSlot.loadPercentageImage.fillAmount = value;
        }

        private void ApplySlotTint(LoadSlot slot, bool instant)
        {
            Color glow = VisualTheme.WithAlpha(theme.GetFamily(slot.ColorFamily).fill, theme.glowAlpha);
            LeanTween.cancel(glowImage.gameObject);
            if (instant)
            {
                glowImage.color = glow;
                return;
            }
            LeanTween.color(glowImage.rectTransform, glow, theme.timings.glowTint);
        }

        private void PlayTapJuice()
        {
            VisualTheme.Timings timings = theme.timings;
            GameObject shape = currentSlot.gameObject;
            LeanTween.cancel(shape);
            LeanTween.scale(shape, Vector3.one * 0.92f, timings.tapSquashIn).setEase(LeanTweenType.easeOutQuad);
            LeanTween.scale(shape, Vector3.one, timings.tapSquashOut)
                .setDelay(timings.tapSquashIn)
                .setEase(LeanTweenType.easeOutElastic);

            LeanTween.cancel(flashGroup.gameObject);
            flashGroup.alpha = 0.5f;
            LeanTween.alphaCanvas(flashGroup, 0f, timings.flash);
        }

        private void ShowHitRow(int target, int signedDelta, Color deltaColor)
        {
            fromText.SetText("{0}", target);
            if (signedDelta > 0) deltaText.SetText("(+{0})", signedDelta);
            else deltaText.SetText("({0})", signedDelta);
            deltaText.color = deltaColor;

            LeanTween.cancel(hitRowGroup.gameObject);
            hitRowGroup.alpha = 0f;
            LeanTween.alphaCanvas(hitRowGroup, 1f, 0.2f);
        }

        private void HideRoundElements()
        {
            grade.Hide();
            gameOver.Hide();
            hitTick.Hide();
            LeanTween.cancel(hitRowGroup.gameObject);
            hitRowGroup.alpha = 0f;
            hintGroup.alpha = 0f;
            flashGroup.alpha = 0f;
        }
    }
}
