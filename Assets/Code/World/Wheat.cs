using System;
using System.Collections;
using UnityEngine;

public class Wheat : MonoBehaviour
{
    public event Action<bool> OnSowed;
    public event Action<bool> OnGrown;
    public event Action<bool> OnNeedsWater;
    private bool _hasGrown = false;
    private bool _needsWater = false;
    [SerializeField] private float _growingTimeFromBeingWatered = 5f;
    [SerializeField] private float _needsWaterTime = 5f;

    //Sembrar
    public void Sow()
    {
        StartCoroutine(NeedsWaterCycle());
        OnSowed?.Invoke(true);
    }

    private IEnumerator NeedsWaterCycle()
    {
        yield return new WaitForSeconds(_needsWaterTime);
        _needsWater = true;
        OnNeedsWater?.Invoke(true);
    }

    public void Water()
    {
        _needsWater = false;
        StartCoroutine(GrowingCycle());
        OnNeedsWater?.Invoke(false);
    }

    private IEnumerator GrowingCycle()
    {
        yield return new WaitForSeconds(_growingTimeFromBeingWatered);
        _hasGrown = true;
        OnGrown?.Invoke(true);
    }

    //Recoger
    public void Reap()
    {
        _hasGrown = false;
        OnSowed?.Invoke(false);
        OnGrown?.Invoke(false);
    }
}
