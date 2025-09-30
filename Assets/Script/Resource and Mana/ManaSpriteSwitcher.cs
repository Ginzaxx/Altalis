using UnityEngine;

public class ManaSpriteSwitcher : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer WandSprite;
    
    [Header("Mana Sprites")]
    [SerializeField] private Color ManaColor0;
    [SerializeField] private Color ManaColor1;
    [SerializeField] private Color ManaColor2;

    private void Update()
    {
        if (ResourceManager.Instance == null) return;

        int mana = ResourceManager.Instance.CurrentMana;

        // Ganti sprite berdasarkan jumlah mana
        switch (mana)
        {
            case 0:
                WandSprite.color = ManaColor0;
                break;
            case 1:
                WandSprite.color = ManaColor1;
                break;
            case 2:
                WandSprite.color = ManaColor2;
                break;
        }
    }
}
