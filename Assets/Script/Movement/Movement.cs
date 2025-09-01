using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D RbD;
    private Animator Animate;

    [Header("Movement")]
    public float SideSpeed = 8f;
    private float originalSideSpeed; // Store original speed for crouch calculations
    
    [Header("Jumping")]
    public float JumpPower = 6f;
    public float GravityPower = 1f;
    
    [Header("Crouching")]
    public float CrouchPower = 2f;
    private bool isCrouching = false;
    
    private float SideMove;
    
    [Header("GroundCheck")]
    public Vector2 GroundCheckSize = new Vector2(0.5f, 0.05f);
    public Transform GroundCheckPos;
    public LayerMask GroundMask;

    void Start()
    {
        // Get Rigidbody and Animator values
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
        
        // Store original speed
        originalSideSpeed = SideSpeed;
    }

    void Update()
    {
        // Only move if movement is enabled and we're in movement mode
        if (!enabled) return;
        
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

        // Convert Player Inputs into Jump Power values
        if (context.performed)
        {
            // Hold Down on Jump Button = Big Jump
            RbD.velocity = new Vector2(RbD.velocity.x, JumpPower);
        }
        else if (context.canceled && RbD.velocity.y > JumpPower * 0.5)
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
            isCrouching = true;
            SideSpeed = originalSideSpeed / CrouchPower;
        }
        else if (context.canceled && isCrouching)
        {
            isCrouching = false;
            SideSpeed = originalSideSpeed;
        }
    }

    // Ground checking method (you might want to implement this properly)
    private bool IsGrounded()
    {
        if (GroundCheckPos == null) return true; // Default to true if no ground check setup
        
        Collider2D groundHit = Physics2D.OverlapBox(GroundCheckPos.position, GroundCheckSize, 0f, GroundMask);
        return groundHit != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (GroundCheckPos != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawCube(GroundCheckPos.position, GroundCheckSize);
        }
    }
}