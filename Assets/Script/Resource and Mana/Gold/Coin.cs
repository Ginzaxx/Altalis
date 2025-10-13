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
                GoldManager.Instance.AddGold(goldValue);
            }

            Destroy(gameObject);
        }
    }
}
