using UnityEngine;
using UnityEngine.AI;

public class Prueba : MonoBehaviour
{
    [SerializeField] private CharacterConfigurationSO _configuration;
    private MovementController _movementController;
    private NavMeshAgent _agent;
    private Locator _locator;

    private void Awake()
    {
        _locator = FindObjectOfType<Locator>();
        _agent = GetComponent<NavMeshAgent>();
        _movementController = new MovementController(_agent, _configuration, Vector3.zero);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Establo"));
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            _movementController.MoveToPosition(_locator.GetPlaceOfInterestPositionFromName("Almacen"));
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            _movementController.Stop();
        }
    }
}
