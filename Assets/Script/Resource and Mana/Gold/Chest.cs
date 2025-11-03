using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Settings")]
    public GameObject coinPrefab;        // prefab coin yang akan keluar
    public int coinAmount = 15;          // jumlah coin yang keluar saat dibuka
    public float coinForce = 5f;         // gaya acak untuk menyebar coin
    public bool canReopen = false;       // apakah chest bisa dibuka lebih dari sekali
    public float coinPickupDelay = 0.5f; // waktu sebelum coin bisa diambil

    [Header("Animation")]
    private Animator ChestAnimation;


    [Header("UI Prompt")]
    public GameObject interactIcon;      // ikon/sprite “Press E” yang muncul di dekat player

    private bool isPlayerNear = false;
    private bool isOpened = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ChestAnimation = GetComponent<Animator>();

        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;

        // Ganti sprite jadi open
        ChestAnimation.SetTrigger("openChest");

        //play sound box open
        SoundManager.PlaySound("BoxOpen", 1, this.transform.position);

        // Spawn coins
        StartCoroutine(SpawnCoins());

        // Sembunyikan ikon interaksi secara permanen
        if (interactIcon != null)
            interactIcon.SetActive(false);


    }

    IEnumerator SpawnCoins()
    {
        for (int i = 0; i < coinAmount; i++)
        {
            Vector2 spawnPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 1f, 0f);
            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            // Tambahkan delay pickup pada coin
            StartCoroutine(DisableCoinPickupTemporarily(coin));

            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 force = new Vector2(Random.Range(-1f, 1f), 1f) * coinForce;
                rb.AddForce(force, ForceMode2D.Impulse);
            }

            yield return new WaitForSeconds(0.05f); // delay kecil antar coin keluar
        }
    }

    IEnumerator DisableCoinPickupTemporarily(GameObject coin)
    {
        Collider2D collider = coin.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            yield return new WaitForSeconds(coinPickupDelay);
            collider.enabled = true;
        }
    }

    void ResetChest()
    {
        isOpened = false;
        // if (spriteRenderer != null && closedSprite != null)
        //     spriteRenderer.sprite = closedSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;

            // Hanya tampilkan ikon jika chest belum dibuka
            if (!isOpened && interactIcon != null)
                interactIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;

            // Sembunyikan ikon saat player menjauh
            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }
}
