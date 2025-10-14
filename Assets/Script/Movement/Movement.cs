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
    public Transform GroundCheckPos;
    public float GroundCheckRad = 0.2f;
    public LayerMask GroundLayer;

    [Header("Particle System Dust")]
    public ParticleSystem dustParticle; // Reference to dust PrefabParticleSystem ~ Aflah

    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        RbD.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Check if Player is on Ground
        IsGrounded = Physics2D.OverlapCircle(GroundCheckPos.position, GroundCheckRad, GroundLayer);

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

        // --- Animator ---
        Animate.SetFloat("Walking", !OnIceSlope ? Mathf.Abs(SideMove) : 0);
        Animate.SetBool("Jumping", !IsGrounded);
        Animate.SetBool("Sliding", OnIceSlope);

        Flip();
    }

    public void Move(InputAction.CallbackContext context)
    {
        SideMove = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Convert Player Inputs into Jump values
        if (context.performed)
        {
            if (IsGrounded) // Check if Player is on Ground to Jump
            {
                float jumpDirection = SideMove;

                if (OnIceSlope && jumpDirection < 0) // Ignore if Player is on Ice Slope
                    jumpDirection = 0;

                // Hold Down on Jump Button = Big Jump
                RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);

                // Play Dust Particles ~ Aflah
                dustParticle.Play();
            }
        }
        else if (context.canceled && RbD.velocity.y >= 0)
        {
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
}