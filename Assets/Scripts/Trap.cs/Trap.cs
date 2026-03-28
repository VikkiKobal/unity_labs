using UnityEngine;

/// <summary>
/// Пастка — при дотику гравця зменшує кількість життів у GameStore
/// і відправляє гравця на респаун.
/// </summary>
public class Trap : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Спочатку знімаємо життя, потім — респаун
        if (GameStore.Instance != null && !GameStore.Instance.IsGameOver)
        {
            GameStore.Instance.LoseLife();
        }

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && (GameStore.Instance == null || !GameStore.Instance.IsGameOver))
            pc.Respawn(loseLife: false); // Життя вже знято вище
    }
}
