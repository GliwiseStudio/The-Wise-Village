using System;
using System.Collections;
using UnityEngine;

public class Wheat : MonoBehaviour
{
    public event Action OnGrown;
    public event Action OnNeedsWater;
    private bool _hasGrown = false;
    private bool _needsWater = false;
    [SerializeField] private float _growingTimeFromBeingWatered = 5f;
    private IEnumerator _growingCycleCoroutine;
    [SerializeField] private float _needsWaterTime = 5f;
    private IEnumerator _needsWaterCycleCoroutine;

    private void Awake()
    {
        _growingCycleCoroutine = GrowingCycle();
        _needsWaterCycleCoroutine = NeedsWaterCycle();
    }

    //Sembrar
    public void Sow()
    {
        StartCoroutine(_needsWaterCycleCoroutine);
    }

    private IEnumerator NeedsWaterCycle()
    {
        yield return new WaitForSeconds(_needsWaterTime);
        _needsWater = true;
        OnNeedsWater?.Invoke();
    }

    public void Water()
    {
        _needsWater = false;
        StartCoroutine(_growingCycleCoroutine);
    }

    private IEnumerator GrowingCycle()
    {
        yield return new WaitForSeconds(_growingTimeFromBeingWatered);
        _hasGrown = true;
        OnGrown?.Invoke();
    }

    //Recoger
    public void Reap()
    {
        _hasGrown = false;
    }
}
