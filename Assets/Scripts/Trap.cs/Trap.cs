using UnityEngine;

public class Trap : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameStore.Instance != null && !GameStore.Instance.IsGameOver)
            GameStore.Instance.LoseLife();

        // Activate hit flicker animation on the player
        other.GetComponent<PlayerAnimatorBridge>()?.TriggerHit();

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && (GameStore.Instance == null || !GameStore.Instance.IsGameOver))
            pc.Respawn(loseLife: false);
    }
}
