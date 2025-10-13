using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // untuk reload scene

public class Magma : MonoBehaviour
{
    private const string DeathCountKey = "DeathCount";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Hancurkan player
            Destroy(other.gameObject);

            // Reload scene setelah 0.5 detik (bisa langsung kalau mau instant)
            StartCoroutine(ReloadSceneWithDelay(0.1f));
        }
    }

    private IEnumerator ReloadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
