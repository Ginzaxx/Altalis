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
    [Header("Jumping")]
    public float JumpPower = 4f;
    public float GravityPower = 1f;
    [Header("Crouching")]
    public float CrouchPower = 2f;
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
    }

    void Update()
    {
        // Set Rigicbody Velocity value
        RbD.velocity = new Vector2(SideMove * SideSpeed, RbD.velocity.y);
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Convert Player Inputs into Vector values
        SideMove = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Convert Player Inputs into Jump Power values
        if (context.performed)
        {
            // Hold Down on Jump Button = Big Jump
            RbD.velocity = new Vector2(RbD.velocity.y, JumpPower);
        }
        else if (context.canceled)
        {
            // Light tap on Jump Button = Small Jump
            RbD.velocity = new Vector2(RbD.velocity.y, JumpPower * 0.5f);
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        // Convert Player Inputs into Crouch Slowness
        if (context.performed)
        {
            SideSpeed = SideSpeed / CrouchPower;
        }
        if (context.canceled)
        {
            SideSpeed = SideSpeed * CrouchPower;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(GroundCheckPos.position, GroundCheckSize);
    }
}
