using UnityEngine;

public class PlayerSpriteSwitcher : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Mana Sprites")]
    [SerializeField] private Sprite mana0Sprite;
    [SerializeField] private Sprite mana1Sprite;
    [SerializeField] private Sprite mana2Sprite;

    private void Update()
    {
        if (ResourceManager.Instance == null) return;

        int mana = ResourceManager.Instance.CurrentMana;

        // Ganti sprite berdasarkan jumlah mana
        switch (mana)
        {
            case 0:
                spriteRenderer.sprite = mana0Sprite;
                break;
            case 1:
                spriteRenderer.sprite = mana1Sprite;
                break;
            case 2:
                spriteRenderer.sprite = mana2Sprite;
                break;
            default:
                // Kalau lebih dari 2, pakai sprite terakhir (mana2Sprite)
                spriteRenderer.sprite = mana2Sprite;
                break;
        }
    }
}
