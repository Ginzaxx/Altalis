using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    private const string DeathCountKey = "DeathCount";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang menabrak adalah Player atau Selectable
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            StartCoroutine(ReloadSceneWithDelay(0.1f));
        }
        else if (other.gameObject.CompareTag("Selectable"))
        {
            Destroy(other.gameObject);
        }
    }

    private IEnumerator ReloadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
