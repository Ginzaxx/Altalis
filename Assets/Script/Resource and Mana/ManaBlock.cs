using UnityEngine;

public class ManaBlock : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (ResourceManager.Instance != null)
            {
                // Isi penuh mana
                ResourceManager.Instance.FullRestoreMana();
                Debug.Log("💧 Mana fully restored via collision!");

                // Save checkpoint (pakai posisi player)
                SaveSystem.Instance.Save(
                    collision.transform.position, // player position
                    ResourceManager.Instance.CurrentMana
                );
                Debug.Log("💾 Checkpoint is saved!");
            }
        }
    }
}
