using System;
using UnityEngine;

public class Cow : MonoBehaviour
{
    public event Action OnMilked;
    public event Action OnFed;
    private bool _isEating = false;
    private bool _isBeingMilked = false;

    public void Feed()
    {

    }

    public void Milk()
    {

    }
}
