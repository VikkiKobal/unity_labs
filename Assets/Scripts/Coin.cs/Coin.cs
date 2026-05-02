using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Tooltip("Тривалість анімації підбирання перед знищенням")]
    public float pickupAnimDuration = 0.45f;

    private Animator   _anim;
    private Collider   _col;
    private bool       _picked;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _col  = GetComponent<Collider>();
        // Rotation and bobbing are handled entirely by the CoinController animator
    }

    void OnTriggerEnter(Collider other)
    {
        if (_picked || !other.CompareTag("Player")) return;
        _picked = true;

        GameStore.Instance?.AddCoin();

        // Disable collider so trigger can't fire twice
        if (_col != null) _col.enabled = false;

        if (_anim != null)
        {
            _anim.SetTrigger("Pickup");
            StartCoroutine(DestroyAfter(pickupAnimDuration));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
