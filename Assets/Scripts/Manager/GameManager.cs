using UnityEngine;

public class GameManager : MonoManager<GameManager>
{
    public Transform panelParent, managerParent;
    public Camera uiCamera;

    public delegate void GameStatus();
    public static GameStatus GameNext;
    public static GameStatus GameRestart;
    public static GameStatus GameOver;
    
    protected override void Awake()
    {
        instance = this;

        LoadingManager.Instance.Create();
        AdManager.Instance.Create();
        RateManager.Instance.Create();
        AudioManager.Instance.Create();

        ScorePanel.Instance.Open();
        LoadPanel.Instance.Open();
    }

    public static void GameIsNext()
    {
        GameNext?.Invoke();
    }
    public static void GameIsOver()
    {
        GameOver?.Invoke();
    }
    public static void GameIsRestarting()
    {
        GameRestart?.Invoke();
    }
}