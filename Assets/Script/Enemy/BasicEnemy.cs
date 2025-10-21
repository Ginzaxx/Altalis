using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public int damageToPlayer = 1;
    public LayerMask groundLayer;

    [Header("Colliders (assign in Inspector)")]
    public Collider2D bodyCollider;
    public Collider2D groundCheckCollider;
    public Collider2D topHitCollider;

    private bool movingRight = true;
    private Rigidbody2D rb;
    private bool isDead = false;
    private float flipCooldown = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // ✅ PENTING: Set constraint dan friction
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        if (bodyCollider == null || groundCheckCollider == null || topHitCollider == null)
        {
            Debug.LogError("BasicEnemy: Missing collider assignments!");
        }
        
        Invoke(nameof(CheckInitialGround), 0.1f);
    }

    void CheckInitialGround()
    {
        if (!IsGrounded())
        {
            Flip();
        }
    }

    void Update()
    {
        if (isDead) return;

        flipCooldown -= Time.deltaTime;
        if (flipCooldown <= 0f && !IsGrounded())
        {
            Flip();
            flipCooldown = 0.2f;
        }
    }

    // ✅ UBAH dari Update ke FixedUpdate untuk physics yang lebih stabil
    void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    void Move()
    {
        float dir = movingRight ? 1 : -1;
        
        // ✅ PAKSA set velocity setiap frame (jangan biarkan physics override)
        Vector2 targetVelocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        rb.velocity = targetVelocity;
    }

    bool IsGrounded()
    {
        if (groundCheckCollider == null) return false;
        
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        Collider2D[] results = new Collider2D[1];
        int count = Physics2D.OverlapCollider(groundCheckCollider, filter, results);
        return count > 0;
    }

    void Flip()
    {
        movingRight = !movingRight;
        
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;

        // ✅ Langsung set velocity baru, jangan pakai multiplier
        float dir = movingRight ? 1 : -1;
        rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        
        // ✅ TAMBAHAN: Dorong sedikit ke belakang dulu biar lepas dari edge
        transform.position += new Vector3(dir * 0.1f, 0, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || topHitCollider == null) return;

        if (other.CompareTag("Selectable"))
        {
            Die();
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || bodyCollider == null) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
            }
        }
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        
        if (bodyCollider != null) bodyCollider.enabled = false;
        if (groundCheckCollider != null) groundCheckCollider.enabled = false;
        if (topHitCollider != null) topHitCollider.enabled = false;
        
        Destroy(gameObject, 0.3f);
    }
}