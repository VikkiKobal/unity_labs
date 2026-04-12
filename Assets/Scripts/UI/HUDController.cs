using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("UI елементи HUD")]
    public Text timerText;
    public Text collisionsText;
    public Text coinsText;
    public Text livesText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameStore.Instance == null) return;

        float t = GameStore.Instance.LevelTime;
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);

        if (timerText     != null) timerText.text     = string.Format("Час: {0:00}:{1:00}", minutes, seconds);
        if (collisionsText != null) collisionsText.text = "Зіткнень: " + GameStore.Instance.Collisions;
        if (coinsText     != null) coinsText.text     = "Монети: "   + GameStore.Instance.CoinsCollected;
        if (livesText     != null) livesText.text     = "Життя: "    + GameStore.Instance.Lives;
    }
}
