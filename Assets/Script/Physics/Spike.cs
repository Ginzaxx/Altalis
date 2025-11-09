using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang menabrak adalah Player atau Selectable
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            StartCoroutine(ReloadSceneWithDelay(1f, other.gameObject));
        }
        else if (other.gameObject.CompareTag("Selectable"))
        {
            Destroy(other.gameObject);
        }
    }

    private IEnumerator ReloadSceneWithDelay(float delay, GameObject target)
    {
        // give audio death
        SoundManager.PlaySound("Death", 1, target.transform.position);
        yield return new WaitForSeconds(delay);
        ResourceManager.Instance?.FullRestoreMana();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
