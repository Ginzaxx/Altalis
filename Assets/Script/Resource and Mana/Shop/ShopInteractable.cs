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
        // Pastikan UI shop diawal dalam keadaan nonaktif
        if (shopUIPanel != null)
        {
            shopUIPanel.SetActive(false);
            shopSystem = shopUIPanel.GetComponent<ShopSystem>();
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // 🔹 Tombol Exit di UI Shop — panggil versi tanpa parameter
        if (exitButton != null)
            exitButton.onClick.AddListener(() => CloseShop(true));
    }

    private void Update()
    {
        // Tekan E untuk buka/tutup shop
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }

        // 🔹 ESC juga bisa menutup shop
        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop(true);
        }
    }

    private void ToggleShop()
    {
        if (isShopOpen)
            CloseShop(true);
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

        // Nonaktifkan movement player saat di shop
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Refresh tampilan shop
        shopSystem?.RefreshShop();

        // Simpan progres hanya saat benar-benar membuka shop
        AutoSave();
    }

    // 🔹 CloseShop punya parameter opsional agar bisa dikontrol apakah mau auto-save atau tidak
    private void CloseShop(bool doSave = true)
    {
        isShopOpen = false;

        if (shopUIPanel != null)
            shopUIPanel.SetActive(false);

        if (interactPrompt != null)
            interactPrompt.SetActive(true);

        // Aktifkan kembali kontrol player
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Hanya save jika diminta
        if (doSave)
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
            CloseShop(false); // ❌ tidak auto-save saat keluar area
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}
