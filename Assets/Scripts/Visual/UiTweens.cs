using UnityEngine;
using UnityEngine.UI;

namespace LoadingGame
{
    /// <summary>Shared tween helpers for the panels.</summary>
    public static class UiTweens
    {
        /// <summary>
        /// Activates a button, scales it from 0 to 1 with easeOutBack and makes it interactable when the
        /// tween completes. The tween lives on the button, so cancelling panel tweens does not affect it.
        /// </summary>
        public static void PopInButton(Button button, float duration, float delay = 0f)
        {
            GameObject go = button.gameObject;
            go.SetActive(true);
            LeanTween.cancel(go);
            button.interactable = false;
            go.transform.localScale = Vector3.zero;
            LeanTween.scale(go, Vector3.one, duration)
                .setDelay(delay)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() => button.interactable = true);
        }

        /// <summary>Sets the scale to <paramref name="from"/> and settles it back to 1 with easeOutBack.</summary>
        public static void Punch(Transform target, float from, float duration)
        {
            LeanTween.cancel(target.gameObject);
            target.localScale = Vector3.one * from;
            LeanTween.scale(target.gameObject, Vector3.one, duration).setEase(LeanTweenType.easeOutBack);
        }
    }
}
