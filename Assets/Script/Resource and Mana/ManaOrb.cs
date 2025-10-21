using UnityEngine;

public class ManaOrb : MonoBehaviour
{
    [SerializeField] private GameObject orbVisual; // child visual (sprite / particle)
    private Collider2D orbCollider;
    private ParticleSystem ManaOrbPieces;


    private void Awake()
    {
        Transform parentTransform = GameObject.Find("DontActivate").transform;

        if (parentTransform != null)
        {
            Transform childParent = parentTransform.GetChild(0);
            ManaOrbPieces = childParent.GetComponent<ParticleSystem>();
        }


        orbCollider = GetComponent<Collider2D>();

        if (orbCollider == null)
        {
            orbCollider = gameObject.AddComponent<CircleCollider2D>();
            orbCollider.isTrigger = true;
        }

        if (orbVisual == null)
            orbVisual = this.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {


            if (ResourceManager.Instance != null)
            {
                // 🔥 Cek apakah mana sudah penuh
                if (ResourceManager.Instance.CurrentMana >= 2)
                {
                    Debug.Log("⚠️ Mana sudah penuh, orb tidak bisa diambil.");
                    return; // orb tetap ada, tidak hilang
                }

                // Tambahkan mana
                ResourceManager.Instance.AddMana(1);
                // Instantiate the Particle System let the object handle it self.
                ParticleSystem dd = Instantiate(ManaOrbPieces, transform.position, Quaternion.identity);

                dd.gameObject.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}
