using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    public static event Action<string> OnPlayerDeath;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Invoke event FIRST, then start coroutine
            OnPlayerDeath?.Invoke("spike");
            StartCoroutine(ReloadSceneWithDelay(5f, other.gameObject));
        }
        else if (other.gameObject.CompareTag("Selectable"))
        {
            Destroy(other.gameObject);
        }
    }


    private IEnumerator ReloadSceneWithDelay(float delay, GameObject target)
    {
        // Event already invoked before coroutine started
        SoundManager.PlaySound("Death", 1, target.transform.position);
        yield return new WaitForSeconds(delay);
        ResourceManager.Instance?.FullRestoreMana();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
