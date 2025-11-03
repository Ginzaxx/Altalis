using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("Gold Settings")]
    public int gold;

    [Header("UI Reference")]
    public TMP_Text goldText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
        SaveSystem.Instance?.Save(
            GameObject.FindGameObjectWithTag("Player").transform.position,
            ResourceManager.Instance != null ? ResourceManager.Instance.CurrentMana : 0
        );
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateGoldUI();
            SaveSystem.Instance?.Save(
                GameObject.FindGameObjectWithTag("Player").transform.position,
                ResourceManager.Instance != null ? ResourceManager.Instance.CurrentMana : 0
            );
            return true;
        }
        return false;
    }

    void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + gold.ToString();
        }
    }

    public void LoadGoldFromSave(int savedGold)
    {
        gold = Mathf.Max(0, savedGold);
        UpdateGoldUI();
        Debug.Log($"📥 Loaded Gold: {gold}");
    }
}
