using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Merchant : MonoBehaviour
{
    #region Variables
    private Animator _animator;
    private MerchantAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _merchantBT;
    private Locator _locator;
    private Supplies _suppliesManager;
    private MovementController _movementController;
    [SerializeField] private CharacterConfigurationSO _configuration;

    private NavMeshAgent _navMeshAgent;
    private TargetDetector _targetDetector;

    #endregion

    private void Awake()
    {
        _locator = FindObjectOfType<Locator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        _animator = GetComponent<Animator>();
        
        _animationsHandler = new MerchantAnimationsHandler(_animator);
        _suppliesManager = FindObjectOfType<Supplies>();
        _targetDetector = new TargetDetector(transform, 10f, "Client");
        
        CreateAI();
    }

    private void Start()
    {
        _movementController = new MovementController(_navMeshAgent, _configuration, _locator.GetPlaceOfInterestPositionFromName("Bar"));
    }

    private void Update()
    {
        _merchantBT.Update();

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Debug.Log(_merchantBT.GetCurrentState().Name);
        //}
        
    }

    private void CreateAI()
    {
        _merchantBT = new BehaviourTreeEngine();

        #region First main leaf node
        // Loop to wait for supplies and make bread
        LeafNode hasSuppliesLN = _merchantBT.CreateLeafNode("hasSuppliesLN", DoNothing, CheckSupplies);
        LeafNode makeBreadLN = _merchantBT.CreateLeafNode("makeBreadLN", InteractWithSupplies, InteractedWithSupplies);

        SequenceNode interactWithSuppliesSN = _merchantBT.CreateSequenceNode("InteractWithSupplies", false);

        interactWithSuppliesSN.AddChild(hasSuppliesLN);
        interactWithSuppliesSN.AddChild(makeBreadLN);
        
        // not necesarry because it will be running until it gets it
        //LoopUntilSucceedDecoratorNode waitForSuppliesDN = _merchantBT.CreateLoopUntilSucceedNode("waitForSuppliesDN", interactWithSuppliesSN);

        // Sequence to get supplies
        LeafNode walkToCarrierLN = _merchantBT.CreateLeafNode("walkToCarrierLN", WalkToCarrier, ArrivedToCarrier);
        LeafNode talkToCarrierLN = _merchantBT.CreateLeafNode("talkToCarrierLN", TalkToCarrier, TalkedToCarrier);
        LeafNode walkBackToShopLN = _merchantBT.CreateLeafNode("walkBackToShopLN", WalkBackToShop, WalkedToShop);

        SequenceNode getSuppliesSN = _merchantBT.CreateSequenceNode("getSuppliesSN", false);

        getSuppliesSN.AddChild(walkToCarrierLN);
        getSuppliesSN.AddChild(talkToCarrierLN);
        getSuppliesSN.AddChild(walkBackToShopLN);
        getSuppliesSN.AddChild(interactWithSuppliesSN);


        // Sequence to check if we have supplies, and if not get them
        LeafNode isCounterEmptyLN = _merchantBT.CreateLeafNode("isCounterEmptyLN", DoNothing, CheckIfCounterEmpty);

        SequenceNode checkSuppliesSN = _merchantBT.CreateSequenceNode("checkSuppliesSN", false);

        checkSuppliesSN.AddChild(isCounterEmptyLN);
        checkSuppliesSN.AddChild(getSuppliesSN);

        // Main leaf node supplies, always have to return false, because if there are no supplies you get them and them check for clients, andif there are you just check for clients
        SucceederDecoratorNode succederCheckSuppliesDN = _merchantBT.CreateSucceederNode("succederCheckSuppliesDN", checkSuppliesSN);

        InverterDecoratorNode inverterSuccederCheckSuppliesDN = _merchantBT.CreateInverterNode("inverterSuccederCheckSuppliesDN", succederCheckSuppliesDN);

        #endregion

        #region Second main leaf node

        // serve customers
        LeafNode isThereAClientLN = _merchantBT.CreateLeafNode("isThereAClient", DoNothing, CheckForClients);
        LeafNode serveCustomerLN = _merchantBT.CreateLeafNode("serveCustomer", ServeCustomer, ServedCustomer);

        SequenceNode checkAndServeCustomersSN = _merchantBT.CreateSequenceNode("checkAndServeCustomers", false);
        checkAndServeCustomersSN.AddChild(isThereAClientLN);
        checkAndServeCustomersSN.AddChild(serveCustomerLN);

        #endregion

        #region Main loop and selector

        SelectorNode pickActionSN = _merchantBT.CreateSelectorNode("pickActionSN");
        pickActionSN.AddChild(inverterSuccederCheckSuppliesDN);
        pickActionSN.AddChild(checkAndServeCustomersSN);

        LoopDecoratorNode mainInfiniteLoopDN = _merchantBT.CreateLoopNode("mainInfiniteLoopDN", pickActionSN);

        _merchantBT.SetRootNode(mainInfiniteLoopDN);

        #endregion
    }

    #region Checks
    private void DoNothing() { }
    private ReturnValues CheckForClients()
    {
        GameObject client = _targetDetector.DetectTargetGameObject();
        if (client != null)
        {
            client.GetComponent<IShop>().Shop(); // tell client it is being attended
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private ReturnValues CheckIfCounterEmpty()
    {
        if (_suppliesManager.IsThereMilkLeft() && _suppliesManager.IsThereWheatLeft())
        {
            // Counter ain't empty
            return ReturnValues.Failed;
        }
        else
        {
            return ReturnValues.Succeed;
        }
    }

    private ReturnValues CheckSupplies()
    {
        if (!_suppliesManager.IsThereMilkLeft() && !_suppliesManager.IsThereWheatLeft())
        {
            // the carrier has not delivered the supplies yet, still running
            return ReturnValues.Running;
        }
        else
        {
            // now we've got supplies, that means that they've been delivered, so we can return a succeed and go on with the code to serve some customers
            return ReturnValues.Succeed;
        }
    }

    #endregion

    #region Serve Customer
    private void ServeCustomer()
    {
        _animationsHandler.PlayAnimationState("Sell", 0.1f);
        _suppliesManager.GetOneMilk();
        _suppliesManager.GetOneWheat();
    }

    private ReturnValues ServedCustomer()
    {
        if (_animationsHandler.GetSellSuccesfully() == true)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion

    #region Talk to vendor
    private void TalkToCarrier()
    {
        _animationsHandler.PlayAnimationState("Talk", 0.1f);
    }

    private ReturnValues TalkedToCarrier()
    {
        if (_animationsHandler.GetTalkSuccesfully() == true)
        {
            _suppliesManager.Demand();
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion

    #region Walks
    private void WalkBackToShop()
    {
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Bar"));
    }
    private ReturnValues WalkedToShop()
    {
        if(_locator.IsCharacterInPlace(transform.position, "Bar"))
        {
            _animationsHandler.PlayAnimationState("Idle", 0.1f);
            _suppliesManager.SetIsMerchantInShop(true);
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    private void WalkToCarrier()
    {
        _suppliesManager.SetIsMerchantInShop(false);
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("CarrierPlace"));
    }
    private ReturnValues ArrivedToCarrier()
    {
        if (_locator.IsCharacterInPlace(transform.position, "CarrierPlace") == true)
        {
            _animationsHandler.PlayAnimationState("Idle", 0.1f);
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    #endregion

    #region Interact with supplies
    private void InteractWithSupplies()
    {
        //Debug.Log("interacting w supllies");
        _animationsHandler.PlayAnimationState("InteractWithSupplies", 0.1f);
    }

    private ReturnValues InteractedWithSupplies()
    {
        if (_animationsHandler.GetInteractWithSuppliesSuccesfully() == true)
        {
            //Debug.Log("interacted w supplies succesfully");
            return ReturnValues.Succeed;
        }
        else
        {
            //Debug.Log("still doing animation");
            return ReturnValues.Running;
        }
    }

    #endregion

}
