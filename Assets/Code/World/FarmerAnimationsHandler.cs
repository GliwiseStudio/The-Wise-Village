using UnityEngine;

public class FarmerAnimationsHandler : AnimationsHandler
{
    private int _reapSuccesfullyID;

    public bool GetReapSuccesfully()
    {
        return animator.GetBool(_reapSuccesfullyID);
    }

    public FarmerAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _reapSuccesfullyID = Animator.StringToHash("ReapSuccesfully");
    }
}
