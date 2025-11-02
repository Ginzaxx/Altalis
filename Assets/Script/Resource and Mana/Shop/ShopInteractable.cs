using UnityEngine;
using UnityEngine.UI;

public class ShopInteractable : MonoBehaviour
{
    [Header("UI & Player References")]
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Button exitButton; // 🔹 Tombol Exit di UI Shop

    private bool isPlayerNearby = false;
    private bool isShopOpen = false;
    private ShopSystem shopSystem;

    private void Start()
    {
        if (shopUIPanel != null)
        {
            shopUIPanel.SetActive(false);
            shopSystem = shopUIPanel.GetComponent<ShopSystem>();
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (exitButton != null)
            exitButton.onClick.AddListener(CloseShop); // 🔹 tombol Exit
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }

        // 🔹 ESC untuk keluar juga
        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void ToggleShop()
    {
        if (isShopOpen)
            CloseShop();
        else
            OpenShop();
    }

    private void OpenShop()
    {
        isShopOpen = true;

        if (shopUIPanel != null)
            shopUIPanel.SetActive(true);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        shopSystem?.RefreshShop();
        AutoSave();
    }

    private void CloseShop()
    {
        isShopOpen = false;

        if (shopUIPanel != null)
            shopUIPanel.SetActive(false);

        if (interactPrompt != null)
            interactPrompt.SetActive(true);

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        AutoSave();
    }

    private void AutoSave()
    {
        if (SaveSystem.Instance == null || ResourceManager.Instance == null || playerTransform == null)
            return;

        Vector3 playerPos = playerTransform.position;
        int currentMana = ResourceManager.Instance.CurrentMana;

        SaveSystem.Instance.Save(playerPos, currentMana);
        Debug.Log($"💾 Auto-saved when toggling shop (PlayerPos: {playerPos}, Mana: {currentMana})");
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
            CloseShop(); // 🔹 tutup shop otomatis saat keluar area
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}
