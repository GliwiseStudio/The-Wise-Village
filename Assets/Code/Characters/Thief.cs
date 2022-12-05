using UnityEngine;

public class Thief: MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private ThiefAnimationsHandler _animationsHandler;
    private Locator _locator;
	private StateMachineEngine steal = new StateMachineEngine(true);
	
    private void Awake()
    {
        _animationsHandler = new FarmerAnimationsHandler(_animator);
        _locator = FindObjectOfType<Locator>();
    }

    private void CreateAI()
    {
        
        //PERCEPCIONES
        ValuePerception stealStateActivePerception = steal.CreatePerception<ValuePerception>(StealStateActive);
		ValuePerception isInObjectivePerception = steal.CreatePerception<ValuePerception>(IsInObjective);
		ValuePerception hasStealAnimationFinishedPerception = steal.CreatePerception<ValuePerception>(HasStealAnimationFinished);
		ValuePerception isSeenByPolicePerception = steal.CreatePerception<ValuePerception>(IsSeenByPolice);
		ValuePerception notReachedOrSeenByPolicePerception = steal.CreatePerception<ValuePerception>(NotReachedOrSeenByPolice); //while thief is running away from police.
		ValuePerception policeReachedThiefPerception = steal.CreatePerception<ValuePerception>(PoliceReachedThiefPerception);
		ValuePerception hasUnconsciusAnimationFinishedPerception = steal.CreatePerception<ValuePerception>(UnconsciusAnimationFinishedPerception);
        

        //ESTADOS
		State walkState = steal.CreateEntryState("Walk", Walk);
		State moveToObjectiveState = steal.CreateState("MoveToObjective", MoveToObjective);
		State stealState = steal.CreateState("Steal", Steal);
		State runAwayPoliceState = steal.CreateState("RunAwayPolice", RunAwayPolice);
		State unconsciusState = steal.CreateState("Unconscius", Unconscius);

        //TRANSICIONES
		steal.CreateTransition("Walk-MoveToObjective", walkState, stealStateActivePerception, moveToObjectiveState);
		steal.CreateTransition("MoveToObjective-Steal", moveToObjectiveState, isInObjectivePerception, stealState);
		steal.CreateTransition("Steal-Walk", stealState, hasStealAnimationFinishedPerception, walkState);
		steal.CreateTransition("MoveToObjective-RunAwayPolice", moveToObjectiveState, isSeenByPolicePerception, runAwayPoliceState);
		steal.CreateTransition("RunAwayPolice-MoveToObjective", runAwayPoliceState, notReachedOrSeenByPolicePerception, moveToObjectiveState);
		steal.CreateTransition("RunAwayPolice-Unconscius", runAwayPoliceState, policeReachedThiefPerception, unconsciusState);
		steal.CreateTransition("Unconscius-Walk", unconsciusState, hasUnconsciusAnimationFinishedPerception, walkState);      
    }
	
	private ReturnValues StealStateActive(){
		return ReturnValues.Succed;
	}
	
	private ReturnValues IsInObjective(){
		return ReturnValues.Succed;
	}
	
	private bool HasStealAnimationFinished(){
		
	}
	
	private ReturnValues IsSeenByPolice(){
		ReturnValues.Succed;
	}
	
	private bool NotReachedOrSeenByPolice(){
		return true;
	}
		
	private ReturnValues PoliceReachedThiefPerception(){
		ReturnValues.Succed;
	}
	
	private bool UnconsciusAnimationFinishedPerception(){
		return true;
	}
	
	
	private void Walk(){
		
	}
	
	private void MoveToObjective(){
		
	}
	
	private void Steal(){
		
	}
	
	private void RunAwayPolice(){
		
	}
	
	private void Unconscius(){
		
	}
	}