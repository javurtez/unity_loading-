using LoadingGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanel : BaseStaticPanel<LoadPanel>
{
    public TextMeshProUGUI targetPercentageText;
    public TextMeshProUGUI hitPercentageText;

    public Button nextButton;
    public Button retryButton;
    public Button playButton;

    public LoadSlot[] loadSlotPrefabs;

    private int loadSlotIndex = 0;
    private LoadSlot[] loadSlots;

    public LoadSlot loadSlot;
    public GameObject target, hit;

    [Tooltip("Parent for the instantiated shape slots. Falls back to the panel root when empty.")]
    [SerializeField] private RectTransform slotContainer;
    [Tooltip("Optional visual feedback. The panel works without it.")]
    [SerializeField] private LoadPanelView view;
    [SerializeField] private VisualTheme theme;

    protected void Start()
    {
        target.SetActive(false);
        hit.SetActive(false);

        playButton.transform.localScale = Vector3.zero;
        playButton.interactable = false;

        LeanTween.scale(playButton.gameObject, Vector2.one, .3f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
        {
            playButton.interactable = true;
        });

        Technical.Shuffle(loadSlotPrefabs);
        Transform slotParent = slotContainer != null ? slotContainer : transform;
        loadSlots = new LoadSlot[loadSlotPrefabs.Length];
        int index = 0;
        foreach (var item in loadSlotPrefabs)
        {
            var loadSlot = Instantiate(item, slotParent);
            loadSlot.name = item.name;
            loadSlot.Close();
            loadSlots[index] = loadSlot;
            index++;
        }

        if (view != null) view.BeginIdle(loadSlots[loadSlotIndex]);
    }

    public void GetCurrentLoad()
    {
        loadSlot?.Close();

        loadSlot = loadSlots[loadSlotIndex];
        loadSlot.Open();

        loadSlotIndex = (loadSlotIndex + 1) % loadSlots.Length;

        if (view != null) view.OnSlotChanged(loadSlot);
    }

    public void AdjustLoading(float Percentage)
    {
        loadSlot.loadPercentageImage.fillAmount = Percentage / 100f;
        if (view != null) view.OnLoading(Percentage);
    }
    public void AdjustHit(int targetPercentage, int hitPercentage)
    {
        if (targetPercentage < 0)
        {
            hitPercentageText.text = "";
            loadSlot.correctPercentageImage.fillAmount = 0;
            nextButton.gameObject.SetActive(false);
            retryButton.gameObject.SetActive(false);
            if (view != null) view.OnRoundReset();
            return;
        }
        loadSlot.loadPercentageImage.fillAmount = hitPercentage / 100f;
        hitPercentageText.text = "" + hitPercentage;
        loadSlot.correctPercentageImage.fillAmount = targetPercentage / 100f;

        if (hitPercentage <= targetPercentage)
        {
            loadSlot.correctPercentageImage.transform.SetAsFirstSibling();
        }
        else
        {
            loadSlot.correctPercentageImage.transform.SetAsLastSibling();
        }

        if (view != null) view.OnHit(targetPercentage, hitPercentage);

        LeanTween.delayedCall(gameObject, .2f, () =>
        {
            if (theme != null)
                UiTweens.PopInButton(nextButton, theme.timings.buttonAppear);
            else
                nextButton.gameObject.SetActive(true);
        });
    }
    public void AdjustLoadTarget(int targetPercentage)
    {
        if (view != null)
            targetPercentageText.SetText("<mspace=0.6em>{0}</mspace><size=60%>%</size>", targetPercentage);
        else
            targetPercentageText.text = targetPercentage + "";
        if (view != null) view.OnTargetSet(targetPercentage);
    }

    public void OnNext()
    {
        GameManager.GameIsNext();
    }
    public void OnRestart()
    {
        GameManager.GameIsRestarting();
    }
    public void OnPlay()
    {
        LoadingManager.Instance.GamePlay();
    }
}