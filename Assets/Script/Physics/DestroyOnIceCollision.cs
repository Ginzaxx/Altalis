using UnityEngine;

public class DestroyOnIceCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Jika object yang ditabrak memiliki tag "Ice"
        if (collision.gameObject.CompareTag("Ice"))
        {
            // Hancurkan diri sendiri
            Destroy(gameObject);
        }
    }
}
