// using UnityEngine;
// using UnityEngine.InputSystem;

// public class MovementAflahVersion : MonoBehaviour
// {
//     // ... (Variabel lain tetap sama) ...
//     private Rigidbody2D RbD;
//     private Animator Animate;

//     [Header("Movement")]
//     public float SideSpeed = 8f;
//     private float SideMove;
//     private float OriginalSideSpeed;

//     [Header("Jumping")]
//     public float JumpPower = 16f;
//     public float fallMultiplier = 2.5f;
//     public float lowJumpMultiplier = 2f;
//     private bool IsJumping = false;
//     private bool isJumpButtonPressed = false;

//     [Header("Crouching")]
//     public float CrouchSlowness = 4f;

//     // --- ADD THESE TWO LINES HERE ---
//     [Header("Ground Check & Coyote Time")]
//     public float coyoteTime = 0.1f; // This is the value you set in the Inspector.
//     private float coyoteTimeCounter;  // This is the script's internal timer.
//     // -----------------------------
//     public Transform groundCheckPoint;
//     public float groundCheckDistance = 0.5f;
//     private bool IsGrounded = true;

//     [Header("Ice & Slope Physics")]
//     public float slideSpeedMultiplier = 5f;
//     public float uphillDrag = 3f;
//     private bool isOnIce = false;
//     private Vector2 slopeNormalPerp;
//     private RaycastHit2D groundHit;
//     public float slideJumpVelocityThreshold = 2f;

//     public bool isLeft = false;
//     public bool isRight = false;



//     void Start()
//     {
//         RbD = GetComponent<Rigidbody2D>();
//         Animate = GetComponent<Animator>();
//         OriginalSideSpeed = SideSpeed;
//         Physics2D.queriesStartInColliders = false;

//         // --- PENAMBAHAN PENGAMAN ---
//         // Memastikan slideSpeedMultiplier tidak akan pernah negatif, meskipun salah input di Inspector.
//         if (slideSpeedMultiplier < 0)
//         {
//             slideSpeedMultiplier = Mathf.Abs(slideSpeedMultiplier);
//         }
//     }

//     void FixedUpdate()
//     {
//         // Debug.Log("IsGrounded status: " + IsGrounded);
//         if (!enabled) return;

//         CheckGround();
//         // Debug.Log(IsGrounded);

//         if (isOnIce && IsGrounded && !isJumpButtonPressed)
//         {
//             ApplyIceMovement();
//         }
//         else if (isJumpButtonPressed && isRight == true && isLeft == false && Input.GetKey(KeyCode.D))
//         {
//             ApplyNormalMovement();
//         }
//         else if (IsGrounded)
//         {
//             isLeft = false;
//             isRight = false;
//             ApplyNormalMovement();
//         }
//         // DELETE THIS ENTIRE IF/ELSE BLOCK
//         if (IsGrounded)
//         {
//             coyoteTimeCounter = coyoteTime;
//         }
//         else
//         {
//             coyoteTimeCounter -= Time.fixedDeltaTime;
//         }
//     }

//     private void CheckGround()
//     {
//         isOnIce = false;
//         IsGrounded = false;
//         // Simpan hasil raycast ke variabel groundHit
//         groundHit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));
//         Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);

//         if (groundHit.collider != null)
//         {
//             IsGrounded = true;
//             IsJumping = false;

//             slopeNormalPerp = new Vector2(groundHit.normal.y, -groundHit.normal.x);


//             // --- INILAH SOLUSI KUNCINYA ---
//             // Memaksa vektor agar SELALU menunjuk ke arah bawah (komponen Y negatif)
//             if (slopeNormalPerp.y > 0)
//             {
//                 slopeNormalPerp *= -1;
//             }
//             // --- SELESAI ---

//             if (groundHit.collider.CompareTag("Ice"))
//             {
//                 if (groundHit.collider.gameObject.name == "tileiceFaceRight")
//                 {
//                     isRight = true;
//                     isLeft = false;
//                 }
//                 isOnIce = true;
//             }
//         }
//     }

//     private void ApplyNormalMovement()
//     {
//         RbD.velocity = new Vector2(SideMove * SideSpeed, RbD.velocity.y);
//     }

//     private void ApplyIceMovement()
//     {
//         Vector2 slideForce = slopeNormalPerp * slideSpeedMultiplier;
//         RbD.AddForce(slideForce);

//         Vector2 playerForce = new Vector2(SideMove * SideSpeed, 0);

//         if (Vector2.Dot(RbD.velocity, slopeNormalPerp) > 0)
//         {
//             playerForce *= (1 / uphillDrag);
//         }

//         RbD.AddForce(playerForce);
//         RbD.velocity = Vector2.ClampMagnitude(RbD.velocity, 50f);
//     }

//     public void Jump(InputAction.CallbackContext context)
//     {

//         // STEP 1: See if this function is even being called.
//         if (context.performed)
//         {
//             // Debug.LogWarning("Jump button PRESSED!");
//         }

//         // STEP 2: Check the value of the coyote timer when you press the button.
//         // Debug.LogWarning("Current Coyote Time Counter: " + coyoteTimeCounter);
//         if (!enabled) return;

//         // Sekarang kondisi lompat hanya perlu mengecek coyote time (yang direset saat grounded)
//         // dan langsung memanggil satu fungsi lompat yang kuat dan konsisten.
//         if (context.performed && coyoteTimeCounter > 0f)
//         {
//             PerformJump();
//         }

//         if (context.performed) isJumpButtonPressed = true;
//         else if (context.canceled) isJumpButtonPressed = false;
//     }


//     private void PerformJump()
//     {
//         // INILAH LOMPATAN "HARD RESET"
//         // Secara paksa mengatur kecepatan vertikal ke JumpPower, mengabaikan seberapa cepat
//         // karakter sedang meluncur ke bawah. Ini memberikan rasa "impuls" yang spontan.
//         // Kecepatan horizontal (RbD.velocity.x) dipertahankan agar momentum tetap ada.


//         RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);






//         IsJumping = true;
//         coyoteTimeCounter = 0f; // Reset coyote time agar tidak bisa double jump di udara
//     }
//     private void BetterJump()
//     {
//         if (RbD.velocity.y < 0)
//         {
//             RbD.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
//         }
//         else if (RbD.velocity.y > 0 && !isJumpButtonPressed)
//         {
//             RbD.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
//         }
//     }

//     public void Move(InputAction.CallbackContext context)
//     {
//         SideMove = context.ReadValue<Vector2>().x;
//     }

//     public void Crouch(InputAction.CallbackContext context)
//     {
//         if (context.performed) SideSpeed = OriginalSideSpeed / CrouchSlowness;
//         else if (context.canceled) SideSpeed = OriginalSideSpeed;
//     }
// }


using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementAflahVersion : MonoBehaviour
{
    // ... (Variabel lain tetap sama) ...
    private Rigidbody2D RbD;
    private Animator Animate;

    [Header("Movement")]
    public float SideSpeed = 8f;
    private float SideMove;
    private float OriginalSideSpeed;

    [Header("Jumping")]
    public float JumpPower = 16f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private bool IsJumping = false;
    private bool isJumpButtonPressed = false;
    [Header("Jump Cooldown")]
    public float jumpCooldown = 0f; // Durasi cooldown lompat (detik), atur di Inspector
    private bool isJumpCooldownActive = false; // Flag untuk cek apakah cooldown aktif

    [Header("Crouching")]
    public float CrouchSlowness = 4f;

    [Header("Ground Check & Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.5f;
    private bool IsGrounded = true;

    [Header("Ice & Slope Physics")]
    public float slideSpeedMultiplier = 5f;
    public float uphillDrag = 3f;
    private bool isOnIce = false;
    private Vector2 slopeNormalPerp;
    private RaycastHit2D groundHit;
    public float slideJumpVelocityThreshold = 2f;

    // MODIFIKASI: Tambah variabel untuk batasi jump direction di ice
    [Header("Ice Jump Restriction")]
    public float jumpRestrictionTime = 0.3f; // Durasi batasan input setelah jump di ice (detik)
    private float jumpRestrictionTimer = 0f; // Timer internal
    private bool isJumpRestricted = false; // Flag apakah sedang dibatasi



    public bool isLeft = false;
    public bool isRight = false;

    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        OriginalSideSpeed = SideSpeed;
        Physics2D.queriesStartInColliders = false;

        if (slideSpeedMultiplier < 0)
        {
            slideSpeedMultiplier = Mathf.Abs(slideSpeedMultiplier);
        }
    }

    void FixedUpdate()
    {
        if (!enabled) return;

        CheckGround();

        // Update timer restriction (untuk ice jump restriction)
        if (isJumpRestricted)
        {
            jumpRestrictionTimer -= Time.fixedDeltaTime;
            if (jumpRestrictionTimer <= 0f)
            {
                isJumpRestricted = false;
            }
        }

        if (isOnIce && !IsGrounded && IsJumping)
        {
            ApplyIceJumpMovement();
        }
        else if (isOnIce && IsGrounded && !isJumpButtonPressed)
        {
            ApplyIceMovement();
        }
        else
        {
            isLeft = false;
            isRight = false;
            ApplyNormalMovement();
        }

        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        BetterJump();
    }

    private void CheckGround()
    {
        isOnIce = false;
        IsGrounded = false;
        groundHit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);

        if (groundHit.collider != null)
        {
            IsGrounded = true;
            IsJumping = false;

            slopeNormalPerp = new Vector2(groundHit.normal.y, -groundHit.normal.x);

            if (slopeNormalPerp.y > 0)
            {
                slopeNormalPerp *= -1;
            }

            if (groundHit.collider.CompareTag("Ice"))
            {
                if (groundHit.collider.gameObject.name == "tileiceFaceRight")
                {
                    isRight = true;
                    isLeft = false;
                }
                else
                {
                    // Asumsi jika bukan right, maka left (tambah ini untuk robustness)
                    isLeft = true;
                    isRight = false;
                }
                isOnIce = true;
            }
        }
    }

    private void ApplyNormalMovement()
    {
        RbD.velocity = new Vector2(SideMove * SideSpeed, RbD.velocity.y);
    }

    private void ApplyIceMovement()
    {
        Vector2 slideForce = slopeNormalPerp * slideSpeedMultiplier;
        RbD.AddForce(slideForce);

        Vector2 playerForce = new Vector2(SideMove * SideSpeed, 0);

        if (Vector2.Dot(RbD.velocity, slopeNormalPerp) > 0)
        {
            playerForce *= (1 / uphillDrag);
        }

        // MODIFIKASI: Tambah resistensi saat input melawan slope
        if (isJumpRestricted)
        {
            if (isRight && SideMove < 0f)
            {
                playerForce.x = Mathf.Max(playerForce.x, 0f);
                RbD.AddForce(slopeNormalPerp * slideSpeedMultiplier * 0.5f);
            }
            else if (isLeft && SideMove > 0f)
            {
                playerForce.x = Mathf.Min(playerForce.x, 0f);
                RbD.AddForce(-slopeNormalPerp * slideSpeedMultiplier * 0.5f);
            }
        }

        RbD.AddForce(playerForce);
        RbD.velocity = Vector2.ClampMagnitude(RbD.velocity, 50f);
    }


    // MODIFIKASI: Fungsi baru untuk handle movement saat lompat di ice
    private void ApplyIceJumpMovement()
    {
        // Debug untuk cek input
        Debug.Log($"Applying Ice Jump Movement: SideMove = {SideMove}, isRight = {isRight}, isLeft = {isLeft}");

        Vector2 playerForce = new Vector2(SideMove * SideSpeed, RbD.velocity.y);

        // Batasi input berdasarkan arah slope saat restriction aktif
        if (isJumpRestricted)
        {
            if (isRight && SideMove < 0f) // Coba ke kiri di slope kanan
            {
                playerForce.x = Mathf.Max(playerForce.x, 0f); // Tidak izinkan ke kiri
                RbD.AddForce(slopeNormalPerp * slideSpeedMultiplier * 0.5f); // Dorong ke arah slope
            }
            else if (isLeft && SideMove > 0f) // Coba ke kanan di slope kiri
            {
                playerForce.x = Mathf.Min(playerForce.x, 0f); // Tidak izinkan ke kanan
                RbD.AddForce(-slopeNormalPerp * slideSpeedMultiplier * 0.5f);
            }
        }

        // Terapkan kecepatan horizontal
        RbD.velocity = new Vector2(playerForce.x, RbD.velocity.y);
        RbD.velocity = Vector2.ClampMagnitude(RbD.velocity, 50f);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.LogWarning($"Jump button pressed, Cooldown Active: {isJumpCooldownActive}");
        }

        if (!enabled) return;

        // MODIFIKASI: Cek apakah cooldown tidak aktif dan coyote time valid
        if (context.performed && coyoteTimeCounter > 0f && !isJumpCooldownActive)
        {
            PerformJump();
        }

        if (context.performed) isJumpButtonPressed = true;
        else if (context.canceled) isJumpButtonPressed = false;
    }

    private void PerformJump()
    {
        RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
        Debug.Log($"Jump Performed: Horizontal Velocity = {RbD.velocity.x}, Starting Cooldown for {jumpCooldown} seconds");

        IsJumping = true;
        coyoteTimeCounter = 0f;

        if (isOnIce)
        {
            isJumpRestricted = true;
            jumpRestrictionTimer = jumpRestrictionTime;
        }

        // MODIFIKASI: Mulai coroutine untuk cooldown
        StartCoroutine(JumpCooldown());
    }


    // MODIFIKASI: Coroutine untuk mengelola jump cooldown
    private IEnumerator JumpCooldown()
    {
        isJumpCooldownActive = true;
        yield return new WaitForSeconds(jumpCooldown);
        isJumpCooldownActive = false;
        Debug.Log("Jump Cooldown Finished, Jump Allowed Again");
    }
    private void BetterJump()
    {
        // if (RbD.velocity.y < 0)
        // {
        //     RbD.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        // }
        // else if (RbD.velocity.y > 0 && !isJumpButtonPressed)
        // {
        //     RbD.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        // }
    }

    public void Move(InputAction.CallbackContext context)
    {
        SideMove = context.ReadValue<Vector2>().x;
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed) SideSpeed = OriginalSideSpeed / CrouchSlowness;
        else if (context.canceled) SideSpeed = OriginalSideSpeed;
    }
}