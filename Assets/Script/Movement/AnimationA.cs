using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationA : MonoBehaviour
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

    [Header("Death Circle Animation")]
    public GameObject DeathCircle;
    public Animator DeathCircleAnim;


    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        if (DeathDetectorRebirth.Instance.isReloadBecauseDeath == true)
        {
            // doing dhe nigga goblok anjing tolol.
            //this fucking code should be fucking working.
            Debug.LogError("KKKK");
            DeathCircle.SetActive(true);
            DeathCircleAnim.SetTrigger("Rebirth");
        }
        Magma.OnPlayerDeath += DeathListener;
        Spike.OnPlayerDeath += DeathListener;
    }



    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        Magma.OnPlayerDeath -= DeathListener;
        Spike.OnPlayerDeath -= DeathListener;
    }

    void OnDestroy()
    {
        // Also unsubscribe when destroyed (during scene reload)
        Magma.OnPlayerDeath -= DeathListener;
        Spike.OnPlayerDeath -= DeathListener;
    }
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

    void DeathListener()
    {
        // Check if objects still exist before using them
        if (this == null || DeathCircle == null || DeathCircleAnim == null)
        {
            return; // Object is being destroyed, exit early
        }
        Animate.SetTrigger("Die");
        DeathCircle.SetActive(true);
        DeathCircleAnim.SetTrigger("Death");
        DeathDetectorRebirth.Instance.isReloadBecauseDeath = true;
    }

}
