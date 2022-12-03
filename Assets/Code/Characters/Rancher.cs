using UnityEngine;

public class Rancher : MonoBehaviour
{
    private BehaviourTreeEngine _rancherBT;

    private void CreateAI()
    {
        _rancherBT = new BehaviourTreeEngine();

        //SECUENCIA 1
        LeafNode isInBarnLeafNode = _rancherBT.CreateLeafNode("IsInBarn", null, IsInBarn);
        LeafNode areCowsHungryLeafNode = _rancherBT.CreateLeafNode("AreCowsHungry", null, AreCowsHungry);

        StateMachineEngine feedCowsMachineState = new StateMachineEngine(true);

        //PERCEPCIONES
        ValuePerception areCowsInFeeders = feedCowsMachineState.CreatePerception<ValuePerception>(AreCowsInFeedersPerception);
        ValuePerception isFoodCollected = feedCowsMachineState.CreatePerception<ValuePerception>(IsFoodCollectedPerception);
        ValuePerception isInBarn = feedCowsMachineState.CreatePerception<ValuePerception>(IsInBarnPerception);
        ValuePerception areCowsFed = feedCowsMachineState.CreatePerception<ValuePerception>(AreCowsFedPerception);

        //ESTADOS
        State moveToFeederState = feedCowsMachineState.CreateEntryState("MoveToFeeders", MoveToFeeders);
        State collectFoodState = feedCowsMachineState.CreateState("CollectFood", CollectFood);
        State moveToBarnState = feedCowsMachineState.CreateState("MoveToBarn", MoveToBarn);
        State feedCowsState = feedCowsMachineState.CreateState("FeedCows", FeedCows);

        //TRANSICIONES
        feedCowsMachineState.CreateTransition("MoveToFeeders-CollectFood", moveToFeederState, areCowsInFeeders, collectFoodState);
        feedCowsMachineState.CreateTransition("CollectFood-MoveToBarn", collectFoodState, isFoodCollected, moveToBarnState);
        feedCowsMachineState.CreateTransition("MoveToBarn-FeedCows", moveToBarnState, isInBarn, feedCowsState);
        feedCowsMachineState.CreateExitTransition("FeedCows-Exit", feedCowsState, areCowsFed, ReturnValues.Succeed);

        LeafNode feedSubmachineLeafNode = _rancherBT.CreateSubBehaviour("FeedCowsProcess", feedCowsMachineState);
        SucceederDecoratorNode feedCowsSucceeder = _rancherBT.CreateSucceederNode("FeedCowsSucceeder", feedSubmachineLeafNode);

        SequenceNode feedSequence = _rancherBT.CreateSequenceNode("FeedSequence", false);
        feedSequence.AddChild(isInBarnLeafNode);
        feedSequence.AddChild(areCowsHungryLeafNode);
        feedSequence.AddChild(feedCowsSucceeder);

        //SECUENCIA 2
        LeafNode areCowsWithMilkLeafNode = _rancherBT.CreateLeafNode("AreCowsWithMilk", null, AreCowsWithMilk);

        LeafNode milkCowsLeafNode = _rancherBT.CreateLeafNode("MilkCows", MilkCows, MilkCowsCheck);
        SucceederDecoratorNode milkCowsSucceeder = _rancherBT.CreateSucceederNode("MilkCowsSucceeder", milkCowsLeafNode);

        SequenceNode milkCowsSequence = _rancherBT.CreateSequenceNode("MilkCowsSequence", false);
        milkCowsSequence.AddChild(isInBarnLeafNode);
        milkCowsSequence.AddChild(areCowsWithMilkLeafNode);
        milkCowsSequence.AddChild(milkCowsSucceeder);

        //SECUENCIA 3

        //SELECTOR
    }

    private ReturnValues IsInBarn()
    {
        return ReturnValues.Succeed;
    }

    private ReturnValues AreCowsHungry()
    {
        return ReturnValues.Succeed;
    }

    private bool AreCowsInFeedersPerception()
    {
        return true;
    }

    private bool IsFoodCollectedPerception()
    {
        return true;
    }

    private bool IsInBarnPerception()
    {
        return true;
    }
    
    private bool AreCowsFedPerception()
    {
        return true;
    }

    private void MoveToFeeders()
    {

    }
    
    private void CollectFood()
    {

    }

    private void MoveToBarn()
    {

    }

    private void FeedCows()
    {

    }

    private ReturnValues AreCowsWithMilk()
    {
        return ReturnValues.Succeed;
    }

    private void MilkCows()
    {

    }

    private ReturnValues MilkCowsCheck()
    {
        return ReturnValues.Succeed;
    }
}
