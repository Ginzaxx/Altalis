using UnityEngine;

public class Animation : MonoBehaviour
{
    private bool IsGrounded;
    private Rigidbody2D RbD;
    private Animator Animate;

    [Header("Movement")]
    public Movement Movement;

    [Header("Ground Check")]
    public float GroundCheckRad = 0.2f;
    public Transform GroundCheckPos;
    public LayerMask GroundLayer;


    void Start()
    {
        RbD = GetComponent<Rigidbody2D>();
        Animate = GetComponent<Animator>();
    }

    void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(GroundCheckPos.position, GroundCheckRad, GroundLayer);

        Animate.SetFloat("Walking", Mathf.Abs(Movement.SideMove));
        Animate.SetFloat("YVelocity", RbD.velocity.y);
        Animate.SetBool("Jumping", !IsGrounded);
    }
}
