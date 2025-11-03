using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // untuk reload scene

public class Magma : MonoBehaviour
{
    private void Start()
    {
        SoundManager.PlayAmbience("Magma", transform.position);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.gameObject.CompareTag("Player"))
        {
            // Hancurkan player
            Destroy(target.gameObject);


            // Reload scene setelah 0.5 detik (bisa langsung kalau mau instant)
            StartCoroutine(ReloadSceneWithDelay(1f, target.gameObject));
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
