using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target (Player)")]
    public Transform target;

    [Header("Offset Kamera")]
    public Vector3 offset = new Vector3(0, 2, -10);

    [Header("Smooth Follow")]
    public float smoothSpeed = 0.125f;

    [Header("Manual Camera Control")]
    public float manualMoveSpeed = 5f; // kecepatan kamera saat W/S ditekan
    private float manualYOffset = 0f;  // simpan offset manual dari input
    public float maxUpOffset = 3f;     // batas kamera ke atas relatif terhadap player
    public float maxDownOffset = -2f;  // batas kamera ke bawah relatif terhadap player

    [Header("World Borders (Manual)")]
    public bool useManualBorders = true;
    public Vector2 minPosition; // batas bawah kiri
    public Vector2 maxPosition; // batas atas kanan

    [Header("World Borders (Collider)")]
    public BoxCollider2D bounds; // opsional, kalau mau otomatis

    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        Camera cam = Camera.main;

        // hitung ukuran kamera (orthographic)
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        if (!useManualBorders && bounds != null)
        {
            minBounds = bounds.bounds.min;
            maxBounds = bounds.bounds.max;
        }
    }

    void Update()
    {
        if (target == null) return;

        // --- Input manual kamera (dengan batas relatif player) ---
        if (Input.GetKey(KeyCode.W))
        {
            manualYOffset += manualMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            manualYOffset -= manualMoveSpeed * Time.deltaTime;
        }

        // Clamp manualYOffset agar tidak lebih dari batas atas/bawah relatif player
        manualYOffset = Mathf.Clamp(manualYOffset, maxDownOffset, maxUpOffset);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Posisi kamera mengikuti player + offset + manual offset Y
        Vector3 desiredPosition = target.position + offset + new Vector3(0, manualYOffset, 0);

        // Clamp posisi kamera agar tidak keluar world
        float clampX, clampY;

        if (useManualBorders)
        {
            clampX = Mathf.Clamp(desiredPosition.x, minPosition.x + camHalfWidth, maxPosition.x - camHalfWidth);
            clampY = Mathf.Clamp(desiredPosition.y, minPosition.y + camHalfHeight, maxPosition.y - camHalfHeight);
        }
        else
        {
            clampX = Mathf.Clamp(desiredPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
            clampY = Mathf.Clamp(desiredPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);
        }

        Vector3 clampedPosition = new Vector3(clampX, clampY, desiredPosition.z);

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
