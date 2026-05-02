using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        other.GetComponent<PlayerAnimatorBridge>()?.TriggerCelebrate();
        GameManager.Instance?.OnFinish();
    }
}
