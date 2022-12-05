using UnityEngine;

public class ResetAnimatorBoolOnEnter : StateMachineBehaviour
{
    [SerializeField] private string parameterName = System.String.Empty;
    [SerializeField] private bool status = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(parameterName, status);
    }
}
