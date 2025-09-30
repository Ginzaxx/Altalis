using UnityEngine;
using System.Collections;

public class SpikeStompActivator : MonoBehaviour
{
    public GameObject spikePrefab; // Prefab duri yang akan di-spawn
    public float spikeDelay = 0.7f; // Jeda sebelum duri muncul (dalam detik)
    private GameObject spawnedSpike; // Referensi duri yang di-spawn
    private Coroutine spikeCoroutine; // Referensi ke coroutine untuk membatalkan jika perlu

    // Deteksi ketika pemain masuk ke collider blok
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && spawnedSpike == null)
        {
            // Memulai coroutine untuk mengaktifkan duri setelah jeda
            spikeCoroutine = StartCoroutine(ActivateSpikeWithDelay());
        }
    }

    IEnumerator ActivateSpikeWithDelay()
    {
        // Tunggu selama waktu jeda (0.7 detik)
        yield return new WaitForSeconds(spikeDelay);

        // Pastikan duri belum di-spawn dan pemain masih di atas blok
        if (spawnedSpike == null)
        {
            // Spawn duri di posisi tertentu di atas blok
            Vector3 spawnPosition = transform.position + new Vector3(0, 0.4f, 0); // Sesuaikan offset
            spawnedSpike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(0.3f);
            Destroy(spawnedSpike);

        }
    }
}