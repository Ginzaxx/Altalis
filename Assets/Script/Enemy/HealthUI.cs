using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;

    [Header("Stats")]
    public int maxHealth = 3;
    [HideInInspector] public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    void Update()
    {
        float target = currentHealth;
        healthSlider.value = Mathf.Lerp(healthSlider.value, target, Time.deltaTime * 5f);
    }

    public void UpdateHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        healthSlider.value = currentHealth;
    }

    public void GainHealth(int amount)
    {
        UpdateHealth(currentHealth + amount);
    }

    public void LoseHealth(int amount)
    {
        UpdateHealth(currentHealth - amount);
    }
}
