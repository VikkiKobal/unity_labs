using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    public static LevelCompleteUI Instance { get; private set; }

    [Header("Панелі")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public GameObject leaderboardPanel;

    [Header("Тексти Перемога")]
    public Text victoryTimeText;
    public Text victoryCoinsText;
    public Text victoryCollisionsText;

    [Header("Тексти Програш")]
    public Text gameOverTimeText;
    public Text gameOverCoinsText;
    public Text gameOverCollisionsText;

    [Header("Кнопки Перемога")]
    public Button victoryRestartButton;
    public Button victoryRecordsButton;
    public Button victoryQuitButton;

    [Header("Кнопки Програш")]
    public Button gameOverRestartButton;
    public Button gameOverRecordsButton;
    public Button gameOverQuitButton;

    [Header("Закрити рекорди")]
    public Button closeLeaderboardButton;

    void Awake() { Instance = this; }

    void Start()
    {
        Hide(victoryPanel);
        Hide(gameOverPanel);
        Hide(leaderboardPanel);

        GameStore.OnGameOver += OnGameOverEvent;

        Bind(victoryRestartButton,    OnRestart);
        Bind(victoryRecordsButton,    OnRecords);
        Bind(victoryQuitButton,       OnQuit);
        Bind(gameOverRestartButton,   OnRestart);
        Bind(gameOverRecordsButton,   OnRecords);
        Bind(gameOverQuitButton,      OnQuit);
        Bind(closeLeaderboardButton,  OnCloseLeaderboard);
    }

    void OnDestroy() { GameStore.OnGameOver -= OnGameOverEvent; }

    public void ShowVictory()
    {
        FillStats(victoryTimeText, victoryCoinsText, victoryCollisionsText);
        Show(victoryPanel);
        OpenLeaderboard();
    }

    void OnGameOverEvent()
    {
        FillStats(gameOverTimeText, gameOverCoinsText, gameOverCollisionsText);
        Show(gameOverPanel);
        OpenLeaderboard();
    }

    void OpenLeaderboard()
    {
        Show(leaderboardPanel);
        if (LeaderboardUI.Instance != null) LeaderboardUI.Instance.Refresh();
    }

    void OnCloseLeaderboard() { Hide(leaderboardPanel); }

    void FillStats(Text timeT, Text coinsT, Text collT)
    {
        if (GameStore.Instance == null) return;
        float sec = GameStore.Instance.LevelTime;
        int m = Mathf.FloorToInt(sec / 60f);
        int s = Mathf.FloorToInt(sec % 60f);
        SetText(timeT,  string.Format("Час: {0:00}:{1:00}", m, s));
        SetText(coinsT, "Монет: "    + GameStore.Instance.CoinsCollected);
        SetText(collT,  "Зіткнень: " + GameStore.Instance.Collisions);
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        if (GameStore.Instance != null) GameStore.Instance.ResetSession();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnRecords() { OpenLeaderboard(); }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void Show(GameObject go) { if (go != null) go.SetActive(true);  }
    static void Hide(GameObject go) { if (go != null) go.SetActive(false); }
    static void SetText(Text t, string v) { if (t != null) t.text = v; }
    static void Bind(Button b, UnityEngine.Events.UnityAction a) { if (b != null) b.onClick.AddListener(a); }
}
