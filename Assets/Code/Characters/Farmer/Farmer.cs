using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Farmer : MonoBehaviour
{
    [SerializeField] private CharacterConfigurationSO _configuration;
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshProUGUI _text;
    private FarmerAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _farmerBT;
    private StateMachineEngine _reapWheatSubStateMachine;
    private Locator _locator;
    private MovementController _movementController;
    private NavMeshAgent _agent;
    private Warehouse _warehouse;

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
        _agent = GetComponent<NavMeshAgent>();
        _warehouse = FindObjectOfType<Warehouse>();
        CreateAI();
    }

    private void Start()
    {
        _movementController = new MovementController(_agent, _configuration, _locator.GetPlaceOfInterestPositionFromName("Countryside"));
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
        if(IsCharacterInCountryside())
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    private bool IsCharacterInCountryside()
    {
        return _locator.IsCharacterInPlace(transform.position, "Countryside");
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
        //Debug.Log("Sow Wheat");
        _text.text = "Sowing wheat";
        _animationsHandler.PlayAnimationState("SowWheat", 0.1f);
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
        //Debug.Log("Water Wheat");
        _text.text = "Watering wheat";
        _animationsHandler.PlayAnimationState("WaterWheat", 0.1f);
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
        return _locator.IsCharacterInPlace(transform.position, "Storage");
    }

    private bool IsInCountrysidePerception()
    {
        return IsCharacterInCountryside();
    }

    private void ReapWheat()
    {
        //Debug.Log("Reap Wheat");
        _text.text = "Reaping wheat";
        _animationsHandler.PlayAnimationState("ReapWheat", 0.1f);
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
        //Debug.Log("Moving To Storage");
        _text.text = "Going to storage";
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Storage"));
    }

    private void StoreWheat()
    {
        //Debug.Log("Store Wheat");
        StartCoroutine(StoreWheatCycle());
    }

    private IEnumerator StoreWheatCycle()
    {
        _isStoringWheat = true;
        yield return new WaitForSeconds(3f);
        _isStoringWheat = false;
        _warehouse.AddWheat(1);
    }

    private void MoveToCountryside()
    {
        //Debug.Log("Moving To Countryside");
        _text.text = "Going to countryside";
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Countryside"));
    }

    private void DoNothing()
    {

    }

    private void Update()
    {
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
