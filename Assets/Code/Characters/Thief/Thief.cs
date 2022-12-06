using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Thief : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private CharacterConfigurationSO _configuration;
	private ThiefAnimationsHanlder _animationsHandler;
	private Locator _locator;
	private StateMachineEngine _stealFSM;
	private MovementController _movementController;
	private NavMeshAgent _agent;
	private WaypointsController _waypointsController;

	private Vector3 _currentWaypoint;
	private bool _isStealing = false;
	private bool _isMovingToRandomWaypoints = false;
	private bool _isUnconscious = false;

	private void Awake()
	{
		_animationsHandler = new ThiefAnimationsHanlder(_animator);
		_agent = GetComponent<NavMeshAgent>();
		_movementController = new MovementController(_agent, _configuration, Vector3.zero);
		_locator = FindObjectOfType<Locator>();
		_waypointsController = FindObjectOfType<WaypointsController>();
		CreateAI();
	}

	private void CreateAI()
	{
		_stealFSM = new StateMachineEngine();

		//PERCEPCIONES
		Perception stealStateActivePerception = _stealFSM.CreatePerception<ValuePerception>(StealStateActive);
		Perception isInShopPerception = _stealFSM.CreatePerception<ValuePerception>(IsInShop);
		Perception hasFinishedStealingPerception = _stealFSM.CreatePerception<ValuePerception>(HasFinishedStealing);
		Perception isSeenByPolicePerception = _stealFSM.CreatePerception<PushPerception>();
		Perception lostByPolicePerception = _stealFSM.CreatePerception<PushPerception>();
		Perception policeReachedThiefPerception = _stealFSM.CreatePerception<PushPerception>();
		Perception hasFinishedBeingUnconsciousPerception = _stealFSM.CreatePerception<ValuePerception>(hasFinishedBeingUnconscious);


		//ESTADOS
		State walkState = _stealFSM.CreateEntryState("Walk", Walk);
		State moveToObjectiveState = _stealFSM.CreateState("MoveToObjective", MoveToObjective);
		State stealState = _stealFSM.CreateState("Steal", Steal);
		State runAwayPoliceState = _stealFSM.CreateState("RunAwayPolice", RunAwayPolice);
		State unconsciusState = _stealFSM.CreateState("Unconscius", Unconscius);

		//TRANSICIONES
		_stealFSM.CreateTransition("Walk-MoveToObjective", walkState, stealStateActivePerception, moveToObjectiveState);
		_stealFSM.CreateTransition("MoveToObjective-Steal", moveToObjectiveState, isInShopPerception, stealState);
		_stealFSM.CreateTransition("Steal-Walk", stealState, hasFinishedStealingPerception, walkState);
		_stealFSM.CreateTransition("MoveToObjective-RunAwayPolice", moveToObjectiveState, isSeenByPolicePerception, runAwayPoliceState);
		_stealFSM.CreateTransition("RunAwayPolice-MoveToObjective", runAwayPoliceState, lostByPolicePerception, moveToObjectiveState);
		_stealFSM.CreateTransition("RunAwayPolice-Unconscius", runAwayPoliceState, policeReachedThiefPerception, unconsciusState);
		_stealFSM.CreateTransition("Unconscius-Walk", unconsciusState, hasFinishedBeingUnconsciousPerception, walkState);
	}

    private void Update()
    {
		if(_isMovingToRandomWaypoints)
        {
			isCharacterInCurrentWaypoint();
		}

		_stealFSM.Update();

		if(Input.GetKeyDown(KeyCode.V))
        {
			OnBeingSeenByPolice();
        }
		else if (Input.GetKeyDown(KeyCode.I))
		{
			OnBeingUnconscious();
		}
		else if (Input.GetKeyDown(KeyCode.P))
		{
			OnBeingLostByPolice();
		}
	}

	private void OnBeingUnconscious()
    {
		_stealFSM.Fire("RunAwayPolice-Unconscius");
    }

	private void OnBeingSeenByPolice()
    {
		_stealFSM.Fire("MoveToObjective-RunAwayPolice");
    }

	private void OnBeingLostByPolice()
    {
		_stealFSM.Fire("RunAwayPolice-MoveToObjective");
	}

    private bool StealStateActive()
	{
		return _isStealing;
	}

	private IEnumerator StealingStateActive()
	{
		yield return new WaitForSeconds(10f);
		_isStealing = true;
	}

	private bool IsInShop()
	{
		return _locator.IsCharacterInPlace(transform.position ,"Shop");
	}

	private bool HasFinishedStealing()
	{
		return !_isStealing;
	}

	private bool hasFinishedBeingUnconscious()
	{
		return !_isUnconscious;
	}

	private void isCharacterInCurrentWaypoint()
    {
		if(_waypointsController.IsCharacterInPlace(transform.position, _currentWaypoint))
        {
			MoveToRandomWaypoint();
        }
    }

	private void Walk()
	{
		Debug.Log("Walking");
		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		StartCoroutine(StealingStateActive());
		_isMovingToRandomWaypoints = true;
		MoveToRandomWaypoint();
	}

	private void MoveToRandomWaypoint()
	{
		_currentWaypoint = _waypointsController.GetRandomWaypoint();
		_movementController.MoveToPosition(_currentWaypoint);
	}

	private void MoveToObjective()
	{
		Debug.Log("Moving to steal to shop");

		_animationsHandler.PlayAnimationState("Walk", 0.1f);
		_isMovingToRandomWaypoints = false;
		_agent.speed = _configuration.MovementSpeed;
		_movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Shop"));
	}

	private void Steal()
	{
		Debug.Log("Stealing");
		StartCoroutine(StealingCycle());
	}

	private IEnumerator StealingCycle()
    {
		yield return new WaitForSeconds(4f);
		_isStealing = false;
    }

	private void RunAwayPolice()
	{
		Debug.Log("Running away from police");

		_animationsHandler.PlayAnimationState("Run", 0.1f);
		_isStealing = false;
		_isMovingToRandomWaypoints = true;
		_agent.speed = 16;
		MoveToRandomWaypoint();
	}

	private void Unconscius()
	{
		Debug.Log("Being Unconscious");
		StartCoroutine(BeingUnconsciousCycle());

		_animationsHandler.PlayAnimationState("Unconscious", 0.1f);
		_isUnconscious = true;
		_agent.speed = _configuration.MovementSpeed;
		_movementController.Stop();
		_isMovingToRandomWaypoints = false;
	}

	private IEnumerator BeingUnconsciousCycle()
    {
		yield return new WaitForSeconds(5f);
		_isUnconscious = false;
    }
}
