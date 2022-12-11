using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : MonoBehaviour, IShop
{
    [SerializeField] CharacterConfigurationSO _configuration;
    [SerializeField] private float minRandomSeconds = 0;
    [SerializeField] private float maxRandomSeconds = 30;
    [SerializeField] private TextMeshProUGUI _text;

    private Animator _animator;
    private VillagerAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _villagerBT;
    private Locator _locator;
    private MovementController _movementController;
    private NavMeshAgent _navMeshAgent;
    private Vector3 _currentWaypoint;
    private WaypointsController _waypointsController;

    private bool _isHungry = false;
    private bool _isThirsty = false;
    private bool _hasBeenServed = false;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _animationsHandler = new VillagerAnimationsHandler(_animator);
        _locator = FindObjectOfType<Locator>();
        _waypointsController = FindObjectOfType<WaypointsController>();

        CreateAI();
    }

    private void Start()
    {
        _movementController = new MovementController(_navMeshAgent, _configuration, _locator.GetPlaceOfInterestPositionFromName("House"));
        StartCoroutine(GetThirsty());
        StartCoroutine(GetHungry());
    }

    private void Update()
    {
        _villagerBT.Update();
    }

    // Update is called once per frame
    private void CreateAI()
    {
        _villagerBT = new BehaviourTreeEngine();

        #region Is Hungry Sequence
        LeafNode isHungryLeafNode = _villagerBT.CreateLeafNode("IsHungry", DoNothing, IsHungry);
        LeafNode goToShopLeafNode = _villagerBT.CreateLeafNode("GoToShop", GoToShop, ArrivedToShop);
        LeafNode waitInLineLeafNode = _villagerBT.CreateLeafNode("WaitInLine", WaitInLine, Served);
        LeafNode shopLeafNode = _villagerBT.CreateLeafNode("Shop", GetProducts, GottenProducts);
        LeafNode goEatLeafNode = _villagerBT.CreateLeafNode("GoEat", GoToHouse, ArrivedToHouseWithFood);

        SequenceNode isHungrySequence = _villagerBT.CreateSequenceNode("isHungrySequence", false);
        isHungrySequence.AddChild(isHungryLeafNode);
        isHungrySequence.AddChild(goToShopLeafNode);
        isHungrySequence.AddChild(waitInLineLeafNode);
        isHungrySequence.AddChild(shopLeafNode);
        isHungrySequence.AddChild(goEatLeafNode);
        #endregion

        #region Is Thirsty Sequence
        LeafNode isThirstyLeafNode = _villagerBT.CreateLeafNode("IsThirsty", DoNothing, IsThirsty);
        LeafNode goToWellLeafNode = _villagerBT.CreateLeafNode("GoToWell", GoToWell, ArrivedToWell);
        LeafNode getWaterLeafNode = _villagerBT.CreateLeafNode("GetWater", GetWater, GottenWater);
        LeafNode goDrinkLeafNode = _villagerBT.CreateLeafNode("GoDrink", DrinkWater, DrankWater);

        SequenceNode isThirstySequence = _villagerBT.CreateSequenceNode("isThirstySequence", false);
        isThirstySequence.AddChild(isThirstyLeafNode);
        isThirstySequence.AddChild(goToWellLeafNode);
        isThirstySequence.AddChild(getWaterLeafNode);
        isThirstySequence.AddChild(goDrinkLeafNode);
        #endregion

        #region General Selector and infinite loop
        // walk leaf node
        LeafNode walkLeafNode = _villagerBT.CreateLeafNode("Walk", Walk, WalkCheck);

        //SELECTOR
        SelectorNode selector = _villagerBT.CreateSelectorNode("Selector");
        selector.AddChild(isHungrySequence);
        selector.AddChild(isThirstySequence);
        selector.AddChild(walkLeafNode);

        //LOOP INFINITO
        LoopDecoratorNode rootLoop = _villagerBT.CreateLoopNode("LoopInfinito", selector);
        _villagerBT.SetRootNode(rootLoop);
        #endregion
    }

    #region Checks
    private void DoNothing() {}

    private ReturnValues IsHungry()
    {
        if (_isHungry)
        {
            //Debug.Log("HUNGRY");
            return ReturnValues.Succeed;
        }
        else
        {
            //Debug.Log("NOT HUNGRY");
            return ReturnValues.Failed;
        }
        
    }
    private ReturnValues IsThirsty()
    {
        if (_isThirsty)
        {
            //Debug.Log("THIRSTY");
            return ReturnValues.Succeed;
        }
        else
        {
            //Debug.Log("NOT THIRSTY");
            return ReturnValues.Failed;
        }
    }
    #endregion

    #region Get water
    private void GetWater()
    {
        _text.text = "Getting water";
        _animationsHandler.PlayAnimationState("GetWater", 0.1f);
    }

    private ReturnValues GottenWater()
    {
        if (_animationsHandler.GetWaterSuccesfully())
        {
            //Debug.Log("done getting water");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    #endregion

    #region Get products from vendor
    private void GetProducts()
    {
        _text.text = "Getting products";
        _animationsHandler.PlayAnimationState("GetProducts", 0.1f);
    }

    private ReturnValues GottenProducts()
    {
        if (_animationsHandler.GetProductsSuccesfully())
        {
            //Debug.Log("Done getting products");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion

    #region Wait in line
    private void WaitInLine()
    {
        // the villager is now a client, because it is waiting in the line
        _text.text = "Waiting to shop";
        gameObject.layer = LayerMask.NameToLayer("Client");
    }

    private ReturnValues Served()
    {
        if (_hasBeenServed)
        {
            _movementController.ContinueMovement();
            _hasBeenServed = false; // reset for next time
            gameObject.layer = LayerMask.NameToLayer("Villager");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    #endregion
    
    #region Go to shop
    private void GoToShop()
    {
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Shop"));
    }

    private ReturnValues ArrivedToShop()
    {
        if (_locator.IsCharacterInPlace(transform.position, "Shop") == true)
        {
            _movementController.Stop();
            _animator.Play("Idle");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    #endregion

    #region Go to well
    private void GoToWell()
    {
        _text.text = "Going to well";
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Well"));
    }

    private ReturnValues ArrivedToWell()
    {
        if (_locator.IsCharacterInPlace(transform.position, "Well") == true)
        {
            _animator.Play("Idle");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    #endregion

    #region Drink water
    private void DrinkWater()
    {
        _text.text = "Drinking water";
        _animationsHandler.PlayAnimationState("Drink", 0.1f);
    }
    private ReturnValues DrankWater()
    {
        if (_animationsHandler.GetDrankSuccesfully())
        {
            //Debug.Log("Done drinking");
            _isThirsty = false;
            StartCoroutine(GetThirsty());
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion

    #region Go to house
    private void GoToHouse()
    {
        _text.text = "Going to house";
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("House"));
    }

    private ReturnValues ArrivedToHouseWithFood()
    {
        if (_locator.IsCharacterInPlace(transform.position, "House") == true)
        {
            _animator.Play("Idle");
            _isHungry = false;
            StartCoroutine(GetHungry());
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion

    #region Walk randomly
    private void Walk()
    {
        _text.text = "Going for a walk";
        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        MoveToRandomWaypoint();
    }

    private void MoveToRandomWaypoint()
    {
        _currentWaypoint = _waypointsController.GetRandomWaypoint("Villager");
        _movementController.MoveToPosition(_currentWaypoint);
    }

    private ReturnValues WalkCheck()
    {
        if (_waypointsController.IsCharacterInPlace(transform.position, _currentWaypoint))
        {
            _animator.Play("Idle");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion

    public void Shop()
    {
        _hasBeenServed = true;
    }

    IEnumerator GetHungry()
    {
        yield return new WaitForSeconds(Random.Range(minRandomSeconds, maxRandomSeconds));
        _isHungry = true;
    }

    IEnumerator GetThirsty()
    {
        yield return new WaitForSeconds(Random.Range(minRandomSeconds, maxRandomSeconds));
        _isThirsty = true;
    }

}

