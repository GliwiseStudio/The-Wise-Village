using UnityEngine;
using UnityEngine.Assertions;

public class Warehouse : MonoBehaviour
{
    private int _wheat = 0;
    private int _milk = 0;

    public void ToEmpty()
    {
        _wheat = 0;
        _milk = 0;
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
