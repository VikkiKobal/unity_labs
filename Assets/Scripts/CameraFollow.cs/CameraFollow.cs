using UnityEngine;

/// <summary>
/// Плавна камера від третьої особи, що слідує за гравцем.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Ціль")]
    public Transform target;

    [Header("Позиція")]
    public Vector3 offset = new Vector3(0f, 4.5f, -8f);
    public float smoothSpeed = 10f;

    [Header("Погляд")]
    public Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + lookOffset);
    }
}
