using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
            GameManager.Instance?.OnFinish();
    }
}
