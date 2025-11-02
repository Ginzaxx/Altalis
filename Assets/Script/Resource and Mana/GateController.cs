using UnityEngine;

public class GateController : MonoBehaviour
{
    public bool isOpen = false;
    public float openHeight = 3f;
    public float openSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;

        // 🔹 Pastikan posisi awal sesuai state (terutama setelah load)
        ApplySavedState();
    }

    void Update()
    {
        // Smooth movement ke posisi target
        Vector3 target = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * openSpeed);
    }

    public void OpenGate()
    {
        isOpen = true;
    }

    public void CloseGate()
    {
        isOpen = false;
    }

    // 🔹 Dipanggil setelah restore save untuk menyesuaikan posisi gate
    public void ApplySavedState()
    {
        transform.position = isOpen ? openPosition : closedPosition;
    }
}
