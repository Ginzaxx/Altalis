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

    [Header("Ice Slope")]
    public float IceSlideSpeed = 8f;
    private float IceSlideMove;
    private bool OnIceSlopeLeft = false;
    private bool OnIceSlopeRight = false;
    private bool MovementLockedLeft = false;
    private bool MovementLockedRight = false;

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
    }

    void Update()
    {
        // Only Update if Scene is enabled
        if (!enabled) return;

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
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Only process Movement input if the script is enabled
        if (!enabled) return;

        // Convert Player Inputs into Vector values
        Walking = context.ReadValue<Vector2>().x * SideSpeed;
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
        }
    }

    private IEnumerator LockLeftMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
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
    }
}