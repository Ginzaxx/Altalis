using UnityEngine;

public class Coin : MonoBehaviour
{
    public int goldValue = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (GoldManager.Instance != null)
            {
                SoundManager.PlaySound("CoinCollect", 1f);
                GoldManager.Instance.AddGold(goldValue);
            }

            Destroy(gameObject);
        }
    }
}
