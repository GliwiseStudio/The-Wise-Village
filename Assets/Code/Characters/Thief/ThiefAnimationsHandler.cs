using UnityEngine;

public class ThiefAnimationsHandler : AnimationsHandler
{
    private int _stealSuccesfullyID;
    public ThiefAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _stealSuccesfullyID = Animator.StringToHash("StealSuccesfully");
    }

    public bool GetStealSuccesfully()
    {
        return animator.GetBool(_stealSuccesfullyID);
    }
}
