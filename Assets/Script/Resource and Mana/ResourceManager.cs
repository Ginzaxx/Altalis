using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Mana Settings")]
    [SerializeField] private int manaCostPerPlacement = 1;
    [SerializeField] private int startingMana = 2;
    [SerializeField] private int currentMana = 2;
    [SerializeField] private int maxMana;
    [SerializeField] public ManaUI manaUI;

    [Header("Selection Settings")]
    [SerializeField] private int selectLimit = 3;

    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public int SelectLimit => selectLimit;

    private void Awake()
    {
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

        maxMana = startingMana;
        currentMana = maxMana;
        manaUI.SetMana(currentMana, maxMana);
    }

    public bool TrySpendMana() => SpendMana(manaCostPerPlacement);

    public bool SpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;

            manaUI.SetMana(currentMana, maxMana);

            Debug.Log($"Spent {amount} mana. Remaining mana: {currentMana}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough mana to spend {amount}");
            return false;
        }
    }

    public void AddMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);

        manaUI.SetMana(currentMana, maxMana);

        Debug.Log($"Added {amount} mana. Current mana: {currentMana}/{maxMana}");
    }

    public void FullRestoreMana()
    {
        currentMana = maxMana;

        manaUI.SetMana(currentMana, maxMana);

        Debug.Log($"Mana restored to full: {currentMana}");
    }

    public void IncreaseMaxMana(int amount)
    {
        maxMana += amount;
        currentMana = maxMana;

        manaUI.SetMana(currentMana, maxMana);

        Debug.Log($"✨ Increased Max Mana by {amount}. Now Max Mana = {maxMana}");
    }

    // Increase selection limit (no max check — handled in Shop)
    public void IncreaseSelectionLimit(int amount)
    {
        selectLimit += amount;
        Debug.Log($"🎯 Increased Selection Limit! Now Select Limit = {selectLimit}");
    }

    public void LoadStatsFromSave(int savedMaxMana, int savedSelectLimit)
    {
        // Pastikan nilai valid
        if (savedMaxMana > 0)
            maxMana = savedMaxMana;

        if (savedSelectLimit > 0)
            selectLimit = savedSelectLimit;

        // Pastikan currentMana sinkron & tidak lebih besar dari max
        currentMana = Mathf.Min(currentMana, maxMana);

        manaUI.SetMana(currentMana, maxMana);

        Debug.Log($"📥 Loaded Resource Stats: MaxMana = {maxMana}, SelectLimit = {selectLimit}");
    }
}
