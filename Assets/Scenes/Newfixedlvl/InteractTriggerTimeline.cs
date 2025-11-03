using UnityEngine;
using UnityEngine.Playables;

public class InteractTriggerTimeline : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector timeline;               // Timeline yang akan dijalankan
    public SpriteRenderer interactIcon;             // Sprite "Press E"
    public MonoBehaviour scriptToActivate;          // Script yang akan diaktifkan setelah timeline selesai
    public string playerTag = "Player";             // Tag untuk mendeteksi pemain

    [Header("Sprites to Switch")]
    public GameObject spriteBeforeInteraction;      // Sprite/objek sebelum interaksi
    public GameObject spriteAfterInteraction;       // Sprite/objek sesudah timeline selesai

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerNear = false;
    private bool hasInteracted = false;

    private void Start()
    {
        if (interactIcon != null)
            interactIcon.enabled = false;

        if (scriptToActivate != null)
            scriptToActivate.enabled = false;

        if (spriteBeforeInteraction != null)
            spriteBeforeInteraction.SetActive(true);

        if (spriteAfterInteraction != null)
            spriteAfterInteraction.SetActive(false);

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

        // Mulai timeline
        if (timeline != null)
            timeline.Play();
        else
            OnTimelineFinished(null);
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        // Ganti sprite setelah timeline selesai
        if (spriteBeforeInteraction != null)
            spriteBeforeInteraction.SetActive(false);

        if (spriteAfterInteraction != null)
            spriteAfterInteraction.SetActive(true);

        // Aktifkan script tambahan (kalau ada)
        if (scriptToActivate != null)
            scriptToActivate.enabled = true;
    }

    private void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnTimelineFinished;
    }
}
