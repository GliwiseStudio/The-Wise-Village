using System.Collections;
using UnityEngine;

public class Farmer : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private FarmerAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _farmerBT;
    private StateMachineEngine _reapWheatSubStateMachine;
    private Locator _locator;

    private Wheat _wheat;
    private bool _isWheatSowed = false;
    private bool _isWheatWatered = true;
    private bool _isWheatGrown = false;

    private bool _isSowingWheat = false;
    private bool _isWateringWheat = false;
    private bool _isReapingWheat = false;
    private bool _isStoringWheat = false;

    private void Awake()
    {
        _wheat = FindObjectOfType<Wheat>();
        _animationsHandler = new FarmerAnimationsHandler(_animator);
        _locator = FindObjectOfType<Locator>();
        CreateAI();
    }

    private void CreateAI()
    {
        _farmerBT = new BehaviourTreeEngine();

        //SECUENCIA 1
        LeafNode sowWheatLeafNode = _farmerBT.CreateLeafNode("SowWheat", SowWheat, SowWheatCheck);
        SucceederDecoratorNode sowWheatSucceeder = _farmerBT.CreateSucceederNode("SowWheatSucceeder", sowWheatLeafNode);

        LeafNode isInCountrysideLeafNode = _farmerBT.CreateLeafNode("IsInCountryside", DoNothing, IsInCountryside);

        LeafNode isWheatSowedLeafNode = _farmerBT.CreateLeafNode("IsWheatSowed", DoNothing, IsWheatSowed);
        InverterDecoratorNode IsWheatSowedInverter = _farmerBT.CreateInverterNode("IsWheatSowedInverter", isWheatSowedLeafNode);

        SequenceNode sowWheatSequence = _farmerBT.CreateSequenceNode("SowWheatSequence", false);
        sowWheatSequence.AddChild(isInCountrysideLeafNode);
        sowWheatSequence.AddChild(IsWheatSowedInverter);
        sowWheatSequence.AddChild(sowWheatSucceeder);

        //SECUENCIA 2
        LeafNode isInCountrysideLeafNode2 = _farmerBT.CreateLeafNode("IsInCountryside2", DoNothing, IsInCountryside);
        LeafNode isWheatWateredLeafNode = _farmerBT.CreateLeafNode("IsWheatWatered", DoNothing, IsWheatWatered);
        InverterDecoratorNode IsWheatWateredInverter = _farmerBT.CreateInverterNode("IsWheatWateredInverter", isWheatWateredLeafNode);

        LeafNode waterWheatLeafNode = _farmerBT.CreateLeafNode("WaterWheat", WaterWheat, WaterWheatCheck);
        SucceederDecoratorNode waterWheatSucceeder = _farmerBT.CreateSucceederNode("WaterWheatSucceeder", waterWheatLeafNode);

        SequenceNode waterWheatSequence = _farmerBT.CreateSequenceNode("WaterWheatSequence", false);
        waterWheatSequence.AddChild(isInCountrysideLeafNode2);
        waterWheatSequence.AddChild(IsWheatWateredInverter);
        waterWheatSequence.AddChild(waterWheatSucceeder);

        //SECUENCIA 3
        LeafNode isInCountrysideLeafNode3 = _farmerBT.CreateLeafNode("IsInCountryside3", DoNothing, IsInCountryside);
        LeafNode generatedWheatLeafNode = _farmerBT.CreateLeafNode("GeneratedWheat", DoNothing, IsWheatGenerated);

        _reapWheatSubStateMachine = new StateMachineEngine(true);

        //PERCEPCIONES
        ValuePerception hasFinishedReapingPerception = _reapWheatSubStateMachine.CreatePerception<ValuePerception>(HasFinishedReaping);
        ValuePerception hasFinishedStoringWheatPerception = _reapWheatSubStateMachine.CreatePerception<ValuePerception>(hasFinishedStoringWheat);
        ValuePerception isInStoragePerception = _reapWheatSubStateMachine.CreatePerception<ValuePerception>(IsInStoragePerception);
        ValuePerception isInCountrysidePerception = _reapWheatSubStateMachine.CreatePerception<ValuePerception>(IsInCountrysidePerception);

        //ESTADOS
        State reapWheatState = _reapWheatSubStateMachine.CreateEntryState("ReapWheat", ReapWheat);
        State moveToStorageState = _reapWheatSubStateMachine.CreateState("MoveToStorage", MoveToStorage);
        State storeWheatState = _reapWheatSubStateMachine.CreateState("StoreWheat", StoreWheat);
        State moveToCountrysideState = _reapWheatSubStateMachine.CreateState("MoveToCountryside", MoveToCountryside);

        //TRANSICIONES
        _reapWheatSubStateMachine.CreateTransition("Reap-MoveStorage", reapWheatState, hasFinishedReapingPerception, moveToStorageState);
        _reapWheatSubStateMachine.CreateTransition("MoveStorage-StoreWheat", moveToStorageState, isInStoragePerception, storeWheatState);
        _reapWheatSubStateMachine.CreateTransition("StoreWheat-MoveCountryside", storeWheatState, hasFinishedStoringWheatPerception, moveToCountrysideState);
        
        LeafNode reapWheatLeafNode = _farmerBT.CreateSubBehaviour("ReapWheatStateMachine", _reapWheatSubStateMachine);
        _reapWheatSubStateMachine.CreateExitTransition("MoveCountryside-Exit", moveToCountrysideState, isInCountrysidePerception, ReturnValues.Succeed);

        SucceederDecoratorNode reapWheatSucceder = _farmerBT.CreateSucceederNode("ReapWheatSucceeder", reapWheatLeafNode);

        SequenceNode reapWheatSequence = _farmerBT.CreateSequenceNode("ReapWheatSequence", false);
        reapWheatSequence.AddChild(isInCountrysideLeafNode3);
        reapWheatSequence.AddChild(generatedWheatLeafNode);
        reapWheatSequence.AddChild(reapWheatLeafNode);

        //SELECTOR
        SelectorNode selector = _farmerBT.CreateSelectorNode("Selector");
        selector.AddChild(sowWheatSequence);
        selector.AddChild(waterWheatSequence);
        selector.AddChild(reapWheatSequence);

        //LOOP INFINITO
        LoopDecoratorNode rootLoop = _farmerBT.CreateLoopNode("LoopInfinito", selector);
        _farmerBT.SetRootNode(rootLoop);
    }

    private ReturnValues IsInCountryside()
    {
        return ReturnValues.Succeed;
    }

    private ReturnValues IsWheatSowed()
    {
        if(_isWheatSowed)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void SowWheat()
    {
        Debug.Log("Sow Wheat");
        StartCoroutine(SowWheatCycle());
    }

    private IEnumerator SowWheatCycle()
    {
        _isSowingWheat = true;
        yield return new WaitForSeconds(3f);
        _isSowingWheat = false;
        _wheat.Sow();
    }

    private ReturnValues SowWheatCheck()
    {
        if(_isSowingWheat)
        {
            return ReturnValues.Running;
        }

        return ReturnValues.Succeed;
    }

    private ReturnValues IsWheatWatered()
    {
        if (_isWheatWatered)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void WaterWheat()
    {
        Debug.Log("Water Wheat");
        StartCoroutine(WaterWheatCycle());
    }

    private IEnumerator WaterWheatCycle()
    {
        _isWateringWheat = true;
        yield return new WaitForSeconds(3f);
        _isWateringWheat = false;
        _wheat.Water();
    }

    private ReturnValues WaterWheatCheck()
    {
        if(_isWateringWheat)
        {
            return ReturnValues.Running;
        }

        return ReturnValues.Succeed;
    }

    private ReturnValues IsWheatGenerated()
    {
        if(_isWheatGrown)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private bool HasFinishedReaping()
    {
        return !_isReapingWheat;
    }

    private bool hasFinishedStoringWheat()
    {
        return !_isStoringWheat;
    }

    private bool IsInStoragePerception()
    {
        return true;
    }

    private bool IsInCountrysidePerception()
    {
        return true;
    }

    private void ReapWheat()
    {
        Debug.Log("Reap Wheat");
        StartCoroutine(ReapWheatCycle());
    }

    private IEnumerator ReapWheatCycle()
    {
        _isReapingWheat = true;
        yield return new WaitForSeconds(3f);
        _isReapingWheat = false;
        _wheat.Reap();
    }

    private void MoveToStorage()
    {
        Debug.Log("Moving To Storage");
    }

    private void StoreWheat()
    {
        Debug.Log("Store Wheat");
        StartCoroutine(StoreWheatCycle());
    }

    private IEnumerator StoreWheatCycle()
    {
        _isStoringWheat = true;
        yield return new WaitForSeconds(3f);
        _isStoringWheat = false;
    }

    private void MoveToCountryside()
    {
        Debug.Log("Moving To Countryside");
    }

    private void DoNothing()
    {

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(_farmerBT.GetCurrentState().Name);
        }

        _farmerBT.Update();
        _reapWheatSubStateMachine.Update();
    }

    private void OnEnable()
    {
        _wheat.OnSowed += SetIsWheatSowed;
        _wheat.OnNeedsWater += SetWheatNeedsWater;
        _wheat.OnGrown += SetIsWheatGrown;
    }

    private void OnDisable()
    {
        _wheat.OnSowed -= SetIsWheatSowed;
        _wheat.OnNeedsWater -= SetWheatNeedsWater;
        _wheat.OnGrown -= SetIsWheatGrown;
    }

    private void SetIsWheatSowed(bool status)
    {
        _isWheatSowed = status;
    }

    private void SetWheatNeedsWater(bool status)
    {
        _isWheatWatered = !status;
    }

    private void SetIsWheatGrown(bool status)
    {
        _isWheatGrown = status;
    }
}
