using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Carrier : MonoBehaviour
{
    [SerializeField] private CharacterConfigurationSO _configuration;

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
        Debug.Log("Estado inicial de esperar");

        //temporal
        //StartCoroutine(llamarPercepcion());
    }

    void MoverseAlmacen() 
    {
        Debug.Log("Voy al almacen");
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Almacen"));
    }

    bool ComprobarEstaEnAlmacen()
    {
        //Debug.Log("Estoy de camino al almacen");
        return _locator.IsCharacterInPlace(transform.position, "Almacen");
    }

    void MoverseTienda()
    {
        Debug.Log("Voy a la tienda");
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Shop"));
    }

    bool ComprobarEstaEnTienda()
    {
        Debug.Log("Estoy de camino a la tienda");
        return _locator.IsCharacterInPlace(transform.position, "Shop");
    }

    /*
    bool ComprobarRecogidaSuministros()
    {
        if (_milk > 0 && _wheat > 0)
        {
            Debug.Log("Tengo suministros");
            return true;
        }
        else 
        {
            Debug.Log("No tengo suministros");
            return false;
        }
    }*/
    void EntregarSuministro()
    {
        _supplies.Deliver(_milk, _wheat);
        Debug.Log("Se ha entregado los suministros");
    }

    bool ComprobarEntregaSuministros()
    {
        Debug.Log("Suministros entregados check");
        //Esperar a la animacion
        return true;
    }

    void MoversePuesto()
    {
        Debug.Log("Voy al puesto");
        _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("CarrierPlace"));
    }

    bool ComprobarEstaEnPuesto()
    {
        //Debug.Log("Estoy de camino al puesto");
        return _locator.IsCharacterInPlace(transform.position, "CarrierPlace");
    }

    //ARBOL
    ReturnValues ComprobarLeche() 
    {
        return ReturnValues.Succeed;
        if (_warehouse.HasMilk())
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
        Debug.Log("Leche comprobada");
    }

    ReturnValues ComprobarTrigo()
    {
        return ReturnValues.Succeed;
        if (_warehouse.HasWheat())
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
        Debug.Log("Trigo comprobado");
    }

    void RecogerSuministro() 
    {
        Debug.Log("Suministros recogido");
        _milk = _warehouse.GetMilk();
        _wheat = _warehouse.GetWheat();
    }

    ReturnValues RecogerSuminstroCheck()
    {
        Debug.Log("Suministro recogido 2");
        //esperar animacion
        return ReturnValues.Succeed;
    }

    void DoNothing() 
    {

    }

    IEnumerator llamarPercepcion() 
    {
        yield return new WaitForSeconds(2);
        _FSMcarrier.Fire(trans);
    }
}
