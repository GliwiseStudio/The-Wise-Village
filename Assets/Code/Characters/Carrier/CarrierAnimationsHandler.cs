using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrierAnimationsHandler : AnimationsHandler
{
    private int _grabObjectSuccesfullyID;
    private int _giveItemsSuccesfullyID;

    public CarrierAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _grabObjectSuccesfullyID = Animator.StringToHash("GrabObjectSuccesfully");
        _giveItemsSuccesfullyID = Animator.StringToHash("GiveItemsSuccesfully");
    }

    #region getters
    public bool GetGrabObjectSuccesfully()
    {
        return animator.GetBool(_grabObjectSuccesfullyID);
    }

    public bool GiveItemsSuccesfully()
    {
        return animator.GetBool(_giveItemsSuccesfullyID);
    }

    #endregion
}
