using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // untuk reload scene

public class Spike : MonoBehaviour
{
    private const string DeathCountKey = "DeathCount";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Hancurkan player
            Destroy(other.gameObject);

            // Ambil jumlah kematian dari PlayerPrefs
            int deathCount = PlayerPrefs.GetInt(DeathCountKey, 0);
            deathCount++;

            if (deathCount >= 3)
            {
                // Hapus save game
                SaveSystem.Instance.DeleteSave();

                // Reset death counter
                deathCount = 0;
            }

            // Simpan jumlah kematian terbaru
            PlayerPrefs.SetInt(DeathCountKey, deathCount);
            PlayerPrefs.Save();

            // Reload scene setelah 0.5 detik (bisa langsung kalau mau instant)
            StartCoroutine(ReloadSceneWithDelay(0.5f));
        }
    }

    private IEnumerator ReloadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
