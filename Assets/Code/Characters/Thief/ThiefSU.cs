using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ThiefSU : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private CharacterConfigurationSO _configuration;
	[SerializeField] private float _detectionRange = 5f;
	[SerializeField] private Transform _shopTransformLookAt;
	[SerializeField] private TextMeshProUGUI _text;
	private ThiefAnimationsHandler _animationsHandler;
	private Locator _locator;
	private UtilitySystemEngine _thiefSU;
	private StateMachineEngine _thiefSM;
	private MovementController _movementController;
	private NavMeshAgent _agent;
	private WaypointsController _waypointsController;
	private Supplies _supplies;

	private float _timeWithoutEating = 0f;
	[SerializeField] private float _maximumTimeWithoutEating = 30f;
	private bool _hasBeenSeenByThePolice = false;
	private bool _hasBeenKnockedDownByPolice = false;
	private bool _isMerchantClose = false;
	private bool _isPoliceClose = false;
	private TargetDetector _targetDetector;
	private Vector3 _currentWaypoint = Vector3.zero;

	private bool _isPatrolling = false;
	private bool _isEscaping = false;
	private bool _isStealing = false;

	private void Awake()
	{
		_targetDetector = new TargetDetector(transform, _detectionRange, "Police");
		_animationsHandler = new ThiefAnimationsHandler(_animator);
		_agent = GetComponent<NavMeshAgent>();
		_movementController = new MovementController(_agent, _configuration, new Vector3(0, 1.6f, 0));
		_locator = FindObjectOfType<Locator>();
		_waypointsController = FindObjectOfType<WaypointsController>();
		_supplies = FindObjectOfType<Supplies>();
		CreateAI();
	}

	private void OnEnable()
	{
		_supplies.OnMerchantIsInShop += SetIsMerchantInShop;
	}

	private void OnDisable()
	{
		_supplies.OnMerchantIsInShop -= SetIsMerchantInShop;
	}

	private void CreateAI()
	{
		_thiefSU = new UtilitySystemEngine(false, 1f);

		//Factor Hambre
		Factor hunger = new LeafVariable(() => _timeWithoutEating, _maximumTimeWithoutEating, 0f);
		List<Point2D> points = new List<Point2D>();
		points.Add(new Point2D(0, 0));
		points.Add(new Point2D(0.5f, 0.8f));
		points.Add(new Point2D(1, 1));
		Factor hungerParts = new LinearPartsCurve(hunger, points);

		//Factor en persecuci?n
		Factor pursue = new LeafVariable(() => (_hasBeenSeenByThePolice) ? 1f : 0f, 1f, 0f);

		//Factor vendedor cerca
		Factor isMerchantClose = new LeafVariable(() => (_isMerchantClose) ? 0f : 1f, 1f, 0f);

		//Factor polic?a cerca
		Factor isPoliceClose = new LeafVariable(() => (_isPoliceClose) ? 0f : 1f, 1f, 0f);

		//Factor peligro
		Factor danger = new MinFusion(new List<Factor> {isMerchantClose, isPoliceClose});

		//Factor ganas de robar
		WeightedSumFusion stealingDesire = new WeightedSumFusion(new List<Factor> { hunger, danger}, new List<float> { 0.7f, 0.3f });

		//Factor ganas de robarn't
		Factor stealingDesirent = new InvertWeightFactor(stealingDesire);

		

		//Crear FSM
		_thiefSM = new StateMachineEngine();

		Perception firstPerception = _thiefSM.CreatePerception<ValuePerception>(()=> true);
		Perception inShopPerception = _thiefSM.CreatePerception<ValuePerception>(IsInShop);
		Perception stealedPerception = _thiefSM.CreatePerception<ValuePerception>(HasStealed);

		State first = _thiefSM.CreateEntryState("first", GoToShop);
		State goToShopState = _thiefSM.CreateState("goToShopState", GoToShop);
		State stealState = _thiefSM.CreateState("stealState", Steal);

		_thiefSM.CreateTransition("first-shop", first, firstPerception, goToShopState);
		_thiefSM.CreateTransition("shop-steal", goToShopState, inShopPerception, stealState);
		_thiefSM.CreateTransition("steal-exit", stealState, stealedPerception, first);

		_thiefSU.CreateUtilityAction("Steal", StealAction, stealingDesire);
		_thiefSU.CreateUtilityAction("Patrol", Patrol, stealingDesirent);
		_thiefSU.CreateUtilityAction("Escape", Escape, pursue);
	}

	

    private void Update()
    {
		DetectEnemies();

		if(_isPatrolling || _isEscaping)
        {
			CheckIfItHasArrivedToPoint();
        }

        if (!_hasBeenKnockedDownByPolice)
        {
			_timeWithoutEating += Time.deltaTime;
			if (_timeWithoutEating >= _maximumTimeWithoutEating)
			{
				_timeWithoutEating = _maximumTimeWithoutEating;
			}

			if (!_isStealing)
			{
				_thiefSU.Update();
			}
			else
			{
				_thiefSM.Update();
			}
        }

    }

	#region Patrol // Escape
	private void Patrol()
	{
		_text.text = "Patrolling the shop";
		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		_isEscaping = false;
		_isPatrolling = true;
		_agent.speed = _configuration.MovementSpeed;
		MoveToCurrentWaypoint();
	}

	private void Escape()
	{
		_text.text = "Running away";
		_animationsHandler.PlayAnimationState("Run", 0.1f);
		_isEscaping = true;
		_isPatrolling = false;
		_agent.speed *= 2.5f;
		MoveToCurrentWaypoint();
	}

	private void MoveToCurrentWaypoint()
	{
		if (_isEscaping)
        {
			_currentWaypoint = _waypointsController.GetRandomWaypoint("ThiefEscaping");
		}
        else{
			_currentWaypoint = _waypointsController.GetRandomWaypoint("Thief");
		}
		
		_movementController.MoveToPosition(_currentWaypoint);
	}

	private void CheckIfItHasArrivedToPoint()
	{
		if (_waypointsController.IsCharacterInPlace(transform.position, _currentWaypoint))
		{
			MoveToCurrentWaypoint();
		}
	}

	#endregion

	#region Steal
	private void StealAction() // to go to the submachine (because it isn't really a submachine)
	{
		_text.text = "Going to steal hehe";
		_thiefSM.Reset();
		_isStealing = true;
	}

	private void GoToShop()
    {
		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		_movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("ShopThief"));
		_isPatrolling = false;
		_isEscaping = false;
	}

	private bool IsInShop()
	{
		if (_locator.IsCharacterInPlace(transform.position, "ShopThief"))
		{
			transform.LookAt(_shopTransformLookAt, Vector3.up);
			return true;
        }
        else
        {
			return false;
        }
	}

	private void Steal()
    {
		_animationsHandler.PlayAnimationState("Steal", 0.1f);
		_movementController.Stop();
		
		gameObject.layer = LayerMask.NameToLayer("Thief");

		// steal one supply of each (if there are any)
        if (_supplies.IsThereMilkLeft())
        {
			_supplies.GetOneMilk();
        }
        if (_supplies.IsThereWheatLeft())
        {
			_supplies.GetOneWheat();
        }

		// reset time without eating (even if it hasn't tried to steal)
		_timeWithoutEating = 0;
	}
	

	private bool HasStealed()
    {
		Debug.Log("Bang");
		if (_animationsHandler.GetStealSuccesfully())
		{
			gameObject.layer = LayerMask.NameToLayer("Villager");
			_movementController.ContinueMovement();
			Debug.Log("stealed succesfully");
			_isStealing = false;
			return true;
        }
        else
        {
			return false;
        }
	}

	#endregion

	#region Detect enemies

	private void DetectEnemies()
    {
		if(_targetDetector.DetectTarget() != null)
        {
			_isPoliceClose = true;
        }
		else
        {
			_isPoliceClose = false;
		}
    }

    #endregion

    #region Recieved changes from other characters
    private void SetIsMerchantInShop(bool status)
    {
		_isMerchantClose = status;
	}

	public void IsBeingSeenByPolice(bool hasBeenSeen)
	{
		_hasBeenSeenByThePolice = hasBeenSeen;
	}

    #endregion

    #region Being caught/defeated
    public void HasBeenCaught()
    {
		_text.text = "I've been caught :(";

		gameObject.layer = LayerMask.NameToLayer("Villager");
		_movementController.Stop();

		_hasBeenKnockedDownByPolice = true;

		_isEscaping = false;

		// SHOULD PLAY ANIMATION
		_animationsHandler.PlayAnimationState("Unconscious", 0.1f);
		StartCoroutine(KnockedDown());
	}

	IEnumerator KnockedDown()
    {
		yield return new WaitForSeconds(5);
		Debug.Log("thief: back to business");
		_hasBeenSeenByThePolice = false;
		_hasBeenKnockedDownByPolice = false;
		_movementController.ContinueMovement();
	}
    #endregion
}
