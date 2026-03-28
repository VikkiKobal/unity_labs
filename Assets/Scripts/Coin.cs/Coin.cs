using UnityEngine;

/// <summary>
/// Монета, яку гравець може зібрати.
/// Обертається для привабливості. При дотику — нараховує очко в GameStore.
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Анімація")]
    public float rotateSpeed = 120f;
    public float bobAmplitude = 0.15f;
    public float bobFrequency = 2f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Обертання
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        // Боввання вверх-вниз
        float newY = startY + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameStore.Instance?.AddCoin();
        Destroy(gameObject);
    }
}
