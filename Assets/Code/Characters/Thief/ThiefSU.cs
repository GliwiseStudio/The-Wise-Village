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

	private void Awake()
	{
		_targetDetector = new TargetDetector(transform, 3f, "Police");
		_animationsHandler = new ThiefAnimationsHandler(_animator);
		_agent = GetComponent<NavMeshAgent>();
		_movementController = new MovementController(_agent, _configuration, Vector3.zero);
		_locator = FindObjectOfType<Locator>();
		_waypointsController = FindObjectOfType<WaypointsController>();
		_supplies = FindObjectOfType<Supplies>();
		CreateAI();
	}

	private void CreateAI()
	{
		_thiefSU = new UtilitySystemEngine(false, 1f);

		//Factor Hambre
		Factor hunger = new LeafVariable(() => _timeWithoutEating, _maximumTimeWithoutEating, 0f);
		List<Point2D> points = new List<Point2D>();
		points.Add(new Point2D(0, 0));
		points.Add(new Point2D(0.2f, 0.8f));
		points.Add(new Point2D(1, 1));
		Factor hungerParts = new LinearPartsCurve(hunger, points);

		//Factor en persecución
		Factor pursue = new LeafVariable(() => (_hasBeenSeenByThePolice) ? 1f : 0f, 1f, 0f);

		//Factor vendedor cerca
		Factor isMerchantClose = new LeafVariable(() => (_isMerchantClose) ? 1f : 0f, 1f, 0f);

		//Factor policía cerca
		Factor isPoliceClose = new LeafVariable(() => (_isPoliceClose) ? 1f : 0f, 1f, 0f);

		//Factor peligro
		Factor danger = new MinFusion(new List<Factor> {isMerchantClose, isPoliceClose});

		//Factor ganas de robar
		WeightedSumFusion stealingDesire = new WeightedSumFusion(new List<Factor> { hunger, danger}, new List<float> { 0.7f, 0.3f });

		//Factor ganas de robarn't
		Factor stealingDesirent = new InvertWeightFactor(stealingDesire);

		_thiefSU.CreateUtilityAction("Steal", Steal, stealingDesire);
		_thiefSU.CreateUtilityAction("Patrol", Patrol, stealingDesirent);
		_thiefSU.CreateUtilityAction("Escape", Escape, pursue);
	}

	private void Steal()
    {
		Debug.Log("Steal");
    }

	private void Patrol()
    {
		Debug.Log("Patrol");
		_isPatrolling = true;
		MoveToCurrentWaypoint();
	}

	private void MoveToCurrentWaypoint()
	{
		_currentWaypoint = _waypointsController.GetRandomWaypoint("Thief");
		_movementController.MoveToPosition(_currentWaypoint);
	}

	private void Escape()
    {
		Debug.Log("Me voy");
    }

    private void Update()
    {
		DetectEnemies();

		if(_isPatrolling)
        {
			CheckPatrol();
        }


		_timeWithoutEating += Time.deltaTime;
		if(_timeWithoutEating >= _maximumTimeWithoutEating)
        {
			_timeWithoutEating = _maximumTimeWithoutEating;
        }

		_thiefSU.Update();
    }

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

	private void CheckPatrol()
    {
		if(_waypointsController.IsCharacterInPlace(transform.position, _currentWaypoint))
        {
			MoveToCurrentWaypoint();
		}
    }

	private void OnEnable()
	{
		_supplies.OnMerchantIsInShop += SetIsMerchantInShop;
	}

	private void OnDisable()
	{
		_supplies.OnMerchantIsInShop -= SetIsMerchantInShop;
	}

	private void SetIsMerchantInShop(bool status)
    {
		_isMerchantClose = status;
	}

	public void IsBeingSeenByPolice(bool hasBeenSeen)
	{
		_hasBeenSeenByThePolice = hasBeenSeen;
	}

	public void HasBeenKnockedDown()
    {
		_hasBeenKnockedDownByPolice = true;
    }
}
