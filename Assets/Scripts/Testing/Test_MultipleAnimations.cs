using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_MultipleAnimations : MonoBehaviour
{
    [SerializeField] private float timeUntilAnimationEnd;
    [SerializeField] private Animator animator;
    [SerializeField] private List<string> clips = new();

    //private variables
    private bool isPlayingAnimation;

    private void Update()
    {
        if (!isPlayingAnimation)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(PlayAnimation(clips[0]));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(PlayAnimation(clips[1]));
            }
        }
    }

    private IEnumerator PlayAnimation(string clip)
    {
        isPlayingAnimation = true;

        animator.Play(clip, 0, 0.0f);

        yield return new WaitForSeconds(timeUntilAnimationEnd);

        isPlayingAnimation = false;
    }
}