using System.Collections;
using System.Collections.Generic;
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
    public float GlideSpeed = -0.5f;
    private bool IsJumping = false;
    private bool IsGrounded = true;

    [Header("Crouching")]
    public float CrouchSlowness = 4f;
    private bool IsCrouching = false;

    void Start()
    {
        // Get Rigidbody and Animator values
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();

        // Set original speed
        OriginalSideSpeed = SideSpeed;
    }

    void Update()
    {
        // Only move if movement is enabled and we're in movement mode
        if (!enabled) return;

        // Set Grounded when not falling
        if (RbD.velocity.y >= -0.05 && RbD.velocity.y <= 0.05)
        {
            RbD.gravityScale = 1;
            IsGrounded = true;
        }

        // Set Rigidbody Velocity value
        RbD.velocity = new Vector2(SideMove * SideSpeed, RbD.velocity.y);
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Only process movement input if the script is enabled
        if (!enabled) return;

        // Convert Player Inputs into Vector values
        SideMove = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Only process jump input if the script is enabled
        if (!enabled) return;

        // Convert Player Inputs into Jump or Glide values
        if (context.performed)
        {
            if (IsGrounded) // Check if Player is on Ground to Jump
            {
                // Hold Down on Jump Button = Big Jump
                RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
                IsGrounded = false;
                IsJumping = true;
            }
            else if (IsJumping) 
            {
                // Double tap on Jump Button = Glide
                RbD.velocity = new Vector2(RbD.velocity.x, GlideSpeed);
                RbD.gravityScale = 0;
                IsJumping = false;
            }
        }
        else if (context.canceled && RbD.velocity.y >= 0)
        {
            // Light tap on Jump Button = Small Jump
            RbD.velocity = new Vector2(RbD.velocity.x, RbD.velocity.y * 0.5f);
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        // Only process crouch input if the script is enabled
        if (!enabled) return;

        // Convert Player Inputs into Crouch Slowness
        if (context.performed)
        {
            IsCrouching = true;
            SideSpeed = OriginalSideSpeed / CrouchSlowness;
        }
        else if (context.canceled && IsCrouching)
        {
            IsCrouching = false;
            SideSpeed = OriginalSideSpeed;
        }
    }
}