using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance { get; private set; }

    [Header("UI елементи")]
    public Transform  rowContainer;
    public GameObject rowPrefab;
    public Text       emptyText;

    void Awake()
    {
        Instance = this;
    }

    public void Refresh()
    {
        // Очистити старі рядки
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        if (GameStore.Instance == null) return;

        var board = GameStore.Instance.Leaderboard;

        if (emptyText != null)
            emptyText.gameObject.SetActive(board.Count == 0);

        for (int i = 0; i < board.Count; i++)
        {
            var entry = board[i];
            GameObject row = Instantiate(rowPrefab, rowContainer);

            // Шукаємо дочірні Text за іменами: RankText, NameText, TimeText, CoinsText, CollisionsText
            SetChildText(row, "RankText",       "#" + (i + 1));
            SetChildText(row, "NameText",       entry.playerName);
            SetChildText(row, "TimeText",       FormatTime(entry.timeTaken));
            SetChildText(row, "CoinsText",      "Монет: " + entry.coinsCollected);
            SetChildText(row, "CollisionsText", "Зіткнень: " + entry.collisions);
        }
    }

    private void SetChildText(GameObject parent, string childName, string value)
    {
        Transform t = parent.transform.Find(childName);
        if (t != null)
        {
            Text txt = t.GetComponent<Text>();
            if (txt != null) txt.text = value;
        }
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", m, s);
    }
}
