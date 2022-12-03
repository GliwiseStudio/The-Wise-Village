using UnityEngine;

public class PoliceAnimationsHandler : AnimationsHandler
{
    private int _knockSuccesfullyID;    //????

    public bool GetReapSuccesfully()
    {
        return animator.GetBool(_knockSuccesfullyID);
    }

    public PoliceAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _knockSuccesfullyID = Animator.StringToHash("KnockSuccesfully");
    }
}
