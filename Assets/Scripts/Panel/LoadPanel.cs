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

    protected void Start()
    {
        target.SetActive(false);
        hit.SetActive(false);

        playButton.transform.localScale = Vector3.zero;
        playButton.interactable = false;

        LeanTween.scale(playButton.gameObject, Vector2.one, .3f).setOnComplete(() =>
        {
            playButton.interactable = true;
        });

        Technical.Shuffle(loadSlotPrefabs);
        loadSlots = new LoadSlot[loadSlotPrefabs.Length];
        int index = 0;
        foreach (var item in loadSlotPrefabs)
        {
            var loadSlot = Instantiate(item, transform);
            loadSlot.name = item.name;
            loadSlot.Close();
            loadSlots[index] = loadSlot;
            index++;
        }
    }

    public void GetCurrentLoad()
    {
        loadSlot?.Close();

        loadSlot = loadSlots[loadSlotIndex];
        loadSlot.Open();

        loadSlotIndex = (loadSlotIndex + 1) % loadSlots.Length;
    }

    public void AdjustLoading(float Percentage)
    {
        loadSlot.loadPercentageImage.fillAmount = Percentage / 100f;
    }
    public void AdjustHit(int targetPercentage, int hitPercentage)
    {
        if (targetPercentage < 0)
        {
            hitPercentageText.text = "";
            loadSlot.correctPercentageImage.fillAmount = 0;
            nextButton.gameObject.SetActive(false);
            retryButton.gameObject.SetActive(false);
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

        LeanTween.delayedCall(gameObject, .2f, () =>
        {
            nextButton.gameObject.SetActive(true);
        });
    }
    public void AdjustLoadTarget(int targetPercentage)
    {
        targetPercentageText.text = targetPercentage + "";
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