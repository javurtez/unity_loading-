using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LoadingGame
{
    /// <summary>Scales an interactable button down while it is pressed.</summary>
    public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Selectable selectable;
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float duration = 0.08f;

        private int tweenId = -1;

        /// <inheritdoc />
        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable != null && selectable.IsInteractable()) ScaleTo(pressedScale);
        }

        /// <inheritdoc />
        public void OnPointerUp(PointerEventData eventData)
        {
            if (selectable != null && selectable.IsInteractable()) ScaleTo(1f);
        }

        private void ScaleTo(float scale)
        {
            if (tweenId >= 0) LeanTween.cancel(gameObject, tweenId);
            tweenId = LeanTween.scale(gameObject, Vector3.one * scale, duration).setEase(LeanTweenType.easeOutQuad).id;
        }
    }
}
