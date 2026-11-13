using System;
using TMPro;
using UnityEngine;

namespace LoadingGame
{
    /// <summary>
    /// GAME OVER card: title pop, reached level and an optional NEW BEST ribbon. Tweens live on this
    /// object and its children, so LoadingManager cancelling the panel root tweens leaves them running.
    /// </summary>
    public class GameOverPresenter : MonoBehaviour
    {
        [SerializeField] private VisualTheme theme;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform title;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private RectTransform newBestRibbon;

        private int ribbonTweenId = -1;

        /// <summary>
        /// Shows the card after the theme's game over delay. <paramref name="onShown"/> runs when the card
        /// starts appearing, so callers can dim the shape in sync.
        /// </summary>
        public void Play(int level, bool isNewBest, Action onShown)
        {
            Hide();
            LeanTween.delayedCall(gameObject, theme.timings.gameOverDelay, () =>
            {
                onShown?.Invoke();
                Reveal(level, isNewBest);
            });
        }

        /// <summary>Hides the card and stops its tweens.</summary>
        public void Hide()
        {
            LeanTween.cancel(gameObject);
            LeanTween.cancel(title.gameObject);
            StopRibbon();
            canvasGroup.alpha = 0f;
            newBestRibbon.gameObject.SetActive(false);
        }

        private void Reveal(int level, bool isNewBest)
        {
            float pop = theme.timings.gameOverPop;
            canvasGroup.alpha = 0f;
            LeanTween.alphaCanvas(canvasGroup, 1f, pop * 0.5f);

            title.localScale = Vector3.one * 0.8f;
            LeanTween.scale(title.gameObject, Vector3.one, pop).setEase(LeanTweenType.easeOutBack);

            levelText.SetText("LEVEL {0}", level);

            newBestRibbon.gameObject.SetActive(isNewBest);
            if (isNewBest) StartRibbon();
        }

        private void StartRibbon()
        {
            newBestRibbon.localScale = Vector3.one;
            ribbonTweenId = LeanTween.scale(newBestRibbon.gameObject, Vector3.one * 1.08f, 0.5f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong()
                .id;
        }

        private void StopRibbon()
        {
            if (ribbonTweenId < 0) return;
            LeanTween.cancel(newBestRibbon.gameObject, ribbonTweenId);
            ribbonTweenId = -1;
            newBestRibbon.localScale = Vector3.one;
        }
    }
}
