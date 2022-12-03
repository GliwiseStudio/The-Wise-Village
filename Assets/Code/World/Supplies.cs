using System;
using UnityEngine;

public class Supplies : MonoBehaviour
{
    public event Action OnDelivery;
    public event Action OnDemand;
    private int _milk = 0;
    private int _wheat = 0;

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
}
