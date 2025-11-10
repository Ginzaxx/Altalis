using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D RbD;

    [Header("AudioSource")]
    private AudioSource audioWalk;

    [Header("Movement")]
    public float SideSpeed = 8f;
    public float SideMove;
    private bool IsFacingRight = true;

    [Header("Jumping & Gliding")]
    public float JumpPower = 8f;
    private bool IsGrounded;

    [Header("Ice Slope")]
    private bool MovementLocked = false;
    private bool OnIceSlope = false;
    public float IceSlideSpeed = 8f;
    public float IceAcceleration = 10f;
    private Vector2 slopeTangent;

    [Header("Ground Check")]
    public float GroundCheckRad = 0.2f;
    public Transform GroundCheckPos;
    public LayerMask GroundLayer;
    public bool isOnFly = false;

    [Header("Is Death?")]
    public bool IsEnablingMovement;


    [Header("Particle System Dust")]
    public ParticleSystem dustParticle;

    [Header("Death Circle Animation")]
    public GameObject DeathCircle;
    public Animator DeathCircleAnim;


    private Animator Animate;
    private MonoBehaviour MovementScript;



    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        Magma.OnPlayerDeath += DisableInputMovement;
        Spike.OnPlayerDeath += DisableInputMovement;
    }

    private void OnDisable()
    {
        Magma.OnPlayerDeath -= DisableInputMovement;
        Spike.OnPlayerDeath -= DisableInputMovement;
    }
    void Start()
    {
        MovementScript = GameObject.Find("Player").GetComponent<Movement>();
        IsEnablingMovement = true;
        RbD = GetComponent<Rigidbody2D>();
        audioWalk = GetComponent<AudioSource>();
        audioWalk.pitch = 2;
    }

    private bool wasGroundedLastFrame = false;
    void Update()
    {
        // --- Ground Check ---
        IsGrounded = Physics2D.OverlapCircle(GroundCheckPos.position, GroundCheckRad, GroundLayer);

        if (IsGrounded == true && !wasGroundedLastFrame)
        {
            isOnFly = false;
            dustParticle.Play();
            // fall from jump (landing sound)
            SoundManager.PlaySound("JumpLanding", 1, null, 1);
        }

        if (IsGrounded && Mathf.Abs(SideMove) != 0.0f)
        {
            // play walking sound
            audioWalk.enabled = true;
        }
        else
        {
            // play walking sound
            audioWalk.enabled = false;
        }

        // --- Ice Movement ---
        if (!OnIceSlope)
        {
            float moveX = (MovementLocked ? Mathf.Max(0, SideMove) : SideMove) * SideSpeed;
            RbD.velocity = new Vector2(moveX, RbD.velocity.y);
        }
        else
        {
            float moveX = Mathf.Max(0, SideMove) * SideSpeed;
            Vector2 targetVelocity = slopeTangent * (moveX + IceSlideSpeed);
            Vector2 currentVelocity = RbD.velocity;
            Vector2 newVelocity = Vector2.Lerp(currentVelocity, targetVelocity, IceAcceleration * Time.deltaTime);
            RbD.velocity = newVelocity;
        }

        Flip();

        // Save current grounded state for next frame
        wasGroundedLastFrame = IsGrounded;
    }

    void DisableInputMovement()
    {


        // 🛑 TAMBAHKAN 3 BARIS INI UNTUK MEMBEKUKAN PLAYER 🛑
        if (RbD != null)
        {
            RbD.velocity = Vector2.zero;         // 1. Hentikan semua pergerakan/geseran
            RbD.angularVelocity = 0f;          // 2. Hentikan semua rotasi
            RbD.gravityScale = 0f;             // 3. Hentikan pemain agar tidak jatuh (biarkan animasi yang mengontrol)
        }

    }

    public void Move(InputAction.CallbackContext context)
    {

        // Debug.Log("Pressing Move");
        SideMove = context.ReadValue<Vector2>().x;

    }

    public void Jump(InputAction.CallbackContext context)
    {

        // Convert Player Inputs into Jump values
        if (context.performed)
        {
            Debug.Log("Pressing Jump");
            SoundManager.PlaySound("Jump", 1, null, 1);
            if (IsGrounded) // Check if Player is on Ground to Jump
            {
                isOnFly = true;
                // Hold Down on Jump Button = Big Jump
                RbD.velocity = new Vector2(SideMove * SideSpeed, JumpPower);

                // Play Dust Particles ~ Aflah
                dustParticle.Play();
            }
        }
        else if (context.canceled && RbD.velocity.y >= 0)
        {
            isOnFly = true;
            // Light Tap on Jump Button = Small Jump
            RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
        }

    }

    private void Flip()
    {
        if ((OnIceSlope && IsFacingRight && slopeTangent.x < 0) || (OnIceSlope && !IsFacingRight && slopeTangent.x > 0) || (IsFacingRight && SideMove < 0) || (!IsFacingRight && SideMove > 0))
        {
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
            IsFacingRight = !IsFacingRight;
        }
    }

    private void OnCollisionEnter2D(Collision2D Collision)
    {
        if (Collision.collider.CompareTag("Ice"))
        {
            OnIceSlope = true;
            MovementLocked = false;
            Vector2 contactNormal = Collision.GetContact(0).normal;
            slopeTangent = new Vector2(-contactNormal.y, contactNormal.x).normalized;

            if (contactNormal.x > 0)
            {
                if (slopeTangent.x < 0) slopeTangent = -slopeTangent;
            }
            else
            {
                if (slopeTangent.x > 0) slopeTangent = -slopeTangent;
            }
            if (slopeTangent.y > 0) slopeTangent = -slopeTangent;

            Debug.Log("On Ice (Collision), Slope Tangent: " + slopeTangent + ", Normal: " + contactNormal);
        }
    }

    private void OnCollisionExit2D(Collision2D Collision)
    {
        if (Collision.collider.CompareTag("Ice"))
        {
            OnIceSlope = false;
            Debug.Log("Exit Ice (Collision)");
            StartCoroutine(LockMovement(0.5f));
        }
    }

    private IEnumerator LockMovement(float duration)
    {
        MovementLocked = true;
        yield return new WaitForSeconds(duration);
        MovementLocked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (GroundCheckPos != null)
            Gizmos.DrawSphere(GroundCheckPos.position, GroundCheckRad);
    }

    public bool GetIsFacingRight()
    {
        return IsFacingRight;
    }
}