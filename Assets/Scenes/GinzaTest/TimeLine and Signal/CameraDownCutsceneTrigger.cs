using UnityEngine;
using UnityEngine.Playables;

public class CameraDownTimelineTrigger : MonoBehaviour
{
    [Header("Timeline yang akan dimainkan")]
    public PlayableDirector cutsceneDirector;

    [Header("Referensi CameraFollow2D")]
    public CameraFollow2D cameraFollow;

    [Header("Sensitivitas Deteksi Arah Bawah")]
    public float downThreshold = -0.5f; // seberapa jauh harus menekan ke bawah

    private bool isPlayerInside = false;
    private bool hasPlayed = false;

    void Update()
    {
        if (hasPlayed || cameraFollow == null || cutsceneDirector == null)
            return;

        // Deteksi apakah player di dalam area dan menekan ke bawah
        if (isPlayerInside && cameraFollow.currentInputY < downThreshold)
        {
            Debug.Log("🎬 Kamera digerakkan ke bawah, Timeline dimulai!");
            cutsceneDirector.Play();
            hasPlayed = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log("🟢 Player memasuki area trigger kamera");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log("🔴 Player keluar dari area trigger kamera");
        }
    }
}
