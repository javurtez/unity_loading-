using LoadingGame;
using TMPro;
using UnityEngine;

public class ScorePanel : BaseStaticPanel<ScorePanel>
{
    private const string NumberFormat = "<mspace=0.6em>{0}</mspace>";

    public TextMeshProUGUI hpPercentageText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI maxLevelText;

    [Tooltip("Optional animated HP chip. Falls back to plain text when empty.")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private VisualTheme theme;
    [SerializeField] private RectTransform trophyIcon;

    private int shownLevel;

    /// <summary>Best level recorded before the current run, as last passed to AdjustLevel.</summary>
    public int PreviousBest { get; private set; }

    public override void Open()
    {
        base.Open();

        PreviousBest = LoadingManager.Instance.MaxLevel;
        maxLevelText.SetText(NumberFormat, PreviousBest);
        levelText.SetText(NumberFormat, 0);
        if (healthDisplay != null)
            healthDisplay.SetImmediate(Constants.max_health);
        else
            hpPercentageText.text = Constants.max_health + "";
    }

    public void AdjustHP(int currentHp, int errorPercentage)
    {
        if (healthDisplay != null)
        {
            healthDisplay.Show(currentHp, errorPercentage);
            return;
        }

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
        PreviousBest = maxLevel;
        levelText.SetText(NumberFormat, level);
        if (maxLevel < level)
        {
            maxLevelText.SetText(NumberFormat, level);
        }

        if (theme != null && level > shownLevel)
            UiTweens.Punch(levelText.transform, 1.4f, theme.timings.levelPunch);
        shownLevel = level;
    }

    /// <summary>Spins the trophy icon once to celebrate a new best level.</summary>
    public void PlayNewBest()
    {
        if (theme == null || trophyIcon == null) return;
        LeanTween.cancel(trophyIcon.gameObject);
        trophyIcon.localEulerAngles = Vector3.zero;
        LeanTween.rotateAround(trophyIcon, Vector3.forward, -360f, theme.timings.trophySpin).setEase(LeanTweenType.easeOutCubic);
    }
}
