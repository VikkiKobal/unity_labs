using UnityEngine;
using UnityEngine.UI;

public class LoginScreenUI : MonoBehaviour
{
    [Header("UI елементи")]
    public GameObject loginPanel;
    public InputField loginInputField;
    public Button     startButton;
    public Text       errorText;

    void Start()
    {
        Time.timeScale = 1f;
        if (loginPanel != null) loginPanel.SetActive(true);
        if (errorText  != null) errorText.text = "";
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
    }

    void OnStartClicked()
    {
        string playerName = loginInputField != null ? loginInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(playerName))
        {
            if (errorText != null) errorText.text = "Будь ласка, введіть логін!";
            return;
        }
        if (GameStore.Instance != null)
            GameStore.Instance.PlayerName = playerName;

        if (loginPanel != null) loginPanel.SetActive(false);
    }
}
