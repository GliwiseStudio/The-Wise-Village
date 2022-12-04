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
        LeafNode generatedMilkLeafNode = _rancherBT.CreateLeafNode("GeneratedMilk", null, IsMilkGenerated);

        StateMachineEngine storeMilkStateMachine = new StateMachineEngine(true);

        //PERCEPCIONES
        ValuePerception isMilkCollectedPerception = storeMilkStateMachine.CreatePerception<ValuePerception>(IsMilkCollected);
        ValuePerception isInStoragePerception = storeMilkStateMachine.CreatePerception<ValuePerception>(IsInStorage);
        ValuePerception isMilkStoredPerception = storeMilkStateMachine.CreatePerception<ValuePerception>(IsMilkStored);
        ValuePerception isInBarnPerception = storeMilkStateMachine.CreatePerception<ValuePerception>(IsInBarnPerception);

        //ESTADOS
        State collectMilkState = storeMilkStateMachine.CreateEntryState("CollectMilk", CollectMilk);
        State moveToStoreageState = storeMilkStateMachine.CreateState("MoveToStorage", MoveToStorage);
        State storeMilkState = storeMilkStateMachine.CreateState("StoreMilk", StoreMilk);
        State moveToBarnStatee = storeMilkStateMachine.CreateState("MoveToBarn", MoveToBarn);

        //TRANSICIONES
        storeMilkStateMachine.CreateTransition("CollectMilk-MoveToStorage", collectMilkState, isMilkStoredPerception, moveToStoreageState);
        storeMilkStateMachine.CreateTransition("MoveToStorage-StoreMilk", moveToStoreageState, isInStoragePerception, storeMilkState);
        storeMilkStateMachine.CreateTransition("StoreMilk-MoveToBarn", storeMilkState, isMilkStoredPerception, moveToBarnStatee);
        storeMilkStateMachine.CreateExitTransition("MoveToBarn-Exit", moveToBarnStatee, isInBarnPerception, ReturnValues.Succeed);

        LeafNode storeMilkLeafNode = _rancherBT.CreateSubBehaviour("StoreMilkProcess", storeMilkStateMachine);
        SucceederDecoratorNode storeMilkSucceeder = _rancherBT.CreateSucceederNode("StoreMilkSucceeder", storeMilkLeafNode);

        SequenceNode storeMilkSequence = _rancherBT.CreateSequenceNode("StoreMilkSequence", false);
        storeMilkSequence.AddChild(isInBarnLeafNode);
        storeMilkSequence.AddChild(generatedMilkLeafNode);
        storeMilkSequence.AddChild(storeMilkSucceeder);

        //SELECTOR
        SelectorNode selector = _rancherBT.CreateSelectorNode("Selector");
        selector.AddChild(feedSequence);
        selector.AddChild(milkCowsSequence);
        selector.AddChild(storeMilkSequence);

        //LOOP INFINITO
        LoopDecoratorNode infiniteLoop = _rancherBT.CreateLoopNode("InfiniteLoop", selector);

        _rancherBT.SetRootNode(infiniteLoop);
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

    private ReturnValues IsMilkGenerated()
    {
        return ReturnValues.Succeed;
    }

    private bool IsMilkCollected()
    {
        return true;
    }

    private bool IsInStorage()
    {
        return true;
    }

    private bool IsMilkStored()
    {
        return true;
    }

    private void CollectMilk()
    {

    }

    private void MoveToStorage()
    {

    }

    private void StoreMilk()
    {

    }
}
