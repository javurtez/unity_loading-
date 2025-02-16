using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class LoadSlot : BaseSlot
{
    [SerializeField] private Sprite icon;

    public Image correctPercentageImage;
    public Image loadPercentageImage;

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