using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int health = 3;
    public float respawnDelay = 1f;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Player kena damage! Sisa nyawa: " + health);

        if (health <= 0)
        {
            Debug.Log("Player mati!");
            StartCoroutine(ReloadSceneWithDelay(0.1f));
        }
    }


    private IEnumerator ReloadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResourceManager.Instance?.FullRestoreMana();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
