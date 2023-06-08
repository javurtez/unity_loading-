using TMPro;

public class ScorePanel : BaseStaticPanel<ScorePanel>
{
    public TextMeshProUGUI hpPercentageText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI maxLevelText;

    public override void Open()
    {
        base.Open();

        maxLevelText.text = "" + LoadingManager.Instance.MaxLevel;
        hpPercentageText.text = Constants.max_health + "";
        levelText.text = "0";
    }

    public void AdjustHP(int currentHp, int errorPercentage)
    {
        string errorString = errorPercentage == 0 ? "" : "-" + errorPercentage;
        if (errorPercentage == 0)
            errorString = "";
        else if (errorPercentage > 0)
            errorString = "+" + errorPercentage;
        else if (errorPercentage < 0)
            errorString = "" + errorPercentage;

        hpPercentageText.text = currentHp + "<pos=50%>" + errorString;
    }
    public void AdjustLevel(int maxLevel, int level)
    {
        levelText.text = level + "";
        if (maxLevel < level)
        {
            maxLevelText.text = level + "";
        }
    }
}