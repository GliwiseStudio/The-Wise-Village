using System;
using UnityEngine;

public class Supplies : MonoBehaviour
{
    public event Action<bool> OnMerchantIsInShop;
    public event Action OnDelivery;
    public event Action OnDemand;
    [SerializeField] private int _milk = 3;
    [SerializeField] private int _wheat = 1;

    public void SetIsMerchantInShop(bool status)
    {
        OnMerchantIsInShop?.Invoke(status);
    }

    public int GetMilk()
    {
        int tmp = _milk;
        _milk = 0;
        return tmp;
    }

    public int GetWheat()
    {
        int tmp = _wheat;
        _wheat = 0;
        return tmp;
    }

    public void Deliver(int milk, int wheat)
    {
        _milk += milk;
        _wheat += wheat;
        OnDelivery?.Invoke();
    }

    public void Demand()
    {
        OnDemand?.Invoke();
    }

    public bool IsThereMilkLeft()
    {
        if (_milk > 0)
            return true;
        else
            return false;
    }

    public bool IsThereWheatLeft()
    {
        if (_wheat > 0)
            return true;
        else
            return false;
    }

    public void MerchantGet()
    {
        _wheat--;
        _milk--;
    }
}
