using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public HealthUI healthUI;
    public int health = 3;
    public float respawnDelay = 1f;

    public void TakeDamage(int amount)
    {
        health -= amount;
        healthUI.LoseHealth(amount);
        Debug.Log("Player took damage! Remaining lives: " + health);

        if (health <= 0)
        {
            Debug.Log("Player Died!");
            StartCoroutine(ReloadSceneWithDelay(respawnDelay));
        }
    }

    private IEnumerator ReloadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResourceManager.Instance?.FullRestoreMana();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
