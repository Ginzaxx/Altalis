// using UnityEngine;
// using UnityEngine.InputSystem;

// public class MovementAflahVersion : MonoBehaviour
// {
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

//     [Header("Crouching")]
//     public float CrouchSlowness = 4f;

//     [Header("Ground Check")]
//     public Transform groundCheckPoint;
//     public float groundCheckDistance = 0.5f;
//     private bool IsGrounded = true;

//     [Header("Ice & Slope Physics")]
//     public float slideSpeedMultiplier = 5f; // Pengali kekuatan meluncur
//     public float uphillDrag = 3f; // Seberapa sulit untuk mendaki lereng es
//     private bool isOnIce = false;
//     private Vector2 slopeNormalPerp; // Vektor yang menunjuk ke arah lereng menurun

//     private bool isJumpButtonPressed = false;

//     void Start()
//     {
//         RbD = GetComponent<Rigidbody2D>();
//         Animate = GetComponent<Animator>();
//         OriginalSideSpeed = SideSpeed;
//         Physics2D.queriesStartInColliders = false;
//     }

//     void Update()
//     {
//         BetterJump();
//     }

//     void FixedUpdate()
//     {
//         if (!enabled) return;

//         CheckGround();

//         if (isOnIce && IsGrounded)
//         {
//             ApplyIceMovement();
//         }
//         else
//         {
//             ApplyNormalMovement();
//         }
//     }

//     private void CheckGround()
//     {
//         isOnIce = false;
//         IsGrounded = false;

//         RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground")); // Gunakan LayerMask untuk efisiensi
//         Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);

//         if (hit.collider != null)
//         {
//             IsGrounded = true;
//             IsJumping = false;

//             // Hitung vektor arah lereng (tegak lurus dari normal)
//             slopeNormalPerp = new Vector2(hit.normal.y, -hit.normal.x);

//             if (hit.collider.CompareTag("Ice"))
//             {
//                 Debug.Log("ice detect");
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
//         // 1. Gaya meluncur alami ke bawah lereng
//         Vector2 slideForce = slopeNormalPerp * slideSpeedMultiplier;
//         RbD.AddForce(slideForce);

//         // 2. Kontrol pemain yang licin (dikurangi)
//         Vector2 playerForce = new Vector2(SideMove * SideSpeed, 0);

//         // 3. Cek apakah pemain mencoba mendaki lereng
//         // Dot product akan negatif jika arah input berlawanan dengan arah lereng
//         if (Vector2.Dot(RbD.velocity, slopeNormalPerp) > 0)
//         {
//             // Menerapkan gaya hambat (drag) saat mencoba mendaki
//             playerForce *= (1 / uphillDrag);
//         }

//         RbD.AddForce(playerForce);

//         // Batasi kecepatan maksimal untuk mencegah akselerasi tak terbatas
//         RbD.velocity = Vector2.ClampMagnitude(RbD.velocity, 15f);
//     }

//     // --- FUNGSI-FUNGSI INPUT (DIPANGGIL OLEH PLAYER INPUT COMPONENT) ---

//     public void Move(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;
//         SideMove = context.ReadValue<Vector2>().x;
//     }

//     public void Jump(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;

//         if (context.performed && IsGrounded)
//         {
//             RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
//             IsJumping = true;
//         }

//         // UPDATE THE FLAG BASED ON THE CONTEXT
//         if (context.performed)
//         {
//             isJumpButtonPressed = true;
//         }
//         else if (context.canceled)
//         {
//             isJumpButtonPressed = false;
//         }
//     }

//     private void BetterJump()
//     {
//         if (RbD.velocity.y < 0) // If falling
//         {
//             RbD.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
//         }
//         // Check our boolean flag instead of the non-existent 'context'
//         else if (RbD.velocity.y > 0 && !isJumpButtonPressed) // If rising AND jump button is released
//         {
//             RbD.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
//         }
//     }


//     public void Crouch(InputAction.CallbackContext context)
//     {
//         if (!enabled) return;
//         if (context.performed)
//         {
//             SideSpeed = OriginalSideSpeed / CrouchSlowness;
//         }
//         else if (context.canceled)
//         {
//             SideSpeed = OriginalSideSpeed;
//         }
//     }
// }

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

    [Header("Crouching")]
    public float CrouchSlowness = 4f;

    // --- ADD THESE TWO LINES HERE ---
    [Header("Ground Check & Coyote Time")]
    public float coyoteTime = 0.1f; // This is the value you set in the Inspector.
    private float coyoteTimeCounter;  // This is the script's internal timer.
    // -----------------------------
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

    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        OriginalSideSpeed = SideSpeed;
        Physics2D.queriesStartInColliders = false;

        // --- PENAMBAHAN PENGAMAN ---
        // Memastikan slideSpeedMultiplier tidak akan pernah negatif, meskipun salah input di Inspector.
        if (slideSpeedMultiplier < 0)
        {
            slideSpeedMultiplier = Mathf.Abs(slideSpeedMultiplier);
        }
    }

    // ... (Sisa kode dari Update() hingga Crouch() tidak perlu diubah, biarkan seperti yang sudah ada) ...

    void Update()
    {
        BetterJump();
    }

    void FixedUpdate()
    {
        // Debug.Log("IsGrounded status: " + IsGrounded);
        if (!enabled) return;

        CheckGround();
        // Debug.Log(IsGrounded);

        if (isOnIce && IsGrounded)
        {
            ApplyIceMovement();
        }
        else
        {
            ApplyNormalMovement();
        }

        // DELETE THIS ENTIRE IF/ELSE BLOCK
        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }

    private void CheckGround()
    {
        isOnIce = false;
        IsGrounded = false;
        // Simpan hasil raycast ke variabel groundHit
        groundHit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);

        if (groundHit.collider != null)
        {
            IsGrounded = true;
            IsJumping = false;

            slopeNormalPerp = new Vector2(groundHit.normal.y, -groundHit.normal.x);

            // --- INILAH SOLUSI KUNCINYA ---
            // Memaksa vektor agar SELALU menunjuk ke arah bawah (komponen Y negatif)
            if (slopeNormalPerp.y > 0)
            {
                slopeNormalPerp *= -1;
            }
            // --- SELESAI ---

            if (groundHit.collider.CompareTag("Ice"))
            {
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

        RbD.AddForce(playerForce);
        RbD.velocity = Vector2.ClampMagnitude(RbD.velocity, 15f);
    }

    // public void Jump(InputAction.CallbackContext context)
    // {

    //     if (!enabled) return;

    //     if (context.performed && IsGrounded)
    //     {
    //         if (isOnIce)
    //         {
    //             IceJump();
    //         }
    //         else
    //         {
    //             NormalJump();
    //         }
    //     }

    //     if (context.performed) isJumpButtonPressed = true;
    //     else if (context.canceled) isJumpButtonPressed = false;
    // }

    public void Jump(InputAction.CallbackContext context)
    {

        // STEP 1: See if this function is even being called.
        if (context.performed)
        {
            Debug.LogWarning("Jump button PRESSED!");
        }

        // STEP 2: Check the value of the coyote timer when you press the button.
        Debug.LogWarning("Current Coyote Time Counter: " + coyoteTimeCounter);
        if (!enabled) return;

        // Sekarang kondisi lompat hanya perlu mengecek coyote time (yang direset saat grounded)
        // dan langsung memanggil satu fungsi lompat yang kuat dan konsisten.
        if (context.performed && coyoteTimeCounter > 0f)
        {
            PerformJump();
        }

        if (context.performed) isJumpButtonPressed = true;
        else if (context.canceled) isJumpButtonPressed = false;
    }

    // private void NormalJump()
    // {
    //     RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
    //     IsJumping = true;
    // }

    // private void IceJump()
    // {
    //     // Condition 1: Player is actively pushing against the slope's direction.
    //     bool isActivelyStruggling = SideMove != 0 && Mathf.Sign(SideMove) != Mathf.Sign(slopeNormalPerp.x);

    //     // Condition 2: Player is sliding down the slope faster than our threshold.
    //     // We use Vector2.Dot to see if the velocity is aligned with the slope's downhill direction.
    //     bool isSlidingFast = Vector2.Dot(RbD.velocity, slopeNormalPerp) > slideJumpVelocityThreshold;

    //     // We perform a momentum jump ONLY if the player is sliding fast AND not actively struggling.
    //     if (isSlidingFast && !isActivelyStruggling)
    //     {
    //         // MOMENTUM JUMP (Saat meluncur deras)
    //         // Menambahkan gaya tegak lurus dari lereng untuk menjaga momentum.
    //         RbD.AddForce(groundHit.normal * JumpPower, ForceMode2D.Impulse);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("ICE JUMP ESCAPE");
    //         // ESCAPE JUMP (Saat diam, meluncur pelan, atau sedang mendaki)
    //         // Memberikan lompatan vertikal yang kuat dan bisa diandalkan.
    //         RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
    //     }
    //     IsJumping = true;
    // }

    private void PerformJump()
    {
        // INILAH LOMPATAN "HARD RESET"
        // Secara paksa mengatur kecepatan vertikal ke JumpPower, mengabaikan seberapa cepat
        // karakter sedang meluncur ke bawah. Ini memberikan rasa "impuls" yang spontan.
        // Kecepatan horizontal (RbD.velocity.x) dipertahankan agar momentum tetap ada.
        RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);

        IsJumping = true;
        coyoteTimeCounter = 0f; // Reset coyote time agar tidak bisa double jump di udara
    }
    private void BetterJump()
    {
        if (RbD.velocity.y < 0)
        {
            RbD.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (RbD.velocity.y > 0 && !isJumpButtonPressed)
        {
            RbD.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
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