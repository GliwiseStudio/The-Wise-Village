using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ThiefSU : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private CharacterConfigurationSO _configuration;
	private ThiefAnimationsHandler _animationsHandler;
	private Locator _locator;
	private UtilitySystemEngine _thiefSU;
	private StateMachineEngine _thiefFSM;
	private BehaviourTreeEngine _thiefBT;
	private MovementController _movementController;
	private NavMeshAgent _agent;
	private WaypointsController _waypointsController;
	private Supplies _supplies;

	private float _timeWithoutEating = 0f;
	private float _maximumTimeWithoutEating = 30f;
	private bool _hasBeenSeenByThePolice = false;
	private bool _hasBeenKnockedDownByPolice = false;
	private bool _isMerchantClose = false;
	private bool _isPoliceClose = false;
	private TargetDetector _targetDetector;
	private Vector3 _currentWaypoint = Vector3.zero;

	private bool _isPatrolling = false;
	private bool _isEscaping = false;
	private bool _updateIsInShop = false;
	private bool _updateHasStealed = false;

	private void Awake()
	{
		_targetDetector = new TargetDetector(transform, 10f, "Police");
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

		//Factor en persecución
		Factor pursue = new LeafVariable(() => (_hasBeenSeenByThePolice) ? 1f : 0f, 1f, 0f);

		//Factor vendedor cerca
		Factor isMerchantClose = new LeafVariable(() => (_isMerchantClose) ? 0f : 1f, 1f, 0f);

		//Factor policía cerca
		Factor isPoliceClose = new LeafVariable(() => (_isPoliceClose) ? 0f : 1f, 1f, 0f);

		//Factor peligro
		Factor danger = new MinFusion(new List<Factor> {isMerchantClose, isPoliceClose});

		//Factor ganas de robar
		WeightedSumFusion stealingDesire = new WeightedSumFusion(new List<Factor> { hunger, danger}, new List<float> { 0.7f, 0.3f });

		//Factor ganas de robarn't
		Factor stealingDesirent = new InvertWeightFactor(stealingDesire);


        // MAIN UTILITY
        _thiefSU.CreateUtilityAction("Steal", Steal, stealingDesire);
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

		_timeWithoutEating += Time.deltaTime;
		if (_timeWithoutEating >= _maximumTimeWithoutEating)
        {
			_timeWithoutEating = _maximumTimeWithoutEating;
        }

        if (!_hasBeenKnockedDownByPolice)
        {
            _thiefSU.Update();
        }

    }

	#region Patrol // Escape
	private void Patrol()
	{
		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		Debug.Log("Thief: Patrol");
		_isEscaping = false;
		_isPatrolling = true;
		_agent.speed = _configuration.MovementSpeed;
		MoveToCurrentWaypoint();
	}

	private void Escape()
	{
		_animationsHandler.PlayAnimationState("Run", 0.1f);
		Debug.Log("Thief: Me voy");
		_isEscaping = true;
		_isPatrolling = false;
		_agent.speed *= 2;
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
    private void GoToShop()
    {
		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		_movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Shop"));
		_isPatrolling = false;
		_isEscaping = false;
		_updateIsInShop = true;
	}

	private bool IsInShop()
	{
		if (_locator.IsCharacterInPlace(transform.position, "Shop"))
		{
			_updateIsInShop = false;
			_updateHasStealed = true;
			Debug.Log("Is in shop");
			return true;
        }
        else
        {
			return false;
        }
	}

	private void Steal()
    {
		Debug.Log("IS STEALING");
		_animationsHandler.PlayAnimationState("Steal", 0.1f);
		_movementController.Stop();
		
		gameObject.layer = LayerMask.NameToLayer("Thief");
		// empty supplies
		int milk = _supplies.GetMilk();
		int wheat = _supplies.GetWheat();
		Debug.Log("milk: " + milk + " whate: " + wheat);

		if (milk != 0 || wheat != 0)
		{
			Debug.Log("thief: ate");
			_timeWithoutEating = 0;
		}
	}
	

	private bool HasStealed()
    {
		if (_animationsHandler.GetStealSuccesfully())
		{
			gameObject.layer = LayerMask.NameToLayer("Villager");
			_updateHasStealed = false;
			Debug.Log("stealed succesfully");
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

    private void SetIsMerchantInShop(bool status)
    {
		_isMerchantClose = status;
	}

	public void IsBeingSeenByPolice(bool hasBeenSeen)
	{
		_hasBeenSeenByThePolice = hasBeenSeen;
	}

	public void HasBeenCaught()
    {
		Debug.Log("thief: HAS BEEN CAUGHT");

		_movementController.Stop();

		_hasBeenKnockedDownByPolice = true;

		_isEscaping = false;

		// SHOULD PLAY ANIMATION

		StartCoroutine(KnockedDown());
	}


	IEnumerator KnockedDown()
    {
		yield return new WaitForSeconds(1);
		Debug.Log("thief: back to business");
		_hasBeenSeenByThePolice = false;
		_hasBeenKnockedDownByPolice = false;
	}
}
