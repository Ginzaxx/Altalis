using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraDynamicZoomTrigger2D : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;
    public float baseSize = 5f;           // Normal camera size
    public float zoomFactor = 0.5f;       // Zoom-out per unit distance (X)
    public float maxZoomSize = 15f;       // Batas maksimum zoom
    public float zoomSpeed = 2f;          // Kecepatan zoom (smooth)

    [Header("Player Settings")]
    public string playerTag = "Player";

    private bool playerInside = false;
    private Transform player;
    private float triggerStartX;          // Titik awal saat player masuk trigger
    private float targetSize;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        targetSize = baseSize;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = true;
            player = other.transform;
            triggerStartX = player.position.x;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (playerInside && player != null)
        {
            float distancePast = player.position.x - triggerStartX;
            if (distancePast > 0)
            {
                float desiredSize = baseSize + distancePast * zoomFactor;
                targetSize = Mathf.Clamp(desiredSize, baseSize, maxZoomSize);
            }
            else
            {
                targetSize = baseSize;
            }

            targetCamera.orthographicSize = Mathf.Lerp(
                targetCamera.orthographicSize,
                targetSize,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            player = null;

            // Kembali ke ukuran awal
            StartCoroutine(ReturnToBaseSize());
        }
    }

    System.Collections.IEnumerator ReturnToBaseSize()
    {
        while (Mathf.Abs(targetCamera.orthographicSize - baseSize) > 0.01f)
        {
            targetCamera.orthographicSize = Mathf.Lerp(
                targetCamera.orthographicSize,
                baseSize,
                Time.deltaTime * zoomSpeed
            );
            yield return null;
        }
        targetCamera.orthographicSize = baseSize;
    }
}
