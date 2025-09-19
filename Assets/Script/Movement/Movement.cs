// using System.Collections;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class Movement : MonoBehaviour
// {
//     private Rigidbody2D RbD;
//     private Animator Animate;

//     [Header("Movement")]
//     public float SideSpeed = 8f;
//     private float SideMove;
//     private float OriginalSideSpeed;

//     [Header("Jumping")]
//     public float JumpPower = 6f;
//     private bool IsJumping = false;
//     private bool IsGrounded = true;

//     [Header("Ice Slope")]
//     private bool OnIceSlope = false;
//     private bool MovementLocked = false;
//     public float IceSlideSpeed = 8f; // 🔥 Kecepatan licin di slope

//     [Header("Ground Check")]
//     public Transform groundCheck;
//     public float groundCheckRadius = 0.2f;
//     public LayerMask groundLayer;

//     void Start()
//     {
//         RbD = GetComponent<Rigidbody2D>();
//         Animate = GetComponent<Animator>();
//         OriginalSideSpeed = SideSpeed;
//     }

//     void Update()
//     {
//         if (!enabled) return;

//         // ✅ Cek grounded pakai OverlapCircle
//         IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

//         // Movement
//         if (!OnIceSlope)
//         {
//             // Kalau movement dikunci → abaikan input kiri
//             float moveX = (MovementLocked ? Mathf.Max(0, SideMove) : SideMove) * SideSpeed;
//             RbD.velocity = new Vector2(moveX, RbD.velocity.y);
//         }
//         else
//         {
//             // ❄️ Di IceSlope → selalu geser ke kanan dengan IceSlideSpeed
//             float moveX = Mathf.Max(0, SideMove) * SideSpeed; 
//             float finalX = moveX + IceSlideSpeed; // gabungan input + licin
//             RbD.velocity = new Vector2(finalX, RbD.velocity.y);
//         }
//     }

//     public void Move(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;
//         SideMove = context.ReadValue<Vector2>().x;

//         // Abaikan input kiri saat di slope atau movement lock
//         if ((OnIceSlope || MovementLocked) && SideMove < 0)
//             SideMove = 0;
//     }

//     public void Jump(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;

//         if (context.performed)
//         {
//             if (IsGrounded || OnIceSlope)
//             {
//                 float jumpDirection = SideMove;

//                 if (OnIceSlope && jumpDirection < 0)
//                     jumpDirection = 0; // Abaikan loncat kiri di slope

//                 // 🚀 Jump tetap normal, tidak dipengaruhi licin
//                 RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);

//                 IsGrounded = false;
//                 IsJumping = true;
//             }
//         }
//         else if (context.canceled && RbD.velocity.y >= 0)
//         {
//             // Light tap = small jump
//             RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
//         }
//     }

//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (collision.collider.CompareTag("Ice"))
//         {
//             OnIceSlope = true;
//             MovementLocked = false;
//             Debug.Log("On Ice (Collision)");
//         }
//     }

//     private void OnCollisionExit2D(Collision2D collision)
//     {
//         if (collision.collider.CompareTag("Ice"))
//         {
//             OnIceSlope = false;
//             Debug.Log("Exit Ice (Collision)");

//             // Lock kiri selama 2 detik setelah keluar dari es
//             StartCoroutine(LockLeftMovement(2f));
//         }
//     }

//     private IEnumerator LockLeftMovement(float duration)
//     {
//         MovementLocked = true;
//         yield return new WaitForSeconds(duration);
//         MovementLocked = false;
//     }
// }



// using System.Collections;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class Movement : MonoBehaviour
// {
//     private Rigidbody2D RbD;
//     private Animator Animate;

//     [Header("Movement")]
//     public float SideSpeed = 8f;
//     private float SideMove;
//     private float OriginalSideSpeed;

//     [Header("Jumping")]
//     public float JumpPower = 6f;
//     private bool IsJumping = false;
//     private bool IsGrounded = true;

//     [Header("Ice Slope")]
//     private bool OnIceSlope = false;
//     private bool MovementLocked = false;
//     public float IceSlideSpeed = 8f; // Kecepatan licin di slope
//     public float IceAcceleration = 10f; // Kecepatan akselerasi saat masuk ke es
//     private Vector2 slopeTangent; // Arah tangen slope untuk sliding

//     [Header("Ground Check")]
//     public Transform groundCheck;
//     public float groundCheckRadius = 0.2f;
//     public LayerMask groundLayer;

//     void Start()
//     {
//         RbD = GetComponent<Rigidbody2D>();
//         Animate = GetComponent<Animator>();
//         OriginalSideSpeed = SideSpeed;
//         RbD.constraints = RigidbodyConstraints2D.FreezeRotation; // Pastikan rotasi terkunci
//     }

//     void Update()
//     {
//         if (!enabled) return;

//         // Cek grounded pakai OverlapCircle
//         IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

//         // Movement
//         if (!OnIceSlope)
//         {
//             // Normal movement
//             float moveX = (MovementLocked ? Mathf.Max(0, SideMove) : SideMove) * SideSpeed;
//             RbD.velocity = new Vector2(moveX, RbD.velocity.y);
//         }
//         else
//         {
//             // Ice slope movement: slide along slope tangent
//             float moveX = Mathf.Max(0, SideMove) * SideSpeed; // Input pemain (hanya ke kanan)
//             // Target velocity: kombinasi input pemain dan kecepatan licin sepanjang slope
//             Vector2 targetVelocity = slopeTangent * (moveX + IceSlideSpeed);
//             // Interpolasi halus ke target velocity
//             Vector2 currentVelocity = new Vector2(RbD.velocity.x, RbD.velocity.y);
//             Vector2 newVelocity = Vector2.Lerp(currentVelocity, targetVelocity, IceAcceleration * Time.deltaTime);
//             RbD.velocity = newVelocity;
//         }
//     }

//     public void Move(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;
//         SideMove = context.ReadValue<Vector2>().x;

//         // Abaikan input kiri saat di slope atau movement lock
//         if ((OnIceSlope || MovementLocked) && SideMove < 0)
//             SideMove = 0;
//     }

//     public void Jump(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;

//         if (context.performed)
//         {
//             if (IsGrounded || OnIceSlope)
//             {
//                 float jumpDirection = SideMove;

//                 if (OnIceSlope && jumpDirection < 0)
//                     jumpDirection = 0; // Abaikan loncat kiri di slope

//                 // Jump tetap normal, tidak dipengaruhi licin
//                 RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);

//                 IsGrounded = false;
//                 IsJumping = true;
//             }
//         }
//         else if (context.canceled && RbD.velocity.y >= 0)
//         {
//             // Light tap = small jump
//             RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
//         }
//     }

//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (collision.collider.CompareTag("Ice"))
//         {
//             OnIceSlope = true;
//             MovementLocked = false;
//             // Hitung arah tangen slope berdasarkan normal kontak
//             Vector2 contactNormal = collision.GetContact(0).normal;
//             slopeTangent = new Vector2(-contactNormal.y, contactNormal.x).normalized; // Tangen = perpendicular ke normal
//             // Pastikan arah tangen mengarah ke bawah/kanan (sesuaikan dengan slope)
//             if (slopeTangent.x < 0) slopeTangent = -slopeTangent;
//             Debug.Log("On Ice (Collision), Slope Tangent: " + slopeTangent);
//         }
//     }

//     private void OnCollisionExit2D(Collision2D collision)
//     {
//         if (collision.collider.CompareTag("Ice"))
//         {
//             OnIceSlope = false;
//             Debug.Log("Exit Ice (Collision)");
//             // Lock kiri selama 2 detik setelah keluar dari es
//             StartCoroutine(LockLeftMovement(2f));
//         }
//     }

//     private IEnumerator LockLeftMovement(float duration)
//     {
//         MovementLocked = true;
//         yield return new WaitForSeconds(duration);
//         MovementLocked = false;
//     }
// }


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
<<<<<<< HEAD
    private float Walking;
    private bool IsFacingRight = true;

    [Header("Jumping & Gliding")]
    public float JumpPower = 8f;
    private bool IsGrounded;
    // public float GlideSpeed = 2f;
    // private bool IsJumping = false;

    // [Header("Crouching")]
    // public float CrouchPower = 4f;
    // private bool IsCrouching = false;
=======
    private float OriginalSideSpeed;

    [Header("Jumping")]
    public float JumpPower = 6f;
    private bool IsJumping = false;
    private bool IsGrounded = true;
>>>>>>> parent of 0df4bbd (Merge branch 'Hans' into Ginza)

    [Header("Ice Slope")]
    private bool OnIceSlope = false;
    private bool MovementLocked = false;
    public float IceSlideSpeed = 8f; // Kecepatan licin di slope
    public float IceAcceleration = 10f; // Kecepatan akselerasi saat masuk ke es
    private Vector2 slopeTangent; // Arah tangen slope untuk sliding

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        OriginalSideSpeed = SideSpeed;
        RbD.constraints = RigidbodyConstraints2D.FreezeRotation; // Pastikan rotasi terkunci
    }

    void Update()
    {
        if (!enabled) return;

<<<<<<< HEAD
        IceSlope();
        Flip();

        // Set Animator values
        Animate.SetFloat("SideMove", SideMove * SideMove);
        Animate.SetBool("Grounded", IsGrounded);

        // Set Rigidbody Velocity value

        // Ignore Inputs when Movement is Locked
        SideMove = ((MovementLockedLeft && SideMove < 0) || (MovementLockedRight && SideMove > 0)) ? 0 : Walking;
        RbD.velocity = new Vector2(SideMove + IceSlideMove, RbD.velocity.y);

        // Check if Player is on Ground
        IsGrounded = Physics2D.OverlapCircle(GroundCheckPos.position, GroundCheckRad, GroundLayer);
=======
        // Cek grounded pakai OverlapCircle
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

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
>>>>>>> parent of 0df4bbd (Merge branch 'Hans' into Ginza)
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!enabled) return;
<<<<<<< HEAD

        // Convert Player Inputs into Vector values
        Walking = context.ReadValue<Vector2>().x * SideSpeed;
=======
        SideMove = context.ReadValue<Vector2>().x;

        // Abaikan input kiri saat di slope atau movement lock
        if ((OnIceSlope || MovementLocked) && SideMove < 0)
            SideMove = 0;
>>>>>>> parent of 0df4bbd (Merge branch 'Hans' into Ginza)
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!enabled) return;

        if (context.performed)
        {
            if (IsGrounded || OnIceSlope)
            {
<<<<<<< HEAD
                // Hold Down on Jump Button = Big Jump
                RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
                // IsJumping = true;
                // }
                // else if (IsJumping) 
                // {
                // Double tap on Jump Button = Glide
                // RbD.velocity = new Vector2(RbD.velocity.x, -GlideSpeed);
                // RbD.gravityScale = 0;
                // IsJumping = false;
=======
                float jumpDirection = SideMove;

                if (OnIceSlope && jumpDirection < 0)
                    jumpDirection = 0; // Abaikan loncat kiri di slope

                // Jump tetap normal, tidak dipengaruhi licin
                RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);

                IsGrounded = false;
                IsJumping = true;
>>>>>>> parent of 0df4bbd (Merge branch 'Hans' into Ginza)
            }
        }
        else if (context.canceled && RbD.velocity.y >= 0)
        {
            // Light tap = small jump
            RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
        }
    }

<<<<<<< HEAD
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

    private void IceSlope()
    {
        if (OnIceSlopeLeft)
            IceSlideMove = IceSlideSpeed;
        if (OnIceSlopeRight)
            IceSlideMove = -IceSlideSpeed;
        if (!OnIceSlopeLeft && !OnIceSlopeRight)
            IceSlideMove = 0;
    }
    
    private void OnCollisionEnter2D(Collision2D IceBox)
    {
        if (IceBox.collider.CompareTag("IceLeft"))
        {
            MovementLockedLeft = true;
            OnIceSlopeLeft = true;
            Debug.Log("On Ice Left (IceBox)");
        }
        if (IceBox.collider.CompareTag("IceRight"))
        {
            MovementLockedRight = true;
            OnIceSlopeRight = true;
            Debug.Log("On Ice Right (IceBox)");
        }
    }
    private void OnCollisionExit2D(Collision2D IceBox)
    {
        if (IceBox.collider.CompareTag("IceLeft"))
        {
            OnIceSlopeLeft = false;
            Debug.Log("Exit Ice Left (IceBox)");

            // Lock kiri selama 2 detik setelah keluar dari es
            StartCoroutine(LockLeftMovement(1f));
        }
        if (IceBox.collider.CompareTag("IceRight"))
        {
            OnIceSlopeRight = false;
            Debug.Log("Exit Ice Right (IceBox)");

            // Lock kiri selama 2 detik setelah keluar dari es
            StartCoroutine(LockRightMovement(1f));
=======
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
            StartCoroutine(LockLeftMovement(2f));
>>>>>>> parent of 0df4bbd (Merge branch 'Hans' into Ginza)
        }
    }

    private IEnumerator LockLeftMovement(float duration)
    {
        MovementLocked = true;
        yield return new WaitForSeconds(duration);
<<<<<<< HEAD
        MovementLockedLeft = false;
        Debug.Log("Left Movement Unlocked");
    }
    private IEnumerator LockRightMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        MovementLockedRight = false;
        Debug.Log("Right Movement Unlocked");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GroundCheckPos.position, GroundCheckRad);
=======
        MovementLocked = false;
>>>>>>> parent of 0df4bbd (Merge branch 'Hans' into Ginza)
    }
}