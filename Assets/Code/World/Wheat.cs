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

    [SerializeField] private GameObject _plantedWheatGameObject;
    [SerializeField] private GameObject _growedWheatGameObject;

    //Sembrar
    public void Sow()
    {
        _plantedWheatGameObject.SetActive(true);
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

        _plantedWheatGameObject.SetActive(false);
        _growedWheatGameObject.SetActive(true);

        OnGrown?.Invoke(true);
    }

    //Recoger
    public void Reap()
    {
        _hasGrown = false;
        _growedWheatGameObject.SetActive(false);
        OnSowed?.Invoke(false);
        OnGrown?.Invoke(false);
    }
}
