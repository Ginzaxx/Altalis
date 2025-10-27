using UnityEngine;
using System.Collections;

public class PlaySoundEnter : StateMachineBehaviour
{
    [Header("Sound Settings")]
    public string soundGroup;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Tambahkan delay sebelum suara dimainkan (opsional)")]
    public float delay = 0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (delay > 0f && SoundManager.instance != null)
            SoundManager.instance.StartCoroutine(PlayWithDelay());
        else
            SoundManager.PlaySound(soundGroup, volume);
    }

    private IEnumerator PlayWithDelay()
    {
        yield return new WaitForSeconds(delay);
        SoundManager.PlaySound(soundGroup, volume);
    }
}
