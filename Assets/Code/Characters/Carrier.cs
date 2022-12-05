using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carrier : MonoBehaviour
{
    private StateMachineEngine _FSMcarrier;
    private BehaviourTreeEngine _BTcarrier;

    private Locator _locator;

    Transition trans;

    // Start is called before the first frame update
    void Awake()
    {
        _locator = FindObjectOfType<Locator>();
        CreateAI();
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
        Perception sum = _FSMcarrier.CreatePerception<PushPerception>();
        ValuePerception estaEnAlmacen = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEstaEnAlmacen);
        ValuePerception suministroRecogido = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarRecogidaSuministros);
        ValuePerception estaEnTienda = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEstaEnTienda);
        ValuePerception suministroEntregado = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEntregaSuministros);
        ValuePerception estaEnPuesto = _FSMcarrier.CreatePerception<ValuePerception>(ComprobarEstaEnPuesto);

        //ESTADOS
        State esperar = _FSMcarrier.CreateEntryState("Esperar",EsperarSolicitud);
        State moverseAlmacen = _FSMcarrier.CreateState("MoverseAlmacen", MoverseAlmacen);
        State arbol =_FSMcarrier.CreateSubStateMachine("EstadoArbol", _BTcarrier);
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

        InverterDecoratorNode inverter = _BTcarrier.CreateInverterNode("inverter", secuencia);
        LoopUntilFailDecoratorNode loop = _BTcarrier.CreateLoopUntilFailNode("loop", inverter);
        _BTcarrier.SetRootNode(loop);

        //TRANSICIONES
        trans = _FSMcarrier.CreateTransition("transicion1", esperar, sum, moverseAlmacen);
        Transition transicion2 = _FSMcarrier.CreateTransition("transicion2", moverseAlmacen, estaEnAlmacen, arbol);
        Transition transicion3 = _BTcarrier.CreateExitTransition("transicion3", arbol, suministroRecogido, moverseTienda);
        Transition transicion4 = _FSMcarrier.CreateTransition("transicion4", moverseTienda, estaEnTienda, entregarSuministro);
        Transition transicion5 = _FSMcarrier.CreateTransition("transicion5", entregarSuministro, suministroEntregado, moversePuesto);
        Transition transicion6 = _FSMcarrier.CreateTransition("transicion6", moversePuesto, estaEnPuesto, esperar);

        Debug.Log("Hola caracola");
    }

    void EsperarSolicitud() 
    {
        Debug.Log("Estado inicial de esperar");
        StartCoroutine(llamarPercepcion());
    }

    void MoverseAlmacen() 
    {
        Debug.Log("Se esta moviendo al almacen");
    }

    bool ComprobarEstaEnAlmacen()
    {
        Debug.Log("Esta en el almacen");
        return true;
    }

    void MoverseTienda()
    {
        Debug.Log("Se esta moviendo a la tienda");
    }

    bool ComprobarEstaEnTienda()
    {
        Debug.Log("Esta en la tienda");
        return true;
    }

    bool ComprobarRecogidaSuministros()
    {
        Debug.Log("Suministros recogidos check");
        return true;
    }
    void EntregarSuministro()
    {
        Debug.Log("Se ha entregado los suministros");
    }

    bool ComprobarEntregaSuministros()
    {
        Debug.Log("Suministros entregados check");
        return true;
    }

    void MoversePuesto()
    {
        Debug.Log("Se esta moviendo al puesto");
    }

    bool ComprobarEstaEnPuesto()
    {
        Debug.Log("Esta en el puesto");
        return true;
    }

    //ARBOL
    ReturnValues ComprobarLeche() 
    {
        Debug.Log("Leche comprobada");
        return ReturnValues.Succeed;
    }

    ReturnValues ComprobarTrigo()
    {
        Debug.Log("Trigo comprobado");
        return ReturnValues.Succeed;
    }

    void RecogerSuministro() 
    {
        
    }

    ReturnValues RecogerSuminstroCheck()
    {
        Debug.Log("Suministro recogido");
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
