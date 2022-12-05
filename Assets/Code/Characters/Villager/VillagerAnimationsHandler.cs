using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerAnimationsHandler : AnimationsHandler
{
    private int _getProductsSuccesfullyID;
    private int _getWaterSuccesfullyID;

    public VillagerAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _getWaterSuccesfullyID = Animator.StringToHash("GetWaterSuccesfully");
        _getProductsSuccesfullyID = Animator.StringToHash("GetProductsSuccesfully");
    }

    #region getters
    public bool GetProductsSuccesfully()
    {
        return animator.GetBool(_getProductsSuccesfullyID);
    }

    public bool GetWaterSuccesfully()
    {
        return animator.GetBool(_getWaterSuccesfullyID);
    }
    #endregion
}
