using UnityEngine;

public class DeathCircleInnerController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // This function is called by the Animation Event
    public void OnRebirthAnimationEnd()
    {
        gameObject.SetActive(false);
        DeathDetectorRebirth.Instance.isReloadBecauseDeath = false;
    }
}