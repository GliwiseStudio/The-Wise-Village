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

    [SerializeField] private int _milk = 3;
    [SerializeField] private int _bread = 3;

    #endregion

    private void Awake()
    {
        _locator = FindObjectOfType<Locator>();
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        _movementController = new MovementController(navMeshAgent, _configuration, new Vector3(-71.9f, 1, -17));
        _animator = GetComponent<Animator>();
        
        _animationsHandler = new MerchantAnimationsHandler(_animator);
        _suppliesManager = FindObjectOfType<Supplies>();
        
        CreateAI();
    }

    private void Update()
    {
        _merchantBT.Update();
    }

    #region Listeners
    private void OnEnable()
    {
        _suppliesManager.OnDelivery += UpdateSuppliesInfo;
    }

    private void OnDisable()
    {
        _suppliesManager.OnDelivery -= UpdateSuppliesInfo;
    }

    #endregion

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

        LoopDecoratorNode waitForSuppliesDN = _merchantBT.CreateLoopNode("waitForSuppliesDN", interactWithSuppliesSN);

        // Sequence to get supplies
        LeafNode walkToCarrierLN = _merchantBT.CreateLeafNode("walkToCarrierLN", WalkToCarrier, ArrivedToCarrier);
        LeafNode talkToCarrierLN = _merchantBT.CreateLeafNode("talkToCarrierLN", TalkToCarrier, TalkedToCarrier);
        LeafNode walkBackToShopLN = _merchantBT.CreateLeafNode("walkBackToShopLN", WalkBackToShop, WalkedToShop);

        SequenceNode getSuppliesSN = _merchantBT.CreateSequenceNode("getSuppliesSN", false);

        getSuppliesSN.AddChild(walkToCarrierLN);
        getSuppliesSN.AddChild(talkToCarrierLN);
        getSuppliesSN.AddChild(walkBackToShopLN);
        getSuppliesSN.AddChild(waitForSuppliesDN);

        // CONCEPTUALMENTE ES UN SUCCEDER PERO WENO, NO LO HE METIDO PQ LITERALLY Q EL RESTO NO PUEDEN FALLAR

        // Sequence to check if we have supplies, and if not get them
        LeafNode isCounterEmptyLN = _merchantBT.CreateLeafNode("isCounterEmptyLN", DoNothing, CheckCounterState);

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

    private void UpdateSuppliesInfo() // on delivery event
    {
        _milk += _suppliesManager.GetMilk();
        _bread += _suppliesManager.GetWheat();
    }

    #region Checks
    private void DoNothing() { }
    private ReturnValues CheckForClients()
    {
        return ReturnValues.Succeed;
        // still have to implement this, suceed temporary just to see if it works
    }

    private ReturnValues CheckCounterState()
    {
        if (_milk == 0 || _bread == 0)
        {
            Debug.Log("no things");
            return ReturnValues.Succeed;
        }
        else
        {
            Debug.Log("has things");
            return ReturnValues.Failed;
        }
    }

    private ReturnValues CheckSupplies()
    {
        if (_milk == 0 || _bread == 0)
        {
            Debug.Log("Still no supplies");
            return ReturnValues.Failed;
        }
        else
        {
            Debug.Log("Gotten supplies");
            return ReturnValues.Succeed;
        }
    }

    #endregion

    #region Serve Customer
    private void ServeCustomer()
    {
        Debug.Log("serve customer");
        _animator.Play("Sell");
    }

    private ReturnValues ServedCustomer()
    {
        if (_animationsHandler.GetSellSuccesfully() == true)
        {
            _milk--;
            _bread--;
            // tell client it has been served, still to implement
            Debug.Log("served customer");
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
        _animator.Play("Talk");
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
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Shop"));
    }
    private ReturnValues WalkedToShop()
    {
        if(_locator.IsCharacterInPlace(transform.position, "Shop") == true)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    private void WalkToCarrier()
    {
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("CarrierPlace"));
    }
    private ReturnValues ArrivedToCarrier()
    {
        if (_locator.IsCharacterInPlace(transform.position, "CarrierPlace") == true)
        {
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
        Debug.Log("interacting w supllies");
        //_animator.Play("InteractWithSupplies");
    }

    private ReturnValues InteractedWithSupplies()
    {
        if (_animationsHandler.GetInteractWithSuppliesSuccesfully() == true)
        {
            Debug.Log("interacted w supplies succesfully");
            return ReturnValues.Succeed;
        }
        else
        {
            Debug.Log("still doing animation");
            return ReturnValues.Running;
        }
    }

    #endregion

}
