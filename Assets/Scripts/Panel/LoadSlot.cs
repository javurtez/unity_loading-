using TMPro;
using UnityEngine.UI;

public class LoadSlot : BaseSlot
{
    public Image correctPercentageImage;
    public Image loadPercentageImage;

    public override void Open()
    {
        base.Open();

        correctPercentageImage.fillAmount = 0;
        loadPercentageImage.fillAmount = 0;
    }
}