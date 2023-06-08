using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoManager<LoadingManager>
{
    public int healthPoints = 30;

    public int targetPercentage;
    public int hitPercentage;
    public int errorPercentage;

    public int level = 0;

    public List<int> listOfPercentage = new List<int>();

    private int maxLevel = 0;
    private float loadingSpeed = 20;
    private float currentLoad;
    private bool isHit = false;

    private const int maxLoadCount = 10;

    public int MaxLevel => maxLevel;

    protected override void Awake()
    {
        GameManager.GameRestart += Restart;
        GameManager.GameNext += NextRound;
        GameManager.GameOver += GameOver;

        isHit = true;
        maxLevel = PlayerPrefs.GetInt(Constants.saveLevelId);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (isHit) return;
        currentLoad += loadingSpeed * Time.deltaTime;
        currentLoad = Mathf.Clamp(currentLoad, 0, 100);
        LoadPanel.Instance.AdjustLoading(currentLoad);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            HitTarget((int)currentLoad);
        }
#elif UNITY_ANDROID
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || currentLoad > 100)
        {
            HitTarget((int)currentLoad);
        }
#endif
    }


    public void GamePlay()
    {
        LoadPanel.Instance.playButton.gameObject.SetActive(false);
        LoadPanel.Instance.target.SetActive(true);
        LoadPanel.Instance.hit.SetActive(true);
        Restart();
    }
    private void GameOver()
    {
        LeanTween.cancel(LoadPanel.Instance.gameObject);

        if (Technical.RandomPercentage() >= 35)
        {
            AdManager.Instance.OnInterstitialShow();
        }
        //else if (RemoteManager.Instance.CanShow)
        //{
        //    CrossPromo.Instance.ShowCrossPromoPopup();
        //}

        LoadPanel.Instance.retryButton.gameObject.SetActive(true);
        LoadPanel.Instance.nextButton.gameObject.SetActive(false);

        if (maxLevel < level)
        {
            PlayerPrefs.SetInt(Constants.saveLevelId, level);
            maxLevel = level;
        }
    }
    private void Restart()
    {
        healthPoints = Constants.max_health;
        level = 1;
        if (listOfPercentage == null || listOfPercentage.Count == 0)
        {
            listOfPercentage.Clear();
            for (int x = 0; x < maxLoadCount; x++)
            {
                listOfPercentage.Add(Random.Range(10, 96));
            }
        }
        NextRound();
        ScorePanel.Instance.AdjustLevel(maxLevel, level);
    }
    private void NextRound()
    {
        GetTarget();
        currentLoad = 0;
        LeanTween.delayedCall(.5f, () =>
        {
            isHit = false;
        });
        LoadPanel.Instance.AdjustLoading(currentLoad);
        LoadPanel.Instance.AdjustHit(-1, -1);
        ScorePanel.Instance.AdjustHP(healthPoints, 0);
    }
    public void GetTarget()
    {
        if (listOfPercentage.Count > 0)
        {
            targetPercentage = listOfPercentage[0];
            listOfPercentage.RemoveAt(0);
        }
        else
        {
            targetPercentage = Random.Range(15, 96);
        }
        LoadPanel.Instance.GetCurrentLoad();
        LoadPanel.Instance.AdjustLoadTarget(targetPercentage);
    }

    public void HitTarget(int hit)
    {
        isHit = true;
        int remainder = targetPercentage - hit;
        if (remainder > 0)
        {
            remainder *= -1;
        }

        errorPercentage = remainder;
        if (errorPercentage == 0)
        {
            errorPercentage = 50;
        }

        healthPoints += errorPercentage;
        ScorePanel.Instance.AdjustHP(healthPoints, errorPercentage);
        LoadPanel.Instance.AdjustHit(targetPercentage, hit);
        if (healthPoints < 0)
        {
            //GameOver
            GameManager.GameIsOver();
        }
        else
        {
            level++;
            loadingSpeed += 1;
            loadingSpeed = Mathf.Clamp(loadingSpeed, 10, 40);
            ScorePanel.Instance.AdjustLevel(maxLevel, level);
        }
        AudioManager.Instance.PlayClick();
    }
}