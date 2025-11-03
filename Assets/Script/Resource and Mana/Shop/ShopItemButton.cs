using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private CanvasGroup highlightGroup; // 🔹 digunakan untuk fade highlight

    private ShopItemData itemData;
    private ShopSystem shopSystem;
    private Button button;
    private bool isSelected = false;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float scaleDuration = 0.15f;
    [SerializeField] private float selectedScale = 1.08f;

    private Vector3 originalScale;

    public void Initialize(ShopItemData data, ShopSystem system)
    {
        itemData = data;
        shopSystem = system;
        itemNameText.text = data.itemName;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        // 🔹 Inisialisasi awal
        originalScale = transform.localScale;

        if (highlightGroup != null)
        {
            highlightGroup.alpha = 0;
            highlightGroup.gameObject.SetActive(false);
        }
    }

    private void OnClick()
    {
        shopSystem.DisplayItemDetails(itemData);
        shopSystem.SetSelectedButton(this);
    }

    public void SetSelected(bool selected)
    {
        if (highlightGroup == null) return;

        isSelected = selected;

        highlightGroup.gameObject.SetActive(true);
        StopAllCoroutines();

        StartCoroutine(FadeHighlight(selected ? 1 : 0));
        StartCoroutine(AnimateScale(selected ? selectedScale : 1f));
    }

    private System.Collections.IEnumerator FadeHighlight(float targetAlpha)
    {
        float startAlpha = highlightGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            highlightGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        highlightGroup.alpha = targetAlpha;

        if (targetAlpha == 0)
            highlightGroup.gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator AnimateScale(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float time = 0f;

        while (time < scaleDuration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, time / scaleDuration);
            yield return null;
        }

        transform.localScale = endScale;
    }

    public ShopItemData GetItemData() => itemData;
}
