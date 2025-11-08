using UnityEngine;
using UnityEngine.Playables;

public class InteractTriggerTimeline : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector timeline;               // Timeline yang akan dijalankan
    public SpriteRenderer interactIcon;             // Sprite "Press E"
    public string playerTag = "Player";             // Tag untuk mendeteksi pemain

    [Header("Scripts to Activate After Timeline")]
    public MonoBehaviour gameModeManagerScript;     // Komponen GameModeManager
    public MonoBehaviour gridSelectionScript;       // Komponen GridSelection

    [Header("Sprites to Switch")]
    public GameObject spriteBeforeInteraction;      // Sprite/objek sebelum interaksi
    public GameObject spriteAfterInteraction;       // Sprite/objek sesudah timeline selesai

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerNear = false;
    private bool hasInteracted = false;

    private void Start()
    {
        // Nonaktifkan ikon interaksi
        if (interactIcon != null)
            interactIcon.enabled = false;

        // Nonaktifkan script target di awal
        if (gameModeManagerScript != null)
            gameModeManagerScript.enabled = false;

        if (gridSelectionScript != null)
            gridSelectionScript.enabled = false;

        // Atur sprite awal
        if (spriteBeforeInteraction != null)
            spriteBeforeInteraction.SetActive(true);

        if (spriteAfterInteraction != null)
            spriteAfterInteraction.SetActive(false);

        // Daftarkan event timeline selesai
        if (timeline != null)
            timeline.stopped += OnTimelineFinished;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !hasInteracted)
        {
            isPlayerNear = true;
            if (interactIcon != null)
                interactIcon.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = false;
            if (interactIcon != null)
                interactIcon.enabled = false;
        }
    }

    private void Update()
    {
        if (isPlayerNear && !hasInteracted && Input.GetKeyDown(interactKey))
        {
            TriggerInteraction();
        }
    }

    private void TriggerInteraction()
    {
        hasInteracted = true;

        if (interactIcon != null)
            interactIcon.enabled = false;

        // Jalankan timeline
        if (timeline != null)
        {
            timeline.Play();
            OnTimelineFinished(null);
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        // Ganti sprite setelah timeline selesai
        if (spriteBeforeInteraction != null)
            spriteBeforeInteraction.SetActive(false);

        if (spriteAfterInteraction != null)
            spriteAfterInteraction.SetActive(true);

        // ✅ Aktifkan komponen script setelah timeline selesai
        if (gameModeManagerScript != null)
            gameModeManagerScript.enabled = true;

        if (gridSelectionScript != null)
            gridSelectionScript.enabled = true;
    }

    private void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnTimelineFinished;
    }
}
