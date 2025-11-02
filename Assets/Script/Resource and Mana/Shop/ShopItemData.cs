using UnityEngine;

[System.Serializable]
public class ShopItemData
{
    public string itemName;
    [TextArea] public string description;
    public int baseCost = 100;
    public float costMultiplier = 2f;
    public int upgradeLevel = 0;
    public int maxLevel = 5;

    public enum UpgradeType { Mana, Selection }
    public UpgradeType upgradeType;
}
