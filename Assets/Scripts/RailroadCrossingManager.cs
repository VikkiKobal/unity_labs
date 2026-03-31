using UnityEngine;

/// <summary>
/// Менеджер залізничного переїзду.
/// – Автоматично опускає шлагбаум, коли авто входить у зону виявлення.
/// – Піднімає шлагбаум після того, як авто виїхало.
/// – Пробіл (Space) — ручне перемикання шлагбауму.
///
/// Вимоги: GameObject повинен мати BoxCollider (isTrigger = true).
/// Автомобіль повинен мати тег "Car".
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class RailroadCrossingManager : MonoBehaviour
{
    [Header("Посилання")]
    public RailroadBarrier barrier;

    [Header("Затримка перед підняттям (сек)")]
    public float reopenDelay = 2f;

    private float reopenTimer   = 0f;
    private bool  waitingToOpen = false;

    void Start()
    {
        // Переконуємось, що collider є тригером
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void Update()
    {
        // Ручне перемикання шлагбауму клавішею Пробіл
        if (Input.GetKeyDown(KeyCode.Space) && barrier != null)
        {
            if (barrier.IsOpen) barrier.Close();
            else                barrier.Open();
        }

        // Таймер відкриття після проїзду
        if (waitingToOpen)
        {
            reopenTimer -= Time.deltaTime;
            if (reopenTimer <= 0f)
            {
                barrier?.Open();
                waitingToOpen = false;
            }
        }
    }

    // Авто увійшло в зону → опускаємо шлагбаум
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;
        Debug.Log("[Переїзд] Авто виявлено — шлагбаум опускається!");
        barrier?.Close();
        waitingToOpen = false;
    }

    // Авто виїхало із зони → плануємо підняття шлагбауму
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Car")) return;
        Debug.Log("[Переїзд] Авто проїхало — шлагбаум відкриється через " + reopenDelay + " с.");
        waitingToOpen = true;
        reopenTimer   = reopenDelay;
    }
}
