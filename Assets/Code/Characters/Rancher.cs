using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Rancher : MonoBehaviour
{
    [SerializeField] private CharacterConfigurationSO _configuration;
    private BehaviourTreeEngine _rancherBT;
    private StateMachineEngine _feedCowsSubStateMachine;
    private StateMachineEngine _storeMilkSubStateMachine;

    private Locator _locator;
    private MovementController _movementController;

    private bool _hasFoodFromFeeders = false;

    private void Awake()
    {
        _locator = FindObjectOfType<Locator>();
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        _movementController = new MovementController(navMeshAgent, _configuration, new Vector3(4, 0, -10));

        CreateAI();
    }

    private void CreateAI()
    {
        _rancherBT = new BehaviourTreeEngine();

        //SECUENCIA 1
        LeafNode isInBarnLeafNodeSequence1 = _rancherBT.CreateLeafNode("IsInBarnSequence1", DoNothing, IsInBarn);
        LeafNode areCowsHungryLeafNode = _rancherBT.CreateLeafNode("AreCowsHungry", DoNothing, AreCowsHungry);

        _feedCowsSubStateMachine = new StateMachineEngine(true);

        //PERCEPCIONES
        ValuePerception isInFeederPerception = _feedCowsSubStateMachine.CreatePerception<ValuePerception>(IsInFeedersPerception);
        ValuePerception isFoodCollectedPerception = _feedCowsSubStateMachine.CreatePerception<ValuePerception>(IsFoodCollectedPerception);
        ValuePerception isInBarn = _feedCowsSubStateMachine.CreatePerception<ValuePerception>(IsInBarnPerception);
        ValuePerception areCowsFed = _feedCowsSubStateMachine.CreatePerception<ValuePerception>(AreCowsFedPerception);

        //ESTADOS
        State moveToFeederState = _feedCowsSubStateMachine.CreateEntryState("MoveToFeeders", MoveToFeeders);
        State collectFoodState = _feedCowsSubStateMachine.CreateState("CollectFood", CollectFood);
        State moveToBarnState = _feedCowsSubStateMachine.CreateState("MoveToBarn", MoveToBarn);
        State feedCowsState = _feedCowsSubStateMachine.CreateState("FeedCows", FeedCows);

        //TRANSICIONES
        _feedCowsSubStateMachine.CreateTransition("MoveToFeeders-CollectFood", moveToFeederState, isInFeederPerception, collectFoodState);
        _feedCowsSubStateMachine.CreateTransition("CollectFood-MoveToBarn", collectFoodState, isFoodCollectedPerception, moveToBarnState);
        _feedCowsSubStateMachine.CreateTransition("MoveToBarn-FeedCows", moveToBarnState, isInBarn, feedCowsState);

        LeafNode feedSubmachineLeafNode = _rancherBT.CreateSubBehaviour("FeedCowsProcess", _feedCowsSubStateMachine);
        _feedCowsSubStateMachine.CreateExitTransition("FeedCows-Exit", feedCowsState, areCowsFed, ReturnValues.Succeed);

        SucceederDecoratorNode feedCowsSucceeder = _rancherBT.CreateSucceederNode("FeedCowsSucceeder", feedSubmachineLeafNode);

        SequenceNode feedSequence = _rancherBT.CreateSequenceNode("FeedSequence", false);
        feedSequence.AddChild(isInBarnLeafNodeSequence1);
        feedSequence.AddChild(areCowsHungryLeafNode);
        feedSequence.AddChild(feedCowsSucceeder);

        //SECUENCIA 2
        LeafNode isInBarnLeafNodeSequence2 = _rancherBT.CreateLeafNode("IsInBarnSequence2", DoNothing, IsInBarn);
        LeafNode areCowsWithMilkLeafNode = _rancherBT.CreateLeafNode("AreCowsWithMilk", DoNothing, AreCowsWithMilk);

        LeafNode milkCowsLeafNode = _rancherBT.CreateLeafNode("MilkCows", MilkCows, MilkCowsCheck);
        SucceederDecoratorNode milkCowsSucceeder = _rancherBT.CreateSucceederNode("MilkCowsSucceeder", milkCowsLeafNode);

        SequenceNode milkCowsSequence = _rancherBT.CreateSequenceNode("MilkCowsSequence", false);
        milkCowsSequence.AddChild(isInBarnLeafNodeSequence2);
        milkCowsSequence.AddChild(areCowsWithMilkLeafNode);
        milkCowsSequence.AddChild(milkCowsSucceeder);

        //SECUENCIA 3
        LeafNode isInBarnLeafNodeSequence3 = _rancherBT.CreateLeafNode("IsInBarnSequence3", DoNothing, IsInBarn);
        LeafNode generatedMilkLeafNode = _rancherBT.CreateLeafNode("GeneratedMilk", DoNothing, IsMilkGenerated);

        _storeMilkSubStateMachine = new StateMachineEngine(true);
        LeafNode storeMilkLeafNode = _rancherBT.CreateSubBehaviour("StoreMilkProcess", _storeMilkSubStateMachine);

        //PERCEPCIONES
        ValuePerception isMilkCollectedPerception = _storeMilkSubStateMachine.CreatePerception<ValuePerception>(IsMilkCollected);
        ValuePerception isInStoragePerception = _storeMilkSubStateMachine.CreatePerception<ValuePerception>(IsInStorage);
        ValuePerception isMilkStoredPerception = _storeMilkSubStateMachine.CreatePerception<ValuePerception>(IsMilkStored);
        ValuePerception isInBarnPerception = _storeMilkSubStateMachine.CreatePerception<ValuePerception>(IsInBarnPerception);

        //ESTADOS
        State collectMilkState = _storeMilkSubStateMachine.CreateEntryState("CollectMilk", CollectMilk);
        State moveToStoreageState = _storeMilkSubStateMachine.CreateState("MoveToStorage", MoveToStorage);
        State storeMilkState = _storeMilkSubStateMachine.CreateState("StoreMilk", StoreMilk);
        State moveToBarnStatee = _storeMilkSubStateMachine.CreateState("MoveToBarn", MoveToBarn);

        //TRANSICIONES
        _storeMilkSubStateMachine.CreateTransition("CollectMilk-MoveToStorage", collectMilkState, isMilkStoredPerception, moveToStoreageState);
        _storeMilkSubStateMachine.CreateTransition("MoveToStorage-StoreMilk", moveToStoreageState, isInStoragePerception, storeMilkState);
        _storeMilkSubStateMachine.CreateTransition("StoreMilk-MoveToBarn", storeMilkState, isMilkStoredPerception, moveToBarnStatee);
        _storeMilkSubStateMachine.CreateExitTransition("MoveToBarn-Exit", moveToBarnStatee, isInBarnPerception, ReturnValues.Succeed);

        SucceederDecoratorNode storeMilkSucceeder = _rancherBT.CreateSucceederNode("StoreMilkSucceeder", storeMilkLeafNode);

        SequenceNode storeMilkSequence = _rancherBT.CreateSequenceNode("StoreMilkSequence", false);
        storeMilkSequence.AddChild(isInBarnLeafNodeSequence3);
        storeMilkSequence.AddChild(generatedMilkLeafNode);
        storeMilkSequence.AddChild(storeMilkSucceeder);

        //SELECTOR
        SelectorNode selector = _rancherBT.CreateSelectorNode("Selector");
        selector.AddChild(feedSequence);
        selector.AddChild(milkCowsSequence);
        selector.AddChild(storeMilkSequence);

        //LOOP INFINITO
        LoopDecoratorNode infiniteLoop = _rancherBT.CreateLoopNode("InfiniteLoop", selector);

        _rancherBT.SetRootNode(infiniteLoop);
    }

    private ReturnValues IsInBarn()
    {
        if(IsCharacterInBarn())
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private ReturnValues AreCowsHungry()
    {
        Debug.Log("Boom");
        return ReturnValues.Succeed;
    }

    private bool IsInFeedersPerception()
    {
        return _locator.IsCharacterInPlace(transform.position, "Comederos");
    }

    private bool IsFoodCollectedPerception()
    {
        return _hasFoodFromFeeders;
    }

    private bool IsInBarnPerception()
    {
        return IsCharacterInBarn();
    }

    private bool IsCharacterInBarn()
    {
        return _locator.IsCharacterInPlace(transform.position, "Establo");
    }
    
    private bool AreCowsFedPerception()
    {
        return !_hasFoodFromFeeders;
    }

    private void MoveToFeeders()
    {
        Debug.Log("Move To feeders");
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Comederos"));
    }
    
    private void CollectFood()
    {
        Debug.Log("Collecting food from feeders");
        StartCoroutine(CollectFoodCoroutine());
    }

    private IEnumerator CollectFoodCoroutine()
    {
        yield return new WaitForSeconds(2f);
        _hasFoodFromFeeders = true;
    }

    private void MoveToBarn()
    {
        Debug.Log("Moving To Barn");
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Establo"));
    }

    private void FeedCows()
    {
        Debug.Log("Feeding Cows");
        StartCoroutine(FeedingCows());
    }

    private IEnumerator FeedingCows()
    {
        yield return new WaitForSeconds(3f);
        _hasFoodFromFeeders = false;
    }

    private ReturnValues AreCowsWithMilk()
    {
        Debug.Log("Yes");
        return ReturnValues.Succeed;
    }

    private void MilkCows()
    {

        Debug.Log("Yes");
    }

    private ReturnValues MilkCowsCheck()
    {
        Debug.Log("Yes");
        return ReturnValues.Succeed;
    }

    private ReturnValues IsMilkGenerated()
    {
        Debug.Log("Yes");
        return ReturnValues.Succeed;
    }

    private bool IsMilkCollected()
    {
        Debug.Log("Yes");
        return true;
    }

    private bool IsInStorage()
    {
        Debug.Log("Yes");
        return true;
    }

    private bool IsMilkStored()
    {
        Debug.Log("Yes");
        return true;
    }

    private void CollectMilk()
    {

        Debug.Log("Yes");
    }

    private void MoveToStorage()
    {

        Debug.Log("Yes");
    }

    private void StoreMilk()
    {

        Debug.Log("Yes");
    }

    private void DoNothing() { Debug.Log("AA"); }

    private void Update()
    {
        _storeMilkSubStateMachine.Update();
        _feedCowsSubStateMachine.Update();
        _rancherBT.Update();
    }
}
