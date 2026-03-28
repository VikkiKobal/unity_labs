using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

void Update()
    {
        if (GameStore.Instance == null || GameStore.Instance.IsGameOver) return;
        PlayerController pc = PlayerController.Instance;
        if (pc != null && pc.IsBoosting)
            Debug.Log("BOOST: " + pc.boostTimer.ToString("F1") + "s");
    }

public void OnFinish()
    {
        Debug.Log("[GameManager] Гравець досяг фінішу!");
        GameStore.Instance?.OnLevelComplete();
        Time.timeScale = 0f;
        Debug.Log("\u2605\u2605\u2605 ПЕРЕМОГА! \u2605\u2605\u2605");
    }

public void OnRespawn()
    {
        if (GameStore.Instance != null)
            Debug.Log($"[GameManager] Респаун. Життів: {GameStore.Instance.Lives} | Монет: {GameStore.Instance.CoinsCollected}");
        else
            Debug.Log("[GameManager] Респаун.");
    }
}
