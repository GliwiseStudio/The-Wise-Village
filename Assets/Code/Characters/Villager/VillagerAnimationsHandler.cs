using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerAnimationsHandler : AnimationsHandler
{
    private int _getProductsSuccesfullyID;
    private int _getWaterSuccesfullyID;
    private int _getDrankSuccesfullyID;

    public VillagerAnimationsHandler(Animator animator) : base(animator)
    {
        InitializeVariables();
    }

    private void InitializeVariables()
    {
        _getWaterSuccesfullyID = Animator.StringToHash("GetWaterSuccesfully");
        _getProductsSuccesfullyID = Animator.StringToHash("GetProductsSuccesfully");
        _getDrankSuccesfullyID = Animator.StringToHash("GetDrankSuccesfully");
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

    public bool GetDrankSuccesfully()
    {
        return animator.GetBool(_getDrankSuccesfullyID);
    }
    #endregion
}
