using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target (Player)")]
    public Transform target;

    [Header("Offset Kamera")]
    public Vector3 offset = new Vector3(0, 2, -10);

    [Header("Smooth Follow")]
    public float smoothSpeed = 0.125f;

    [Header("Manual Camera Control")]
    public float manualMoveSpeed = 5f; 
    private float manualYOffset = 0f;  
    public float maxUpOffset = 3f;     
    public float maxDownOffset = -2f;  

    [Header("World Borders (Manual)")]
    public bool useManualBorders = true;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    [Header("World Borders (Collider)")]
    public BoxCollider2D bounds; 

    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        Camera cam = Camera.main;
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

        // Clamp manualYOffset
        manualYOffset = Mathf.Clamp(manualYOffset, maxDownOffset, maxUpOffset);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset + new Vector3(0, manualYOffset, 0);

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

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void MoveCamera(InputAction.CallbackContext context)
    {
        manualYOffset = context.ReadValue<Vector2>().y * manualMoveSpeed * Time.deltaTime;
    }
}
