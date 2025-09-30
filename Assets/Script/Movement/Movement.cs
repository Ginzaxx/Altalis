using System.Collections;
using UnityEngine;

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
    public float IceAcceleration = 10f; // Kecepatan akselerasi saat masuk ke es
    private Vector2 slopeTangent; // Arah tangen slope untuk sliding
    
    [Header("Ground Check")]
    public Transform GroundCheckPos;
    public float GroundCheckRad = 0.2f;
    public LayerMask GroundLayer;

    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        RbD.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Cek tanah
        IsGrounded = Physics2D.OverlapCircle(GroundCheckPos.position, GroundCheckRad, GroundLayer);

        // --- Movement pakai keybinding ---
        if (Input.GetKey(KeyBindings.MoveLeftKey))
            SideMove = -1;
        else if (Input.GetKey(KeyBindings.MoveRightKey))
            SideMove = 1;
        else
            SideMove = 0;

        // --- Jump pakai keybinding ---
        if (Input.GetKeyDown(KeyBindings.JumpKey) && IsGrounded)
        {
            float jumpDirection = SideMove;
            if (OnIceSlope && jumpDirection < 0)
                jumpDirection = 0;

            RbD.velocity = new Vector2(jumpDirection * SideSpeed, JumpPower);
        }
        else if (Input.GetKeyUp(KeyBindings.JumpKey) && RbD.velocity.y > 0)
        {
            // short hop
            RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
        }

        // --- Apply movement ---
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
            StartCoroutine(LockMovement(1f));
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
