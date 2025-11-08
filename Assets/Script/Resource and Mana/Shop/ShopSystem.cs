using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopSystem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private List<ShopItemData> shopItems = new List<ShopItemData>();
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Detail Panel References")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private TMP_Text itemCostText;
    [SerializeField] private Button upgradeButton;

    [Header("Shop UI References")]
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private TMP_Text goldText; // 🪙 Tambahan untuk menampilkan gold pemain

    private ShopItemData selectedItem;
    private ShopItemButton currentSelectedButton;

    private void Start()
    {
        PopulateItemList();
        ClearItemDetails();

        UpdateGoldDisplay(); // tampilkan gold di awal
    }

    private void Update()
    {
        // 🔹 Update gold text terus-menerus (bisa juga diganti event-based)
        UpdateGoldDisplay();
    }

    private void PopulateItemList()
    {
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        foreach (var item in shopItems)
        {
            var buttonObj = Instantiate(itemButtonPrefab, itemListContainer);
            var button = buttonObj.GetComponent<ShopItemButton>();
            button.Initialize(item, this);
        }
    }

    public void DisplayItemDetails(ShopItemData item)
    {
        selectedItem = item;
        var gm = GoldManager.Instance;

        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;

        int currentCost = Mathf.RoundToInt(item.baseCost * Mathf.Pow(item.costMultiplier, item.upgradeLevel));
        itemCostText.text = item.upgradeLevel >= item.maxLevel ? "MAXED" : $"Cost: {currentCost}";

        bool canAfford = (gm != null && gm.gold >= currentCost);
        upgradeButton.interactable = (item.upgradeLevel < item.maxLevel) && canAfford;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => BuyUpgrade(item));

        UpdateGoldDisplay(); // update gold ketika item dipilih
    }

    public void SetSelectedButton(ShopItemButton button)
    {
        // Matikan highlight lama
        if (currentSelectedButton != null && currentSelectedButton != button)
            currentSelectedButton.SetSelected(false);

        // Nyalakan highlight baru
        currentSelectedButton = button;
        currentSelectedButton.SetSelected(true);
    }


    private void BuyUpgrade(ShopItemData item)
    {
        var gm = GoldManager.Instance;
        var rm = ResourceManager.Instance;
        if (gm == null || rm == null) return;

        int cost = Mathf.RoundToInt(item.baseCost * Mathf.Pow(item.costMultiplier, item.upgradeLevel));
        if (gm.gold < cost)
        {
            Debug.LogWarning("❌ Not enough gold!");
            return;
        }

        gm.SpendGold(cost);
        item.upgradeLevel++;

        switch (item.upgradeType)
        {
            case ShopItemData.UpgradeType.Mana:
                rm.IncreaseMaxMana(1);
                rm.FullRestoreMana();
                break;

            case ShopItemData.UpgradeType.Selection:
                rm.IncreaseSelectionLimit(1);
                break;
        }

        Debug.Log($"✅ Upgraded {item.itemName} to Level {item.upgradeLevel}");
        DisplayItemDetails(item); 
        UpdateGoldDisplay(); // tampilkan gold baru setelah beli
    }

    private void ClearItemDetails()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "Pilih item di kiri untuk melihat detail.";
        itemCostText.text = "";
        upgradeButton.interactable = false;
    }

    public void RefreshShop()
    {
        PopulateItemList();
        if (selectedItem != null)
            DisplayItemDetails(selectedItem);
        else
            ClearItemDetails();

        UpdateGoldDisplay();
    }

    // 🔹 Tambahan fungsi baru
    private void UpdateGoldDisplay()
    {
        if (goldText != null && GoldManager.Instance != null)
        {
            goldText.text = $"Aeon Lights: {GoldManager.Instance.gold}";
        }
    }
}
