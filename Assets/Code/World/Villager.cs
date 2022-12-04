using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour
{
    //[SerializeField] private Animator _animator;
    //private VillagerAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _villagerBT;
    private Locator _locator;
    // Start is called before the first frame update
    private void Awake()
    {
        //_animationsHandler = new VillagerAnimationsHandler(_animator);
        _locator = FindObjectOfType<Locator>();
    }

    // Update is called once per frame
    private void CreateAI()
    {
        _villagerBT = new BehaviourTreeEngine();
        //SECUENCIA 1
        LeafNode isHungryLeafNode = _villagerBT.CreateLeafNode("IsHungry", null, IsHungry);

        LeafNode goShopLeafNode = _villagerBT.CreateLeafNode("GoShop", null, GoShop);
        SucceederDecoratorNode goShopSucceeder = _villagerBT.CreateSucceederNode("GoShopSucceeder", goShopLeafNode);

        LeafNode goEatLeafNode = _villagerBT.CreateLeafNode("GoEat", null, GoEat);
        SucceederDecoratorNode goEatSucceeder = _villagerBT.CreateSucceederNode("GoEatSucceeder", goEatLeafNode);

        SequenceNode isHungrySequence = _villagerBT.CreateSequenceNode("isHungry", false);
        isHungrySequence.AddChild(isHungryLeafNode);
        isHungrySequence.AddChild(goShopSucceeder);
        isHungrySequence.AddChild(goEatSucceeder);

        //SECUENCIA 2
        LeafNode isThirstyLeafNode = _villagerBT.CreateLeafNode("IsThirsty", null, IsThirsty);

        LeafNode goWellLeafNode = _villagerBT.CreateLeafNode("GoWell", null, GoWell);
        SucceederDecoratorNode goWellSucceeder = _villagerBT.CreateSucceederNode("GoWellSucceeder", goWellLeafNode);

        LeafNode goDrinkLeafNode = _villagerBT.CreateLeafNode("GoDrink", null, GoDrink);
        SucceederDecoratorNode goDrinkSucceeder = _villagerBT.CreateSucceederNode("GoDrinkSucceeder", goDrinkLeafNode);

        SequenceNode isThirstySequence = _villagerBT.CreateSequenceNode("isHungry", false);
        isHungrySequence.AddChild(isThirstyLeafNode);
        isHungrySequence.AddChild(goWellSucceeder);
        isHungrySequence.AddChild(goDrinkSucceeder);

        //SECUENCIA 3
        LeafNode walkLeafNode = _villagerBT.CreateLeafNode("Walk", Walk, WalkCheck);

        SequenceNode walkSequence = _villagerBT.CreateSequenceNode("walk", false);
        walkSequence.AddChild(walkLeafNode);

        //SELECTOR
        SelectorNode selector = _villagerBT.CreateSelectorNode("Selector");
        selector.AddChild(isHungrySequence);
        selector.AddChild(isThirstyLeafNode);
        selector.AddChild(walkSequence);

        //LOOP INFINITO
        LoopDecoratorNode rootLoop = _villagerBT.CreateLoopNode("LoopInfinito", selector);
        _villagerBT.SetRootNode(rootLoop);
    }
    private ReturnValues IsHungry()
    {
        return ReturnValues.Succeed;
    }
    private ReturnValues GoShop()
    {
        return ReturnValues.Succeed;
    }
    private ReturnValues GoEat()
    {
        return ReturnValues.Succeed;
    }
    private ReturnValues IsThirsty()
    {
        return ReturnValues.Succeed;
    }
    private ReturnValues GoWell()
    {
        return ReturnValues.Succeed;
    }
    private ReturnValues GoDrink()
    {
        return ReturnValues.Succeed;
    }
    private void Walk()
    {

    }
    private ReturnValues WalkCheck()
    {
        return ReturnValues.Succeed;
    }
}

