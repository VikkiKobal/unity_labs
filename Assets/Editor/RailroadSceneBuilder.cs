using UnityEngine;
using UnityEditor;

/// <summary>
/// Будує сцену залізничного переїзду через меню Tools → Build Railroad Crossing
/// </summary>
public class RailroadSceneBuilder : EditorWindow
{
    [MenuItem("Tools/Build Railroad Crossing")]
    public static void Build()
    {
        // ── Очищення старих об'єктів переїзду ─────────────────────────
        string[] toRemove = {
            "Ground", "Road",
            "Rail_Left", "Rail_Right",
            "Tie_Root",
            "BarrierPost", "BarrierPivot",
            "Car",
            "DetectionZone"
        };
        foreach (var name in toRemove)
        {
            var old = GameObject.Find(name);
            if (old != null) Object.DestroyImmediate(old);
        }

        // ── ЗЕМЛЯ ──────────────────────────────────────────────────────
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position   = Vector3.zero;
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        SetColor(ground, new Color(0.45f, 0.58f, 0.38f)); // приглушений оливковий

        // ── ДОРОГА ─────────────────────────────────────────────────────
        var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.position   = new Vector3(0f, 0.06f, 0f);
        road.transform.localScale = new Vector3(2f, 0.12f, 44f);
        SetColor(road, new Color(0.22f, 0.22f, 0.22f)); // темний асфальт

        // ── РЕЙКИ ──────────────────────────────────────────────────────
        CreateRail("Rail_Left",  0f, 0.18f, -0.73f, 26f, 0.12f, 0.15f);
        CreateRail("Rail_Right", 0f, 0.18f,  0.73f, 26f, 0.12f, 0.15f);

        // ── ШПАЛИ ──────────────────────────────────────────────────────
        var tieRoot = new GameObject("Tie_Root");
        tieRoot.transform.position = Vector3.zero;
        for (int i = -12; i <= 12; i += 2)
        {
            var tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tie.name = $"Tie_{i}";
            tie.transform.SetParent(tieRoot.transform);
            tie.transform.position   = new Vector3(i, 0.1f, 0f);
            tie.transform.localScale = new Vector3(0.2f, 0.1f, 2.2f);
            SetColor(tie, new Color(0.35f, 0.22f, 0.12f)); // темне дерево
        }

        // ── СТОВП ШЛАГБАУМУ ────────────────────────────────────────────
        var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = "BarrierPost";
        post.transform.position   = new Vector3(1.8f, 0.65f, -1.2f);
        post.transform.localScale = new Vector3(0.15f, 1.3f, 0.15f);
        SetColor(post, new Color(0.85f, 0.85f, 0.85f)); // світло-сірий

        // ── ШАРНІР (pivot) ШЛАГБАУМУ ───────────────────────────────────
        var pivot = new GameObject("BarrierPivot");
        pivot.transform.position = new Vector3(1.8f, 1.3f, -1.2f);

        // ── СТРІЛА ШЛАГБАУМУ ───────────────────────────────────────────
        var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "BarrierArm";
        arm.transform.SetParent(pivot.transform);
        arm.transform.localPosition = new Vector3(-2f, 0f, 0f);   // зміщення → праворуч
        arm.transform.localScale    = new Vector3(4.5f, 0.12f, 0.12f);
        // смугасте забарвлення через чергування — просто червоний
        SetColor(arm, new Color(0.75f, 0.15f, 0.15f)); // приглушений червоний

        // Додаємо скрипт шлагбауму на pivot
        pivot.AddComponent<RailroadBarrier>();

        // ── АВТОМОБІЛЬ ─────────────────────────────────────────────────
        var car = new GameObject("Car");
        car.transform.position = new Vector3(0f, 0.55f, -20f);
        car.tag = "Car";

        // Кузов
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "CarBody";
        body.transform.SetParent(car.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(1f, 0.5f, 2f);
        SetColor(body, new Color(0.18f, 0.38f, 0.62f)); // приглушений синій

        // Кабіна
        var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "CarCabin";
        cabin.transform.SetParent(car.transform);
        cabin.transform.localPosition = new Vector3(0f, 0.35f, 0.1f);
        cabin.transform.localScale    = new Vector3(0.85f, 0.4f, 1f);
        SetColor(cabin, new Color(0.18f, 0.38f, 0.62f));

        // Collider на кореневому об'єкті (для тригера)
        var carCol = car.AddComponent<BoxCollider>();
        carCol.size   = new Vector3(1f, 1f, 2f);
        carCol.center = Vector3.zero;

        // Rigidbody (для OnTrigger)
        var rb = car.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        // Додаємо CarController
        car.AddComponent<CarController>();

        // ── ЗОНА ВИЯВЛЕННЯ (тригер) ────────────────────────────────────
        var zone = new GameObject("DetectionZone");
        // Центр зони на Z=-10, розмір 8 → покриває від Z=-14 до Z=-6
        // Машина починає закривати шлагбаум за ~14 одиниць до переїзду
        zone.transform.position = new Vector3(0f, 1f, -10f);

        var zoneCol = zone.AddComponent<BoxCollider>();
        zoneCol.isTrigger = true;
        zoneCol.size      = new Vector3(3f, 3f, 8f);

        var manager = zone.AddComponent<RailroadCrossingManager>();
        manager.barrier = pivot.GetComponent<RailroadBarrier>();

        // ── КАМЕРА ─────────────────────────────────────────────────────
        var cam = Camera.main?.gameObject;
        if (cam != null)
        {
            cam.transform.position = new Vector3(9f, 9f, -14f);
            cam.transform.rotation = Quaternion.Euler(32f, -30f, 0f);
        }

        // ── ЗБЕРЕЖЕННЯ ─────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("✅ [RailroadSceneBuilder] Сцену побудовано успішно!");
        Debug.Log("   ▲/▼ — швидкість авто   |   Пробіл — шлагбаум вручну");
    }

    // ── допоміжні методи ─────────────────────────────────────────────
    private static void CreateRail(string name,
        float x, float y, float z, float sx, float sy, float sz)
    {
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.position   = new Vector3(x, y, z);
        rail.transform.localScale = new Vector3(sx, sy, sz);
        SetColor(rail, new Color(0.42f, 0.40f, 0.38f)); // сталево-сірий
    }

    private static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        // URP використовує інший шейдер і властивість _BaseColor
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard"); // fallback
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.color = color; // для Standard fallback
        renderer.sharedMaterial = mat;
    }
}
