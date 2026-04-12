using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    [Header("Панелі")]
    public GameObject menuPanel;
    public GameObject leaderboardPanel;

    [Header("Кнопки")]
    public Button restartButton;
    public Button quitButton;
    public Button recordsButton;
    public Button closeLeaderboardButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (menuPanel        != null) menuPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

        if (restartButton          != null) restartButton.onClick.AddListener(OnRestart);
        if (quitButton             != null) quitButton.onClick.AddListener(OnQuit);
        if (recordsButton          != null) recordsButton.onClick.AddListener(OnShowRecords);
        if (closeLeaderboardButton != null) closeLeaderboardButton.onClick.AddListener(OnCloseLeaderboard);

        GameStore.OnGameOver += ShowMenu;
    }

    void OnDestroy()
    {
        GameStore.OnGameOver -= ShowMenu;
    }

    public void ShowMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        if (GameStore.Instance != null) GameStore.Instance.ResetSession();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnShowRecords()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        if (LeaderboardUI.Instance != null) LeaderboardUI.Instance.Refresh();
    }

    void OnCloseLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }
}
