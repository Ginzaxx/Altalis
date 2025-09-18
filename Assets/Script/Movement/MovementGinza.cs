using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D RbD;
    private Animator Animate;

    [Header("Movement")]
    public float SideSpeed = 8f;
    private float SideMove;
    private float OriginalSideSpeed;

    [Header("Jumping")]
    public float JumpPower = 6f;
    private bool IsGrounded = true;

    [Header("Ice Slope")]
    private bool OnIceSlope = false;
    private bool MovementLocked = false;
    public float IceSlideSpeed = 8f; // 🔥 Kecepatan licin di slope

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        OriginalSideSpeed = SideSpeed;
    }

    void Update()
    {
        if (!enabled) return;

        // ✅ Cek grounded pakai OverlapCircle
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Movement
        if (!OnIceSlope)
        {
            // Kalau movement dikunci → abaikan input kiri
            float moveX = (MovementLocked ? Mathf.Max(0, SideMove) : SideMove) * SideSpeed;
            RbD.velocity = new Vector2(moveX, RbD.velocity.y);
        }
        else
        {
            // ❄️ Di IceSlope → selalu geser ke kanan dengan IceSlideSpeed
            float moveX = Mathf.Max(0, SideMove) * SideSpeed; 
            float finalX = moveX + IceSlideSpeed; // gabungan input + licin
            RbD.velocity = new Vector2(finalX, RbD.velocity.y);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!enabled) return;
        SideMove = context.ReadValue<Vector2>().x;

        // Abaikan input kiri saat di slope atau movement lock
        if ((OnIceSlope || MovementLocked) && SideMove < 0)
            SideMove = 0;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!enabled) return;

        if (context.performed)
        {
            if (IsGrounded || OnIceSlope)
            {
                float jumpDirection = SideMove;

                if (OnIceSlope && jumpDirection < 0)
                    jumpDirection = 0; // Abaikan loncat kiri di slope

                // 🚀 Jump tetap normal, tidak dipengaruhi licin
                RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);

                IsGrounded = false;
            }
        }
        else if (context.canceled && RbD.velocity.y >= 0)
        {
            // Light tap = small jump
            RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ice"))
        {
            OnIceSlope = true;
            MovementLocked = false;
            Debug.Log("On Ice (Collision)");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ice"))
        {
            OnIceSlope = false;
            Debug.Log("Exit Ice (Collision)");

            // Lock kiri selama 2 detik setelah keluar dari es
            StartCoroutine(LockLeftMovement(2f));
        }
    }

    private IEnumerator LockLeftMovement(float duration)
    {
        MovementLocked = true;
        yield return new WaitForSeconds(duration);
        MovementLocked = false;
    }
}
