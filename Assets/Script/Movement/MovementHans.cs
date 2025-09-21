using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementHans : MonoBehaviour
{
    private Rigidbody2D RbD;
    private Animator Animate;

    [Header("Movement")]
    public float SideSpeed = 8f;
    private float SideMove;
    private bool IsFacingRight = true;

    [Header("Jumping & Gliding")]
    public float JumpPower = 8f;
    private bool IsGrounded;
    // public float GlideSpeed = 2f;
    // private bool IsJumping = false;

    // [Header("Crouching")]
    // public float CrouchPower = 4f;
    // private bool IsCrouching = false;

    [Header("Ice Slope")]
    public float IceSlideSpeed = 8f;
    private bool OnIceSlope = false;
    private bool MovementLocked = false;
    public float IceAcceleration = 10f; // Kecepatan akselerasi saat masuk ke es
    private Vector2 slopeTangent; // Arah tangen slope untuk sliding
    
    [Header("Groundcheck")]
    public Transform GroundCheckPos;
    // public Vector2 GroundCheckSize = new Vector2(0.9f, 0.1f);
    public float GroundCheckRad = 0.2f;
    public LayerMask GroundLayer;

    void Start()
    {
        // Get Rigidbody and Animator values
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        RbD.constraints = RigidbodyConstraints2D.FreezeRotation; // Pastikan rotasi terkunci
    }

    void Update()
    {
        // Only Update if Scene is enabled
        if (!enabled) return;

        // Check if Player is on Ground
        IsGrounded = Physics2D.OverlapCircle(GroundCheckPos.position, GroundCheckRad, GroundLayer);

        // Movement
        if (!OnIceSlope)
        {
            // Normal movement
            float moveX = (MovementLocked ? Mathf.Max(0, SideMove) : SideMove) * SideSpeed;
            RbD.velocity = new Vector2(moveX, RbD.velocity.y);
        }
        else
        {
            // Ice slope movement: slide along slope tangent
            float moveX = Mathf.Max(0, SideMove) * SideSpeed; // Input pemain (hanya ke kanan)
            // Target velocity: kombinasi input pemain dan kecepatan licin sepanjang slope
            Vector2 targetVelocity = slopeTangent * (moveX + IceSlideSpeed);
            // Interpolasi halus ke target velocity
            Vector2 currentVelocity = new Vector2(RbD.velocity.x, RbD.velocity.y);
            Vector2 newVelocity = Vector2.Lerp(currentVelocity, targetVelocity, IceAcceleration * Time.deltaTime);
            RbD.velocity = newVelocity;
        }

        // Set Animator values
        Animate.SetFloat("SideMove", SideMove * SideMove);
        Animate.SetBool("Grounded", IsGrounded);
        Flip();
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Only process Movement input if the script is enabled
        if (!enabled) return;
        SideMove = context.ReadValue<Vector2>().x;

        // Abaikan input kiri saat di slope atau movement lock
        if ((OnIceSlope || MovementLocked) && SideMove < 0)
            SideMove = 0;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Only process Jump input if the script is enabled
        if (!enabled) return;

        // Convert Player Inputs into Jump or Glide values
        if (context.performed)
        {
            if (IsGrounded) // Check if Player is on Ground to Jump
            {
                float jumpDirection = SideMove;

                if (OnIceSlope && jumpDirection < 0)
                    jumpDirection = 0; // Abaikan loncat kiri di slope

                // Jump tetap normal, tidak dipengaruhi licin
                // Hold Down on Jump Button = Big Jump
                RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);

                // IsJumping = true;
                // }
                // else if (IsJumping) 
                // {
                // Double tap on Jump Button = Glide
                // RbD.velocity = new Vector2(RbD.velocity.x, -GlideSpeed);
                // RbD.gravityScale = 0;
                // IsJumping = false;
            }
        }
        else if (context.canceled && RbD.velocity.y >= 0)
        {
            // Light tap on Jump Button = Small Jump
            RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
        }
    }

    // public void Crouch(InputAction.CallbackContext context) 
    // {
    // Only process crouch input if the script is enabled
    // if (!enabled) return;
    // Convert Player Inputs into Crouch Slowness
    // if (context.performed && !IsCrouching) 
    // {
    // IsCrouching = true;
    // SideSpeed /= CrouchPower;
    // }
    // else if (context.canceled && IsCrouching)
    // {
    // IsCrouching = false;
    // SideSpeed *= CrouchPower;
    // }
    // }

    private void Flip()
    {
        if (IsFacingRight && SideMove < 0 || !IsFacingRight && SideMove > 0)
        {
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
            IsFacingRight = !IsFacingRight;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ice"))
        {
            OnIceSlope = true;
            MovementLocked = false;
            // Hitung arah tangen slope berdasarkan normal kontak
            Vector2 contactNormal = collision.GetContact(0).normal;
            slopeTangent = new Vector2(-contactNormal.y, contactNormal.x).normalized;
            // Pastikan tangen mengarah ke bawah (y negatif) dan sesuai arah slope
            if (contactNormal.x > 0) // Rightward slope (normal points right/up, slide down-right)
            {
                if (slopeTangent.x < 0) slopeTangent = -slopeTangent;
            }
            else // Leftward slope (normal points left/up, slide down-left)
            {
                if (slopeTangent.x > 0) slopeTangent = -slopeTangent;
            }
            // Final check: Ensure downhill (y < 0)
            if (slopeTangent.y > 0) slopeTangent = -slopeTangent;
            Debug.Log("On Ice (Collision), Slope Tangent: " + slopeTangent + ", Normal: " + contactNormal);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ice"))
        {
            OnIceSlope = false;
            Debug.Log("Exit Ice (Collision)");
            // Lock kiri selama 2 detik setelah keluar dari es
            StartCoroutine(LockMovement(2f));
        }
    }

    private IEnumerator LockMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        MovementLocked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GroundCheckPos.position, GroundCheckRad);
    }
}