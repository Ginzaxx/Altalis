using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance; // Singleton sederhana

    [Header("Mana Settings")]
    [SerializeField] private int startingMana = 2;
    [SerializeField] private int manaCostPerPlacement = 1;

    private int currentMana;

    public int CurrentMana => currentMana;

    private void Awake()
    {
        // Singleton pattern sederhana
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentMana = startingMana;
    }

    public bool TrySpendMana()
    {
        return SpendMana(manaCostPerPlacement);
    }

    public bool SpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            Debug.Log($"💧 Spent {amount} mana. Remaining mana: {currentMana}");
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ Not enough mana to spend {amount}");
            return false;
        }
    }

    public void AddMana(int amount)
    {
        currentMana += amount;
        Debug.Log($"➕ Added {amount} mana. Current mana: {currentMana}");
    }

    public void FullRestoreMana()
    {
        currentMana = startingMana;
        Debug.Log($"🔋 Mana restored to full: {currentMana}");
    }
}
