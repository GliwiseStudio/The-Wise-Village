using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class Rancher : MonoBehaviour
{
    [SerializeField] private CharacterConfigurationSO _configuration;
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshProUGUI _text;

    private Cow _cow;
    private bool _isCowHungry = false;
    private bool _hasCowMilk = false;
    private bool _isMilkingCows = false;

    private Warehouse _warehouse;

    private BehaviourTreeEngine _rancherBT;
    private StateMachineEngine _feedCowsSubStateMachine;
    private StateMachineEngine _storeMilkSubStateMachine;

    private Locator _locator;
    private MovementController _movementController;
    private CarrierAnimationsHandler _animationsHandler;
    private NavMeshAgent _agent;

    private bool _hasFoodFromFeeders = false;
    private bool _hasMilkToCollect = false;
    private bool _hasMilkFromCows = false;

    private void Awake()
    {
        _cow = FindObjectOfType<Cow>();
        _warehouse = FindObjectOfType<Warehouse>();
        _locator = FindObjectOfType<Locator>();
        _agent = GetComponent<NavMeshAgent>();
        _animationsHandler = new CarrierAnimationsHandler(_animator);

        CreateAI();
    }

    private void Start()
    {

        _movementController = new MovementController(_agent, _configuration, _locator.GetPlaceOfInterestPositionFromName("Barn"));
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
        _storeMilkSubStateMachine.CreateTransition("CollectMilk-MoveToStorage", collectMilkState, isMilkCollectedPerception, moveToStoreageState);
        _storeMilkSubStateMachine.CreateTransition("MoveToStorage-StoreMilk", moveToStoreageState, isInStoragePerception, storeMilkState);
        _storeMilkSubStateMachine.CreateTransition("StoreMilk-MoveToBarn", storeMilkState, isMilkStoredPerception, moveToBarnStatee);

        LeafNode storeMilkLeafNode = _rancherBT.CreateSubBehaviour("StoreMilkProcess", _storeMilkSubStateMachine);
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
        //Debug.Log("Check if cow is hungry");
        if(_isCowHungry)
        {
            _text.text = "Cow hungry";
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private bool IsInFeedersPerception()
    {
        return _locator.IsCharacterInPlace(transform.position, "Feeders");
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
        //Debug.Log("Checking if is in barn");
        return _locator.IsCharacterInPlace(transform.position, "Barn");
    }
    
    private bool AreCowsFedPerception()
    {
        //Debug.Log("Are Cows Fed " + _hasFoodFromFeeders);
        return !_hasFoodFromFeeders;
    }

    private void MoveToFeeders()
    {
        _text.text = "Going to feeders";
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Feeders"));
        PlayWalkAnimation();
    }

    private void PlayWalkAnimation()
    {
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
    }
    
    private void CollectFood()
    {
        //Debug.Log("Collecting food from feeders");
        _text.text = "Collectiong food from feeders";
        StartCoroutine(CollectFoodCoroutine());
    }

    private IEnumerator CollectFoodCoroutine()
    {
        yield return new WaitForSeconds(2f);
        _hasFoodFromFeeders = true;
    }

    private void MoveToBarn()
    {
        //Debug.Log("Moving To Barn");
        _text.text = "Going to barn";
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Barn"));
        PlayWalkAnimation();
    }

    private void FeedCows()
    {
        //Debug.Log("Feeding Cows");
        _text.text = "Feeding cows";
        PlayFeedAnimation();
        StartCoroutine(FeedingCows());
    }

    private IEnumerator FeedingCows()
    {
        yield return new WaitForSeconds(3f);
        _hasFoodFromFeeders = false;
        _isCowHungry = false;
        _cow.Feed();
    }

    private ReturnValues AreCowsWithMilk()
    {
        //Debug.Log("Check if cows have milk");
        if(_hasCowMilk)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void MilkCows()
    {
        _text.text = "Milking cow";
        _isMilkingCows = true;
        PlayMilkAnimation();
        StartCoroutine(MilkingCows());
    }

    private IEnumerator MilkingCows()
    {
        yield return new WaitForSeconds(2.8f);
        _hasCowMilk = false;
        _cow.Milk();
        _hasMilkFromCows = true;
        _isMilkingCows = false;
    }

    private ReturnValues MilkCowsCheck()
    {
        if(_isMilkingCows)
        {
            return ReturnValues.Running;
        }

        return ReturnValues.Succeed;
    }

    private ReturnValues IsMilkGenerated()
    {
        if(_hasMilkFromCows)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private bool IsMilkCollected()
    {
        //Debug.Log("Checking if milk is collected");
        return !_hasMilkToCollect;
    }

    private bool IsInStorage()
    {
        //Debug.Log("Checking if is in storage");
        return _locator.IsCharacterInPlace(transform.position, "Storage");
    }

    private bool IsMilkStored()
    {
        //Debug.Log("Checking if milk is stored");
        return !_hasMilkFromCows;
    }

    private void CollectMilk()
    {
        //Debug.Log("CollectingMilk");
        StartCoroutine(CollectingMilk());
    }

    private IEnumerator CollectingMilk()
    {
        yield return new WaitForSeconds(1.8f);
        _hasMilkToCollect = false;
        _hasMilkFromCows = true;
    }

    private void MoveToStorage()
    {
        //Debug.Log("Moving To Storage");
        _text.text = "Going to storage";
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Storage"));
        PlayWalkAnimation();
    }

    private void StoreMilk()
    {
        //Debug.Log("Storing Milk");
        _text.text = "Storing milk";
        StartCoroutine(StoringMilk());
    }

    private IEnumerator StoringMilk()
    {
        yield return new WaitForSeconds(3f);
        _warehouse.AddMilk(1);
        _hasMilkFromCows = false;
    }

    private void DoNothing() {  }

    private void Update()
    {
        _rancherBT.Update();

        _storeMilkSubStateMachine.Update();
        _feedCowsSubStateMachine.Update();

        if(Input.GetKeyDown(KeyCode.P))
        {
            //Debug.Log(_rancherBT.GetCurrentState().Name);
        }
    }

    private void OnEnable()
    {
        _cow.OnBeingHungry += SetHungryCowFlag;
        _cow.OnMilkGenerated += SetMilkCowFlag;
    }

    private void OnDisable()
    {
        _cow.OnBeingHungry -= SetHungryCowFlag;
        _cow.OnMilkGenerated -= SetMilkCowFlag;
    }

    private void SetHungryCowFlag()
    {
        _isCowHungry = true;
    }

    private void SetMilkCowFlag()
    {
        _hasCowMilk = true;
    }

    private void PlayFeedAnimation()
    {
        _animationsHandler.PlayAnimationState("Feed", 0.1f);
    }

    private void PlayMilkAnimation()
    {
        _animationsHandler.PlayAnimationState("Milk", 0.1f);
    }
}
