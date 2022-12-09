using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Police : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private CharacterConfigurationSO _configuration;
	[SerializeField] private float _detectionRange;

	private PoliceAnimationsHandler _animationsHandler;
	private Locator _locator;
	private MovementController _movementController;
	private NavMeshAgent _agent;
	private StateMachineEngine _policeFSM;
	private WaypointsController _waypointsController;

	private TargetDetector _targetDetector;
	private GameObject _thiefGameObject;
	private Transform _thiefTransform;

	private Vector3 _currentWaypoint;

	private bool _isPatrolling = false;
	private bool _followingThief = false;


	private void Awake()
	{
		_animationsHandler = new PoliceAnimationsHandler(_animator);
		_agent = GetComponent<NavMeshAgent>();
		
		_locator = FindObjectOfType<Locator>();
		_targetDetector = new TargetDetector(transform, _detectionRange, "Thief");

		_waypointsController = FindObjectOfType<WaypointsController>();
	}

    private void Start()
    {
		_movementController = new MovementController(_agent, _configuration, _locator.GetPlaceOfInterestPositionFromName("Well"));
		CreateAI();
	}

    private void CreateAI()
	{
		_policeFSM = new StateMachineEngine();

		//PERCEPCIONES
		Perception seenThiefPerception = _policeFSM.CreatePerception<ValuePerception>(SeenThief);
		Perception catchedThiefPerception = _policeFSM.CreatePerception<ValuePerception>(HaveCatchedTheThief);
		Perception lostThiefPerception = _policeFSM.CreatePerception<ValuePerception>(HaveLostThief);
		Perception finishedKnockingPerception = _policeFSM.CreatePerception<ValuePerception>(HaveFinishedKnocking);

		//ESTADOS
		State patrollingToPointState = _policeFSM.CreateEntryState("patrollingToPoint", PatrollingToPoint);
		State followingThiefState = _policeFSM.CreateState("followThief", FollowThief);
		State knockingThiefState = _policeFSM.CreateState("knockThief", Knocking);

		//TRANSICIONES
		_policeFSM.CreateTransition("patrollingToPoint-followingThief", patrollingToPointState, seenThiefPerception, followingThiefState);
		_policeFSM.CreateTransition("followingThief-knockingThief", followingThiefState, catchedThiefPerception, knockingThiefState);
		_policeFSM.CreateTransition("followingThief-lostThief", followingThiefState, lostThiefPerception, patrollingToPointState);
		_policeFSM.CreateTransition("knockingThief-patrolling", knockingThiefState, finishedKnockingPerception, patrollingToPointState);
	
	}

	private void Update()
	{
        if (_isPatrolling)
        {
            isCharacterInCurrentWaypoint();
        }
        else if (_followingThief)
        {
            GoToThief();
        }

        _policeFSM.Update();

	}

	#region Patrol related methods
	private void PatrollingToPoint()
	{
		Debug.Log("PATROLLING TO A POINT");

        _animationsHandler.PlayAnimationState("Walk", 0.1f);
        _agent.speed = _configuration.MovementSpeed;

        MoveToRandomWaypoint();
        _isPatrolling = true;
    }

    private void isCharacterInCurrentWaypoint()
    {
        if (_waypointsController.IsCharacterInPlace(transform.position, _currentWaypoint))
        {
            MoveToRandomWaypoint();
        }
    }

    private void MoveToRandomWaypoint()
    {
        _currentWaypoint = _waypointsController.GetRandomWaypoint("Police");
        _movementController.MoveToPosition(_currentWaypoint);
    }
    #endregion

    #region See thief
    private bool SeenThief()
	{
		if(_targetDetector.DetectTarget() != null)
        {
			Debug.Log("SEEN THIEF");

			_thiefGameObject = _targetDetector.DetectTargetGameObject();
			_thiefGameObject.GetComponent<ThiefSU>().IsBeingSeenByPolice(true);

			_isPatrolling = false; // stops patrolling
			return true;
        }
        else
        {
			return false;
        }
	}
	#endregion

	#region KnockThief
	private bool HaveFinishedKnocking()
	{
		if (_animationsHandler.GetPunchSuccesfully())
        {
			_thiefGameObject.GetComponent<ThiefSU>().IsBeingSeenByPolice(false);
			_thiefGameObject = null;

			return true;
        }
        else
        {
			return false;
        }
	}

	private void Knocking()
	{
		_animationsHandler.PlayAnimationState("Punch", 0.1f);
		Debug.Log("Le hago la animacon de knockear");

		_agent.speed = _configuration.MovementSpeed;

	}

    #endregion

    #region Catch / Loose thief
    private bool HaveCatchedTheThief()
    {
		if (Vector3.Distance(_thiefGameObject.transform.position, transform.position) < 0.1f) 
		{
			_followingThief = false;
			return true;
		}
		else return false;
    }

	private bool HaveLostThief()
	{
        if (_targetDetector.IsTargetInRange(_thiefGameObject.transform.position))
        {
			return false;
        }
        else
        {
			_thiefGameObject.GetComponent<ThiefSU>().IsBeingSeenByPolice(false);

			_followingThief = false;
			_thiefGameObject = null;

			return true;
        }
	}
    #endregion

    #region Follow thief
    private void FollowThief()
	{
		Debug.Log("FOLLOWING THIEF");
		_followingThief = true;

		_agent.SetDestination(_thiefTransform.position);

		_animationsHandler.PlayAnimationState("Run", 0.1f);

		_agent.speed *= 2;
	}

	private void GoToThief()
	{
		_agent.SetDestination(_thiefTransform.position);
	}

	#endregion

}
