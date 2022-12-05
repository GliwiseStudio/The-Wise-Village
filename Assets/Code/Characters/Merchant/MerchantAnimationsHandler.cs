using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantAnimationsHandler : AnimationsHandler
{
    private int _sellSuccesfullyID;
    private int _talkSuccesfullyID;
    private int _interactWithSuppliesSuccesfullyID;

    public MerchantAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _sellSuccesfullyID = Animator.StringToHash("SellSuccesfully");
        _talkSuccesfullyID = Animator.StringToHash("TalkSuccesfully");
        _interactWithSuppliesSuccesfullyID = Animator.StringToHash("InteractWithSuppliesSuccesfully");
    }

    #region getters
    public bool GetSellSuccesfully()
    {
        return animator.GetBool(_sellSuccesfullyID);
    }

    public bool GetTalkSuccesfully()
    {
        return animator.GetBool(_talkSuccesfullyID);
    }

    public bool GetInteractWithSuppliesSuccesfully()
    {
        return animator.GetBool(_interactWithSuppliesSuccesfullyID);
    }
    #endregion
}
