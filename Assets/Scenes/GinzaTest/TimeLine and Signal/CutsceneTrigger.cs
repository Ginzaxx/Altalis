using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector cutsceneDirector;
    public MonoBehaviour playerControlScript;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cutsceneDirector.Play();

            if (playerControlScript != null)
                playerControlScript.enabled = false;
        }
    }
}
