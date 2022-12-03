using UnityEngine;
using UnityEngine.Assertions;

public class Warehouse : MonoBehaviour
{
    private int _wheat = 0;
    private int _milk = 0;

    public bool HasMilk()
    {
        return (_milk > 0);
    }

    public bool HasWheat()
    {
        return (_wheat > 0);
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

    public void AddWheat(int value)
    {
        Assert.IsTrue(value >= 0);
        _wheat += value;
    }

    public void AddMilk(int value)
    {
        Assert.IsTrue(value >= 0);
        _wheat += value;
    }
}
