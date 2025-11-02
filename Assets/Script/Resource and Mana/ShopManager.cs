using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Mana Upgrade Settings")]
    [SerializeField] private int baseManaUpgradeCost = 100;
    [SerializeField] private float manaCostMultiplier = 3f;
    [SerializeField] private int manaUpgradeAmount = 1;
    [SerializeField] private int maxManaLimit = 6;

    [Header("Selection Upgrade Settings")]
    [SerializeField] private int baseSelectionUpgradeCost = 150;
    [SerializeField] private float selectionCostMultiplier = 2f;
    [SerializeField] private int selectionUpgradeAmount = 1;
    [SerializeField] private int maxSelectionLimit = 6;

    [Header("UI References")]
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private GameObject interactPrompt;

    [SerializeField] private TMP_Text maxManaText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text manaCostText;
    [SerializeField] private TMP_Text selectionLimitText;
    [SerializeField] private TMP_Text selectionCostText;

    [SerializeField] private Button upgradeManaButton;
    [SerializeField] private Button upgradeSelectionButton;

    [Header("Player References")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private Transform playerTransform; // 🔹 drag player ke sini di Inspector

    private bool isPlayerNearby = false;
    private bool isShopOpen = false;

    private int currentManaUpgradeCost;
    private int currentSelectionUpgradeCost;
    private int manaUpgradesPurchased = 0;
    private int selectionUpgradesPurchased = 0;

    private void Start()
    {
        currentManaUpgradeCost = baseManaUpgradeCost;
        currentSelectionUpgradeCost = baseSelectionUpgradeCost;

        if (shopUIPanel != null) shopUIPanel.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);

        if (upgradeManaButton != null)
            upgradeManaButton.onClick.AddListener(BuyManaUpgrade);

        if (upgradeSelectionButton != null)
            upgradeSelectionButton.onClick.AddListener(BuySelectionUpgrade);
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }

        if (isShopOpen)
        {
            UpdateShopUI();
        }
    }

    private void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        shopUIPanel.SetActive(isShopOpen);
        interactPrompt.SetActive(!isShopOpen);

        if (playerMovementScript != null)
            playerMovementScript.enabled = !isShopOpen;

        if (isShopOpen)
            UpdateShopUI();

        // 🔹 SAVE PROGRESS setiap kali toggle UI shop
        AutoSave();
    }

    private void AutoSave()
    {
        if (SaveSystem.Instance == null || ResourceManager.Instance == null || playerTransform == null)
        {
            Debug.LogWarning("⚠️ AutoSave gagal — SaveSystem, ResourceManager, atau Player belum diassign!");
            return;
        }

        Vector3 playerPos = playerTransform.position;
        int currentMana = ResourceManager.Instance.CurrentMana;

        SaveSystem.Instance.Save(playerPos, currentMana);
        Debug.Log($"💾 Auto-saved when toggling shop (PlayerPos: {playerPos}, Mana: {currentMana})");
    }

    private void UpdateShopUI()
    {
        var rm = ResourceManager.Instance;
        var gm = GoldManager.Instance;
        if (rm == null || gm == null) return;

        maxManaText.text = $"Max Mana: {rm.MaxMana}";
        selectionLimitText.text = $"Selection Limit: {rm.SelectLimit}";
        goldText.text = $"Gold: {gm.gold}";

        // Mana upgrade UI
        if (rm.MaxMana >= maxManaLimit)
        {
            manaCostText.text = "MAXED";
            upgradeManaButton.interactable = false;
        }
        else
        {
            manaCostText.text = $"Cost: {currentManaUpgradeCost}";
            upgradeManaButton.interactable = true;
        }

        // Selection upgrade UI
        if (rm.SelectLimit >= maxSelectionLimit)
        {
            selectionCostText.text = "MAXED";
            upgradeSelectionButton.interactable = false;
        }
        else
        {
            selectionCostText.text = $"Cost: {currentSelectionUpgradeCost}";
            upgradeSelectionButton.interactable = true;
        }
    }

    private void BuyManaUpgrade()
    {
        var gm = GoldManager.Instance;
        var rm = ResourceManager.Instance;
        if (gm == null || rm == null) return;
        if (rm.MaxMana >= maxManaLimit) return;

        if (gm.SpendGold(currentManaUpgradeCost))
        {
            rm.IncreaseMaxMana(manaUpgradeAmount);
            rm.FullRestoreMana();

            manaUpgradesPurchased++;
            currentManaUpgradeCost = Mathf.RoundToInt(baseManaUpgradeCost * Mathf.Pow(manaCostMultiplier, manaUpgradesPurchased));

            UpdateShopUI();
            Debug.Log($"💧 Max Mana upgraded! New Max: {rm.MaxMana}");

            // 🔹 Save langsung setelah upgrade
            AutoSave();
        }
        else
        {
            Debug.LogWarning("❌ Not enough gold for mana upgrade!");
        }
    }

    private void BuySelectionUpgrade()
    {
        var gm = GoldManager.Instance;
        var rm = ResourceManager.Instance;
        if (gm == null || rm == null) return;
        if (rm.SelectLimit >= maxSelectionLimit) return;

        if (gm.SpendGold(currentSelectionUpgradeCost))
        {
            rm.IncreaseSelectionLimit(selectionUpgradeAmount);

            selectionUpgradesPurchased++;
            currentSelectionUpgradeCost = Mathf.RoundToInt(baseSelectionUpgradeCost * Mathf.Pow(selectionCostMultiplier, selectionUpgradesPurchased));

            UpdateShopUI();
            Debug.Log($"🎯 Selection Limit upgraded! New Limit: {rm.SelectLimit}");

            // 🔹 Save langsung setelah upgrade
            AutoSave();
        }
        else
        {
            Debug.LogWarning("❌ Not enough gold for selection limit upgrade!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (shopUIPanel != null)
                shopUIPanel.SetActive(false);
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;
            isShopOpen = false;

            // 🔹 Auto save saat keluar area shop juga
            AutoSave();
        }
    }
}
