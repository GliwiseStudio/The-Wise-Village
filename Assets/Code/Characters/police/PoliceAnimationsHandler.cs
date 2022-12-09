using UnityEngine;

public class PoliceAnimationsHandler : AnimationsHandler
{
    private int _punchSuccesfullyID;
    public PoliceAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _punchSuccesfullyID = Animator.StringToHash("PunchSuccesfully");
    }

    public bool GetPunchSuccesfully()
    {
        return animator.GetBool(_punchSuccesfullyID);
    }
}
