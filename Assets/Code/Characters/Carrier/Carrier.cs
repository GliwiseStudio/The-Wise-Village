using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Carrier : MonoBehaviour
{
    [SerializeField] private CharacterConfigurationSO _configuration;
    [SerializeField] private TextMeshProUGUI _text;
    private Animator _animator;
    private CarrierAnimationsHandler _carrieAnimatorHandler;

    private StateMachineEngine _FSMcarrier;
    private BehaviourTreeEngine _BTcarrier;

    private Locator _locator;
    private MovementController _movementController;

    private int _milk = 0;
    private int _wheat = 0;

    private Supplies _supplies;
    private Warehouse _warehouse;

    private bool askedForSupplies = false;

    Transition trans;

    // Start is called before the first frame update
    void Awake()
    {
        _locator = FindObjectOfType<Locator>();
        _warehouse = FindObjectOfType<Warehouse>();
        _supplies = FindObjectOfType<Supplies>();
        _animator = GetComponent<Animator>();
        _carrieAnimatorHandler = new CarrierAnimationsHandler(_animator);
        CreateAI();
    }

    private void Start()
    {
        _movementController = new MovementController(this.GetComponent<NavMeshAgent>(), _configuration, _locator.GetPlaceOfInterestPositionFromName("CarrierPlace"));
    }

    private void OnEnable()
    {
        _supplies.OnDemand += OnDemand;
    }

    private void OnDisable()
    {
        _supplies.OnDemand -= OnDemand;
    }

    private void OnDemand() 
    {
        _FSMcarrier.Fire(trans);
    }

    // Update is called once per frame
    void Update()
    {
        _BTcarrier.Update();
        _FSMcarrier.Update();
    }

    void CreateAI() 
    {
        //MAQUINAS
        _BTcarrier = new BehaviourTreeEngine(true);
        _FSMcarrier = new StateMachineEngine();

        //PERCEPCIONES
        Perception suministroSolicitado = _FSMcarrier.CreatePerception<PushPerception>();
        ValuePerception estaEnAlmacen = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEstaEnAlmacen);
        Perception suministroRecogido = _FSMcarrier.CreatePerception<BehaviourTreeStatusPerception>(_BTcarrier, ReturnValues.Succeed);
        ValuePerception estaEnTienda = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEstaEnTienda);
        ValuePerception suministroEntregado = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEntregaSuministros);
        ValuePerception estaEnPuesto = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEstaEnPuesto);

        //ESTADOS
        State esperar = _FSMcarrier.CreateEntryState("Esperar",EsperarSolicitud);
        State moverseAlmacen = _FSMcarrier.CreateState("MoverseAlmacen", MoverseAlmacen);
        State moverseTienda = _FSMcarrier.CreateState("MoverseTienda", MoverseTienda);
        State entregarSuministro = _FSMcarrier.CreateState("EntregarSuministro", EntregarSuministro);
        State moversePuesto = _FSMcarrier.CreateState("MoversePuesto", MoversePuesto);

        //ARBOL
        LeafNode lecheAlmacenada = _BTcarrier.CreateLeafNode("lecheAlmacenada", DoNothing, ComprobarLeche);
        LeafNode trigoAlmacenado = _BTcarrier.CreateLeafNode("trigoAlmacenado", DoNothing, ComprobarTrigo);
        LeafNode recogerSuministro = _BTcarrier.CreateLeafNode("recoger", RecogerSuministro, RecogerSuminstroCheck);

        SequenceNode secuencia = _BTcarrier.CreateSequenceNode("Secuencia", false);
        secuencia.AddChild(lecheAlmacenada);
        secuencia.AddChild(trigoAlmacenado);
        secuencia.AddChild(recogerSuministro);

        //InverterDecoratorNode inverter = _BTcarrier.CreateInverterNode("inverter", secuencia);
        _BTcarrier.SetRootNode(secuencia);

        //TRANSICIONES
        trans = _FSMcarrier.CreateTransition("transicion1", esperar, suministroSolicitado, moverseAlmacen);

        State arbol =_FSMcarrier.CreateSubStateMachine("EstadoArbol", _BTcarrier);

        Transition transicion2 = _FSMcarrier.CreateTransition("transicion2", moverseAlmacen, estaEnAlmacen, arbol);
        Transition transicion3 = _BTcarrier.CreateExitTransition("transicion3", arbol, suministroRecogido, moverseTienda);
        Transition transicion4 = _FSMcarrier.CreateTransition("transicion4", moverseTienda, estaEnTienda, entregarSuministro);
        Transition transicion5 = _FSMcarrier.CreateTransition("transicion5", entregarSuministro, suministroEntregado, moversePuesto);
        Transition transicion6 = _FSMcarrier.CreateTransition("transicion6", moversePuesto, estaEnPuesto, esperar);
    }

    void EsperarSolicitud() 
    {
        _text.text = "Waiting to be asked for supplies";
        _carrieAnimatorHandler.PlayAnimationState("Sitting", 0.1f);
        Invoke("GirarPersonaje", 1f);
    }

    void GirarPersonaje() 
    {
        this.transform.SetPositionAndRotation(this.transform.position, Quaternion.Euler(0, 0, 0));
    }

    void MoverseAlmacen() 
    {
        _carrieAnimatorHandler.PlayAnimationState("Walking", 0.1f);

        _text.text = "Going to storage";
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Storage"));
    }

    bool ComprobarEstaEnAlmacen()
    {
        if (_locator.IsCharacterInPlace(transform.position, "Storage")) 
        {
            _carrieAnimatorHandler.PlayAnimationState("Idle", 0.1f);
            _movementController.Stop();
            _text.text = "Waiting for supplies on storage";
        }
        return _locator.IsCharacterInPlace(transform.position, "Storage");
    }

    void MoverseTienda()
    {
        _text.text = "Going to shop";
        _carrieAnimatorHandler.PlayAnimationState("Walking", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Shop"));
    }

    bool ComprobarEstaEnTienda()
    {
        //Debug.Log("Estoy de camino a la tienda");
        if(_locator.IsCharacterInPlace(transform.position, "Shop"))
        {
            _movementController.Stop();
            _carrieAnimatorHandler.PlayAnimationState("Idle", 0.1f);
        }
        return _locator.IsCharacterInPlace(transform.position, "Shop");
    }

    void EntregarSuministro()
    {
        _text.text = "Giving supplies to merchant";
        _carrieAnimatorHandler.PlayAnimationState("GiveItems", 0.1f);
        //Debug.Log("Se ha entregado los suministros");
    }

    bool ComprobarEntregaSuministros()
    {
        //Debug.Log("Suministros entregados check");
        if (_carrieAnimatorHandler.GiveItemsSuccesfully())
        {
            _supplies.Deliver(_milk, _wheat);
            return true;
        }
        else
        {
            return false;
        }
    }

    void MoversePuesto()
    {
        _text.text = "Going to my place";
        _carrieAnimatorHandler.PlayAnimationState("Walking", 0.1f);
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("CarrierPlace"));
    }

    bool ComprobarEstaEnPuesto()
    {
        //Debug.Log("Estoy de camino al puesto");
        if (_locator.IsCharacterInPlace(transform.position, "CarrierPlace"))
        {
            _carrieAnimatorHandler.PlayAnimationState("Idle", 0.1f);
        }
        return _locator.IsCharacterInPlace(transform.position, "CarrierPlace");
    }

    //ARBOL
    ReturnValues ComprobarLeche()
    {
        //Debug.Log("Leche comprobada");
        if (_warehouse.HasMilk())
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    ReturnValues ComprobarTrigo()
    {
        if (_warehouse.HasWheat())
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    void RecogerSuministro() 
    {
        _text.text = "Getting supplies from storage";
        _carrieAnimatorHandler.PlayAnimationState("GrabObject", 0.1f);
        _milk = _warehouse.GetMilk();
        _wheat = _warehouse.GetWheat();
    }

    ReturnValues RecogerSuminstroCheck()
    {
        if (_carrieAnimatorHandler.GetGrabObjectSuccesfully()) 
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    void DoNothing() 
    {

    }
}
