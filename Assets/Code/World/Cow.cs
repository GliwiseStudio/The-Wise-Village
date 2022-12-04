using System;
using UnityEngine;

public class Cow : MonoBehaviour
{
    public event Action OnMilkGenerated;
    public event Action OnBeingHungry;

    private bool _hasMilk = false;
    private bool _isHungry = false;
    [SerializeField] private float _generateMilkFromBeingFedTime = 5f;
    [SerializeField] private float _becomeHungryTime = 5f;

    private float _hungerCurrentTime = 0f;
    private float _milkCurrentTime = 0f;

    private void Update()
    {
        UpdateHunger();
        UpdateMilkGeneration();
    }

    private void UpdateHunger()
    {
        if(_isHungry)
        {
            return;
        }

        _hungerCurrentTime += Time.deltaTime;

        if(_hungerCurrentTime >= _becomeHungryTime)
        {
            _hungerCurrentTime = 0;
            _isHungry = true;
            OnBeingHungry?.Invoke();
        }
    }

    private void UpdateMilkGeneration()
    {
        if(_hasMilk)
        {
            return;
        }

        _milkCurrentTime += Time.deltaTime;

        if (_milkCurrentTime >= _generateMilkFromBeingFedTime)
        {
            _milkCurrentTime = 0;
            _hasMilk = true;
            OnMilkGenerated?.Invoke();
        }
    }

    public void Feed()
    {
        _isHungry = false;
    }

    public void Milk()
    {
        _hasMilk = false;
    }
}
