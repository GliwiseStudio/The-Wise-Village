using UnityEngine;

public class Farmer : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private FarmerAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _farmerBT;
    private Locator _locator;

    private void Awake()
    {
        _animationsHandler = new FarmerAnimationsHandler(_animator);
        _locator = FindObjectOfType<Locator>();
    }

    private void CreateAI()
    {
        //SECUENCIA 1
        LeafNode sowWheatLeafNode = _farmerBT.CreateLeafNode("SowWheat", SowWheat, SowWheatCheck);
        SucceederDecoratorNode sowWheatSucceeder = _farmerBT.CreateSucceederNode("SowWheatSucceeder", sowWheatLeafNode);

        LeafNode isInCountrysideLeafNode = _farmerBT.CreateLeafNode("IsInCountryside", null, IsInCountryside);

        LeafNode isWheatSowedLeafNode = _farmerBT.CreateLeafNode("IsWheatSowed", null, IsWheatSowed);
        InverterDecoratorNode IsWheatSowedInverter = _farmerBT.CreateInverterNode("IsWheatSowedInverter", isWheatSowedLeafNode);

        SequenceNode sowWheatSequence = _farmerBT.CreateSequenceNode("SowWheatSequence", false);
        sowWheatSequence.AddChild(isInCountrysideLeafNode);
        sowWheatSequence.AddChild(IsWheatSowedInverter);
        sowWheatSequence.AddChild(sowWheatSucceeder);

        //SECUENCIA 2
        LeafNode isWheatWateredLeafNode = _farmerBT.CreateLeafNode("IsWheatWatered", null, IsWheatWatered);
        InverterDecoratorNode IsWheatWateredInverter = _farmerBT.CreateInverterNode("IsWheatWateredInverter", isWheatSowedLeafNode);

        LeafNode waterWheatLeafNode = _farmerBT.CreateLeafNode("WaterWheat", WaterWheat, WaterWheatCheck);
        SucceederDecoratorNode waterWheatSucceeder = _farmerBT.CreateSucceederNode("WaterWheatSucceeder", waterWheatLeafNode);

        SequenceNode waterWheatSequence = _farmerBT.CreateSequenceNode("WaterWheatSequence", false);
        waterWheatSequence.AddChild(isInCountrysideLeafNode);
        waterWheatSequence.AddChild(IsWheatWateredInverter);
        waterWheatSequence.AddChild(waterWheatSucceeder);

        //SECUENCIA 3
        LeafNode generatedWheatLeafNode = _farmerBT.CreateLeafNode("GeneratedWheat", null, IsWheatGenerated);

        StateMachineEngine reapWheatStateMachine = new StateMachineEngine(true);

        //PERCEPCIONES
        ValuePerception hasReapAnimationFinishedPerception = reapWheatStateMachine.CreatePerception<ValuePerception>(HasReapAnimationFinished);
        ValuePerception hasStoredWheatAnimationFinishedPerception = reapWheatStateMachine.CreatePerception<ValuePerception>(HasStoredWheatAnimationFinished);
        ValuePerception isInStoragePerception = reapWheatStateMachine.CreatePerception<ValuePerception>(IsInStoragePerception);
        ValuePerception isInCountrysidePerception = reapWheatStateMachine.CreatePerception<ValuePerception>(IsInCountrysidePerception);

        //ESTADOS
        State reapWheatState = reapWheatStateMachine.CreateEntryState("ReapWheat", ReapWheat);
        State moveToStorageState = reapWheatStateMachine.CreateState("MoveToStorage", MoveToStorage);
        State storeWheatState = reapWheatStateMachine.CreateState("StoreWheat", StoreWheat);
        State moveToCountrysideState = reapWheatStateMachine.CreateState("MoveToCountryside", MoveToCountryside);

        //TRANSICIONES
        reapWheatStateMachine.CreateTransition("Reap-MoveStorage", reapWheatState, hasReapAnimationFinishedPerception, moveToStorageState);
        reapWheatStateMachine.CreateTransition("MoveStorage-StoreWheat", moveToStorageState, hasStoredWheatAnimationFinishedPerception, storeWheatState);
        reapWheatStateMachine.CreateTransition("StoreWheat-MoveCountryside", storeWheatState, isInStoragePerception, moveToCountrysideState);
        reapWheatStateMachine.CreateExitTransition("MoveCountryside-Exit", storeWheatState, isInCountrysidePerception, ReturnValues.Succeed);

        LeafNode reapWheatLeafNode = _farmerBT.CreateSubBehaviour("ReapWheatStateMachine", reapWheatStateMachine);
        SucceederDecoratorNode reapWheatSucceder = _farmerBT.CreateSucceederNode("ReapWheatSucceeder", reapWheatLeafNode);

        SequenceNode reapWheatSequence = _farmerBT.CreateSequenceNode("ReapWheatSequence", false);
        reapWheatSequence.AddChild(isInCountrysideLeafNode);
        reapWheatSequence.AddChild(generatedWheatLeafNode);
        reapWheatSequence.AddChild(reapWheatLeafNode);

        //SELECTOR
        SelectorNode selector = _farmerBT.CreateSelectorNode("Selector");
        selector.AddChild(sowWheatSequence);
        selector.AddChild(waterWheatSequence);
        selector.AddChild(reapWheatSequence);

        //LOOP INFINITO
        _farmerBT.CreateLoopNode("LoopInfinito", selector);
    }

    private ReturnValues IsInCountryside()
    {
        return ReturnValues.Succeed;
    }

    private ReturnValues IsWheatSowed()
    {
        return ReturnValues.Succeed;
    }

    private void SowWheat()
    {

    }

    private ReturnValues SowWheatCheck()
    {
        return ReturnValues.Succeed;
    }

    private ReturnValues IsWheatWatered()
    {
        return ReturnValues.Succeed;
    }

    private void WaterWheat()
    {

    }

    private ReturnValues WaterWheatCheck()
    {
        return ReturnValues.Succeed;
    }

    private ReturnValues IsWheatGenerated()
    {
        return ReturnValues.Succeed;
    }

    private bool HasReapAnimationFinished()
    {
        return true;
    }

    private bool HasStoredWheatAnimationFinished()
    {
        return true;
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

    }

    private void MoveToStorage()
    {

    }

    private void StoreWheat()
    {

    }

    private void MoveToCountryside()
    {

    }
}
