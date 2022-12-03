using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{

    [SerializeField] private Animator _animator;
    private FarmerAnimationsHandler _animationsHandler;
    private BehaviourTreeEngine _merchantBT;
    private Locator _locator;

    private void Awake()
    {
        _merchantBT = new BehaviourTreeEngine();
        _animationsHandler = new FarmerAnimationsHandler(_animator);
        _locator = FindObjectOfType<Locator>();

        CreateAI();
    }

    private void Update()
    {
        _merchantBT.Update();
    }

    private void CreateAI()
    {
        #region First main leaf node
        // Loop to wait for supplies and make bread
        LeafNode hasSuppliesLN = _merchantBT.CreateLeafNode("hasSuppliesLN", HasSupplies, GottenSupplies);
        LeafNode makeBreadLN = _merchantBT.CreateLeafNode("makeBreadLN", MakeBread, BreadMade);

        SequenceNode interactWithSuppliesSN = _merchantBT.CreateSequenceNode("InteractWithSupplies", false);

        interactWithSuppliesSN.AddChild(hasSuppliesLN);
        interactWithSuppliesSN.AddChild(makeBreadLN);

        LoopDecoratorNode waitForSuppliesDN = _merchantBT.CreateLoopNode("waitForSuppliesDN", interactWithSuppliesSN);

        // Sequence to get supplies
        LeafNode walkToCarrierLN = _merchantBT.CreateLeafNode("walkToCarrierLN", WalkToCarrier, ArrivedToCarrier);
        LeafNode talkToCarrierLN = _merchantBT.CreateLeafNode("talkToCarrierLN", TalkToCarrier, TalkedToCarrier);
        LeafNode walkBackToShopLN = _merchantBT.CreateLeafNode("walkBackToShopLN", WalkBackToShop, WalkedToShop);

        SequenceNode getSuppliesSN = _merchantBT.CreateSequenceNode("getSuppliesSN", false);

        getSuppliesSN.AddChild(walkToCarrierLN);
        getSuppliesSN.AddChild(talkToCarrierLN);
        getSuppliesSN.AddChild(walkBackToShopLN);
        getSuppliesSN.AddChild(waitForSuppliesDN);

        // CONCEPTUALMENTE ES UN SUCCEDER PERO WENO, NO LO HE METIDO PQ LITERALLY Q EL RESTO NO PUEDEN FALLAR

        // Sequence to check if we have supplies, and if not get them
        LeafNode isCounterEmptyLN = _merchantBT.CreateLeafNode("isCounterEmptyLN", IsCounterEmpty, GottenCounterState);

        SequenceNode checkSuppliesSN = _merchantBT.CreateSequenceNode("checkSuppliesSN", false);

        checkSuppliesSN.AddChild(isCounterEmptyLN);
        checkSuppliesSN.AddChild(getSuppliesSN);

        // Main leaf node supplies, always have to return false, because if there are no supplies you get them and them check for clients, andif there are you just check for clients
        SucceederDecoratorNode succederCheckSuppliesDN = _merchantBT.CreateSucceederNode("succederCheckSuppliesDN", checkSuppliesSN);

        InverterDecoratorNode inverterSuccederCheckSuppliesDN = _merchantBT.CreateInverterNode("inverterSuccederCheckSuppliesDN", succederCheckSuppliesDN);

        #endregion

        #region Second main leaf node

        // serve customers
        LeafNode isThereAClientLN = _merchantBT.CreateLeafNode("isThereAClient", IsThereAClient, GottenClients);
        LeafNode serveCustomerLN = _merchantBT.CreateLeafNode("serveCustomer", ServeCustomer, ServedCustomer);

        SequenceNode checkAndServeCustomersSN = _merchantBT.CreateSequenceNode("checkAndServeCustomers", false);
        checkAndServeCustomersSN.AddChild(isThereAClientLN);
        checkAndServeCustomersSN.AddChild(serveCustomerLN);

        #endregion

        #region Main loop and selector

        SelectorNode pickActionSN = _merchantBT.CreateSelectorNode("pickActionSN");
        pickActionSN.AddChild(inverterSuccederCheckSuppliesDN);
        pickActionSN.AddChild(checkAndServeCustomersSN);

        LoopDecoratorNode mainInfiniteLoopDN = _merchantBT.CreateLoopNode("mainInfiniteLoopDN", pickActionSN);

        _merchantBT.SetRootNode(mainInfiniteLoopDN);

        #endregion
    }

    #region returns

    private ReturnValues ServedCustomer()
    {
        throw new NotImplementedException();
    }

    private ReturnValues GottenClients()
    {
        throw new NotImplementedException();
    }

    private ReturnValues GottenCounterState()
    {
        throw new NotImplementedException();
    }

    private ReturnValues WalkedToShop()
    {
        throw new NotImplementedException();
    }

    private ReturnValues TalkedToCarrier()
    {
        throw new NotImplementedException();
    }

    private ReturnValues ArrivedToCarrier()
    {
        throw new NotImplementedException();
    }

    private ReturnValues BreadMade()
    {
        throw new NotImplementedException();
    }

    private ReturnValues GottenSupplies()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region methods

    private void ServeCustomer()
    {
        throw new NotImplementedException();
    }

    private void IsThereAClient()
    {
        throw new NotImplementedException();
    }

    private void IsCounterEmpty()
    {
        throw new NotImplementedException();
    }

    private void WalkBackToShop()
    {
        throw new NotImplementedException();
    }

    private void TalkToCarrier()
    {
        throw new NotImplementedException();
    }

    private void WalkToCarrier()
    {
        throw new NotImplementedException();
    }

    private void MakeBread()
    {
        throw new NotImplementedException();
    }

    private void HasSupplies()
    {
        throw new NotImplementedException();
    }

    #endregion
}
