using System;
using System.Collections;
using UnityEngine;

public class Cow : MonoBehaviour
{
    public event Action OnMilkGenerated;
    public event Action OnBeingHungry;

    private bool _hasMilk = false;
    private bool _isHungry = true;
    [SerializeField] private float _generateMilkFromBeingFedTime = 5f;
    private IEnumerator _generateMilkFromBeingFedCoroutine;
    [SerializeField] private float _becomeHungryTime = 5f;
    private IEnumerator _becomeHungryCoroutine;

    private void Awake()
    {
        _generateMilkFromBeingFedCoroutine = GenerateMilkFromBeingFed();
        _becomeHungryCoroutine = BecomeHungry();
    }

    public void Milk()
    {
        _hasMilk = false;
        StartCoroutine(_becomeHungryCoroutine);
    }

    public void Feed()
    {
        _isHungry = false;
        StartCoroutine(_generateMilkFromBeingFedCoroutine);
    }

    private IEnumerator GenerateMilkFromBeingFed()
    {
        yield return new WaitForSeconds(_generateMilkFromBeingFedTime);
        _hasMilk = true;
        OnMilkGenerated?.Invoke();
    }

    private IEnumerator BecomeHungry()
    {
        yield return new WaitForSeconds(_becomeHungryTime);
        _isHungry = true;
        OnBeingHungry?.Invoke();
    }
}
