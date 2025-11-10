using UnityEngine;

public class LeverController : MonoBehaviour
{
    public GateController gate;               // Referensi ke pintu
    public GameObject interactIcon;           // Sprite "Press E" yang muncul saat dekat
    public KeyCode interactKey = KeyCode.E;   // Tombol untuk berinteraksi
    public bool isActivated = false;          // Status lever

    private bool isPlayerNear = false;

    void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            SoundManager.PlaySound("Lever", 0.8f);
            ToggleLever();
        }
    }

    void ToggleLever()
    {
        isActivated = !isActivated;

        if (isActivated)
        {
            gate.OpenGate();
            Debug.Log("Gate opened!");
        }
        else
        {
            gate.CloseGate();
            Debug.Log("Gate closed!");
        }

        // TODO: Tambahkan animasi perubahan lever jika ada
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactIcon != null)
                interactIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }

    public void ApplySavedState()
    {
        // dan kontrol gate langsung jika ingin sinkron
        if (gate != null)
        {
            if (isActivated) gate.OpenGate();
            else gate.CloseGate();
        }
    }
}
