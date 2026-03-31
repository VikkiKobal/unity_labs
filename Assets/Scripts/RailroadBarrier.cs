using UnityEngine;

/// <summary>
/// Шлагбаум залізничного переїзду.
/// Піднімається (Open) та опускається (Close) плавно через обертання навколо осі Z.
/// </summary>
public class RailroadBarrier : MonoBehaviour
{
    [Header("Кути обертання (вісь Z)")]
    public float closedAngle = 0f;      // горизонтально — дорогу перекрито
    public float openAngle   = -85f;    // вертикально — дорогу відкрито

    [Header("Швидкість обертання (°/с)")]
    public float rotationSpeed = 90f;

    private float targetAngle;
    private bool  isOpen = false;

    void Start()
    {
        // Шлагбаум за замовчуванням відкритий — дорога вільна
        targetAngle = openAngle;
        SetAngle(openAngle);
    }

    void Update()
    {
        float current = NormalizeAngle(transform.localEulerAngles.z);
        float next    = Mathf.MoveTowards(current, targetAngle, rotationSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0f, 0f, next);
    }

    /// <summary>Відкрити шлагбаум (підняти)</summary>
    public void Open()
    {
        isOpen      = true;
        targetAngle = openAngle;
        Debug.Log("[Шлагбаум] Відкривається…");
    }

    /// <summary>Закрити шлагбаум (опустити)</summary>
    public void Close()
    {
        isOpen      = false;
        targetAngle = closedAngle;
        Debug.Log("[Шлагбаум] Закривається…");
    }

    public bool IsOpen => isOpen;

    // ── утиліти ──────────────────────────────────────────────
    private void SetAngle(float angle)
        => transform.localEulerAngles = new Vector3(0f, 0f, angle);

    private float NormalizeAngle(float a)
        => a > 180f ? a - 360f : a;
}
