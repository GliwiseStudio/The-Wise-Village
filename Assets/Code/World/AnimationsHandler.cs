using UnityEngine;

public class AnimationsHandler
{
    protected readonly Animator animator = null;

    public AnimationsHandler(Animator animator)
    {
        this.animator = animator;
    }

    public void PlayAnimationState(string animation, float transitionDuration)
    {
        animator.CrossFade(animation, transitionDuration);
    }
}
