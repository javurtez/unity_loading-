using LoadingGame;
using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class LoadSlot : BaseSlot
{
    [SerializeField] private Sprite icon;

    public Image correctPercentageImage;
    public Image loadPercentageImage;

    [Tooltip("Color family used by the visual theme. Set by Tools > Loading > Apply Modern Theme.")]
    [SerializeField] private ShapeColorFamily colorFamily;
    [Tooltip("Used to dim the shape on game over.")]
    [SerializeField] private CanvasGroup canvasGroup;

    /// <summary>Color family of this shape, used to look up theme colors.</summary>
    public ShapeColorFamily ColorFamily => colorFamily;

    /// <summary>Canvas group on the slot root, or null if the prefab has none.</summary>
    public CanvasGroup CanvasGroup => canvasGroup;

    public override void Open()
    {
        base.Open();

        correctPercentageImage.fillAmount = 0;
        loadPercentageImage.fillAmount = 0;
    }

#if UNITY_EDITOR
    [Button]
    private void UpdateIcon()
    {
        var images = GetComponentsInChildren<Image>();
        foreach (var item in images)
        {
            item.sprite = icon;
        }
    }
#endif
}
