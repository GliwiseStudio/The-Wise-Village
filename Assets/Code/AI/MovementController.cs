using UnityEngine;
using UnityEngine.AI;

public class MovementController
{
    private readonly NavMeshAgent _navMeshAgent;
    private readonly CharacterConfigurationSO _characterConfiguration;

    public MovementController(NavMeshAgent navMeshAgent, CharacterConfigurationSO characterConfiguration, Vector3 initialPosition)
    {
        _navMeshAgent = navMeshAgent;
        _characterConfiguration = characterConfiguration;

        _navMeshAgent.speed = _characterConfiguration.MovementSpeed;
        WarpToPosition(initialPosition);
    }

    public void MoveToPosition(Vector3 position)
    {
        if(_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = false;
        }

        _navMeshAgent.SetDestination(position);
    }


    public void ContinueMovement()
    {
        _navMeshAgent.isStopped = false;
    }

    private void WarpToPosition(Vector3 position)
    {
        _navMeshAgent.Warp(position);
    }

    public void Stop()
    {
        _navMeshAgent.isStopped = true;
    }
}
