using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
    public int coinsCollected;
    public float timeTaken;
}

[Serializable]
public class GameData
{
    public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
}

/// <summary>
/// Глобальне сховище стану гри. Єдиний екземпляр (Singleton).
/// Зберігає: кількість життів, зіткнення, монети, час, таблицю рекордів.
/// Генерує подію OnGameOver при втраті всіх життів або закінченні часу.
/// </summary>
public class GameStore : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────
    public static GameStore Instance { get; private set; }

    // ── Подія програшу ───────────────────────────────────────────────
    public static event Action OnGameOver;

    // ── Налаштування (Inspector) ──────────────────────────────────────
    [Header("Налаштування")]
    public int maxLives = 3;
    public float levelTimeLimit = 60f;

    // ── Стан гри ─────────────────────────────────────────────────────
    public int Lives          { get; private set; }
    public int Collisions     { get; private set; }
    public int CoinsCollected { get; private set; }
    public float LevelTime    { get; private set; }
    public bool IsGameOver    { get; private set; }
    public List<LeaderboardEntry> Leaderboard { get; private set; } = new List<LeaderboardEntry>();

    private string SavePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

    // ── Lifecycle ─────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Підписуємо власний обробник події програшу
        OnGameOver += HandleGameOver;

        LoadData();
        ResetSession();
    }

    void OnDestroy()
    {
        OnGameOver -= HandleGameOver;
    }

    void Update()
    {
        if (IsGameOver) return;

        LevelTime += Time.deltaTime;

        if (LevelTime >= levelTimeLimit)
        {
            LevelTime = levelTimeLimit;
            TriggerGameOver("Час вичерпано!");
        }
    }

    void OnApplicationQuit()
    {
        SaveData();
    }

    // ── Публічні методи ───────────────────────────────────────────────

    /// <summary>Викликати при потраплянні в пастку або яму.</summary>
    public void LoseLife()
    {
        if (IsGameOver) return;

        Collisions++;
        Lives = Mathf.Max(0, Lives - 1);

        Debug.Log($"[GameStore] Втрачено життя! Залишилось: {Lives} | Зіткнень: {Collisions}");

        if (Lives <= 0)
            TriggerGameOver("Всі життя витрачені!");
    }

    /// <summary>Викликати при збиранні монети.</summary>
    public void AddCoin()
    {
        if (IsGameOver) return;

        CoinsCollected++;
        Debug.Log($"[GameStore] Монета зібрана! Всього: {CoinsCollected}");
    }

    /// <summary>Викликати при завершенні рівня.</summary>
    public void OnLevelComplete()
    {
        if (IsGameOver) return;

        var entry = new LeaderboardEntry
        {
            coinsCollected = CoinsCollected,
            timeTaken      = LevelTime
        };
        Leaderboard.Add(entry);

        Debug.Log($"[GameStore] Рівень пройдено! Монет: {CoinsCollected} | Час: {LevelTime:F2}s");
        PrintLeaderboard();
        SaveData();
    }

    public void PrintLeaderboard()
    {
        Debug.Log("[GameStore] ═══ ТАБЛИЦЯ РЕКОРДІВ ═══");
        for (int i = 0; i < Leaderboard.Count; i++)
            Debug.Log($"  #{i + 1}  Монет: {Leaderboard[i].coinsCollected}  Час: {Leaderboard[i].timeTaken:F2}s");
    }

    // ── Приватні методи ───────────────────────────────────────────────

    private void ResetSession()
    {
        Lives         = maxLives;
        Collisions    = 0;
        CoinsCollected = 0;
        LevelTime     = 0f;
        IsGameOver    = false;
    }

    private void TriggerGameOver(string reason)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log($"[GameStore] GAME OVER — {reason}");
        OnGameOver?.Invoke();

        SaveData();
        Time.timeScale = 0f;
    }

    /// <summary>Обробник події OnGameOver — виводить повідомлення про програш.</summary>
    private void HandleGameOver()
    {
        Debug.Log("╔══════════════════════════════════════╗");
        Debug.Log("║           *** ПРОГРАШ! ***           ║");
        Debug.Log($"║  Монет зібрано : {CoinsCollected,-5}                ║");
        Debug.Log($"║  Зіткнень      : {Collisions,-5}                ║");
        Debug.Log($"║  Час гри       : {LevelTime:F2}s              ║");
        Debug.Log("╚══════════════════════════════════════╝");
    }

    private void SaveData()
    {
        var data = new GameData { leaderboard = Leaderboard };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[GameStore] Збережено → {SavePath}");
    }

    private void LoadData()
    {
        if (!File.Exists(SavePath))
        {
            Leaderboard = new List<LeaderboardEntry>();
            return;
        }
        try
        {
            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<GameData>(json);
            Leaderboard = data?.leaderboard ?? new List<LeaderboardEntry>();
            Debug.Log($"[GameStore] Завантажено. Записів у таблиці: {Leaderboard.Count}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameStore] Помилка завантаження: {e.Message}");
            Leaderboard = new List<LeaderboardEntry>();
        }
    }
}
