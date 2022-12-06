using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Police : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private CharacterConfigurationSO _configuration;
	private PoliceAnimationsHandler _animationsHandler;
	private Locator _locator;
	private MovementController _movementController;
	private NavMeshAgent _agent;
	private StateMachineEngine _policeFSM;
	private WaypointsController _waypointsController;

	private Vector3 _currentWaypoint;

	private int visionRange = 30;
	private int catchzone = 3;
	private bool _isPatrolling;
	private bool _thiefStealing = false;


	private void Awake()
	{

		_animationsHandler = new PoliceAnimationsHandler(_animator);
		_agent = GetComponent<NavMeshAgent>();
		_movementController = new MovementController(_agent, _configuration, Vector3.zero);
		_locator = FindObjectOfType<Locator>();

		//This should be the flags in the level for example (interessZones)
		_waypointsController = FindObjectOfType<WaypointsController>();
		CreateAI();
	}

	private void CreateAI()
	{
		_policeFSM = new StateMachineEngine();

		//PERCEPCIONES

		Perception patrollingCityPerception = _policeFSM.CreatePerception<ValuePerception>(patrollingCity);
		Perception watchedThiefPerception = _policeFSM.CreatePerception<ValuePerception>(haveWatchedTheThief);
		Perception catchedThiefPerception = _policeFSM.CreatePerception<ValuePerception>(haveCatchedTheThief);
		Perception lostThiefPerception = _policeFSM.CreatePerception<ValuePerception>(haveLostTheThief);
		Perception finishedKnockingPerception = _policeFSM.CreatePerception<ValuePerception>(haveFinishedKnocking);



		//ESTADOS

		State patrollingToPointState = _policeFSM.CreateEntryState("patrollingToPoint", PatrollingToPoint);
		State followingThiefState = _policeFSM.CreateEntryState("followThief", Following);
		State knockingThiefState = _policeFSM.CreateEntryState("knockThief", Knocking);

		//TRANSICIONES

		_policeFSM.CreateTransition("patrollingToPoint-followingThief", patrollingToPointState, watchedThiefPerception, followingThiefState);
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

		_policeFSM.Update();

		if (Input.GetKeyDown(KeyCode.B))
		{
			OnBeingUnconscious();
		}
		else if (Input.GetKeyDown(KeyCode.V))
		{
			OnBeingSeenByPolice();
		}
		else if (Input.GetKeyDown(KeyCode.L))
		{
			OnBeingLostByPolice();
		}
	}

	private void OnBeingUnconscious()
	{
		_policeFSM.Fire("knockingThief-patrolling");
	}

	private void OnBeingSeenByPolice()
	{
		_policeFSM.Fire("patrollingToPoint-followingThief");
	}

	private void OnBeingLostByPolice()
	{
		_policeFSM.Fire("followingThief-lostThief");
	}


	private bool patrollingCity()
    {
		return _isPatrolling;
    }


	private bool haveWatchedTheThief()
	{
		//Debug.Log("Le vi");
		if (_thiefStealing)
        {
            if (distance2points(transform.position, _locator.GetPlaceOfInterestPositionFromName("ladron")) < visionRange)
            {
				return true;
			}
		}
		return false;
	}

		private float distance2points(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b);
		}

	private bool haveCatchedTheThief()
    {
		//Debug.Log("le cogi");
		if (_thiefStealing && distance2points(transform.position, _locator.GetPlaceOfInterestPositionFromName("ladron")) < catchzone)		
			{
			return true;
		}
		else return false;
    }

	private bool haveLostTheThief()
	{
		//falta cuando ha terminado de robar
		//Debug.Log("he perdido");
		if ( (_thiefStealing && distance2points(transform.position, _locator.GetPlaceOfInterestPositionFromName("ladron")) > visionRange))
			{
			return true;
		}
		else return false;
	}

	private bool haveFinishedKnocking()
    {
		//Debug.Log("esta muerto");
		return _isPatrolling;
    }	

	private void PatrollingToPoint()
    {
		_isPatrolling = true;
		_thiefStealing = false;
		Debug.Log("patrolling to a point :3");
		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		_agent.speed = _configuration.MovementSpeed;
		//Get the thief info on position and stealing?


		MoveToRandomWaypoint();
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
			_currentWaypoint = _waypointsController.GetRandomWaypoint();
			_movementController.MoveToPosition(_currentWaypoint);
		}

	private void Following()
    {
		Debug.Log("Le estoy persiguiendo la cola");
		_isPatrolling = false;
		_thiefStealing = true;
		_animationsHandler.PlayAnimationState("Run", 0.1f);
		_movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("ladron"));
		float velocidad = Random.Range(_configuration.MovementSpeed + 4, _configuration.MovementSpeed * 3);
		Debug.Log("voy a " + velocidad + " m/h fiuuum");
		_agent.speed = velocidad;
    }

	private void Knocking()
    {
		_animationsHandler.PlayAnimationState("Punch", 0.1f);
		Debug.Log("Le hago la animacon de knockear");
		
		StartCoroutine(WaitTillReturn());
		Debug.Log("pum pum pum");
		
		_agent.speed = _configuration.MovementSpeed;
	}

		private IEnumerator WaitTillReturn() 
		{
		_agent.speed = 0;
		yield return new WaitForSeconds(2f);
		_thiefStealing = false;

		_isPatrolling = true;
		}



	/// <summary>
	/// /////////////
	/// </summary>
	/// <returns></returns>
}
