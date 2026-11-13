using UnityEngine;

namespace LoadingGame
{
    /// <summary>
    /// Damped horizontal shake on a RectTransform's anchored position. The update delegate is cached
    /// so repeated shakes do not allocate a new closure.
    /// </summary>
    public class UiShake : MonoBehaviour
    {
        [SerializeField] private RectTransform target;

        private System.Action<float> onUpdate;
        private Vector2 restPosition;
        private float amplitude;
        private float cycles;
        private int tweenId = -1;

        private void Awake()
        {
            if (target == null) target = (RectTransform)transform;
            onUpdate = Apply;
            restPosition = target.anchoredPosition;
        }

        /// <summary>Shakes horizontally by up to <paramref name="pixels"/> for <paramref name="cycleCount"/> cycles.</summary>
        public void Play(float pixels, float duration, float cycleCount)
        {
            Stop();
            amplitude = pixels;
            cycles = cycleCount;
            tweenId = LeanTween.value(gameObject, onUpdate, 0f, 1f, duration).id;
        }

        /// <summary>Stops the shake and returns to the rest position.</summary>
        public void Stop()
        {
            if (tweenId >= 0)
            {
                LeanTween.cancel(gameObject, tweenId);
                tweenId = -1;
            }
            target.anchoredPosition = restPosition;
        }

        private void Apply(float t)
        {
            float offset = amplitude * Mathf.Sin(t * cycles * Mathf.PI * 2f) * (1f - t);
            target.anchoredPosition = restPosition + new Vector2(offset, 0f);
            if (t >= 1f) tweenId = -1;
        }
    }
}
