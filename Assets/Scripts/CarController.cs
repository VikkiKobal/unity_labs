using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 5f;
    public float speedStep = 2f;
    public float minSpeed = 0f;
    public float maxSpeed = 20f;

    public float startZ = -20f;
    public float endZ = 25f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            speed = Mathf.Clamp(speed + speedStep, minSpeed, maxSpeed);
            Debug.Log($"[Авто] Швидкість збільшено → {speed:F1} м/с");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            speed = Mathf.Clamp(speed - speedStep, minSpeed, maxSpeed);
            Debug.Log($"[Авто] Швидкість зменшено → {speed:F1} м/с");
        }

        if (transform.position.z > endZ)
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, startZ);
            Debug.Log("[Авто] Повернення на початок дороги.");
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 260, 90), "");
        GUI.Label(new Rect(20, 18, 240, 24), $"Швидкість:  {speed:F1} м/с");
        GUI.Label(new Rect(20, 40, 240, 24), "↑  —  збільшити швидкість");
        GUI.Label(new Rect(20, 60, 240, 24), "↓  —  зменшити швидкість");
        GUI.Label(new Rect(20, 78, 240, 24), "Пробіл  —  шлагбаум вручну");
    }
}
