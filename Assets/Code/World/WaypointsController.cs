using System.Collections.Generic;
using UnityEngine;

public class WaypointsController : MonoBehaviour
{
    private List<Transform> _waypoints;
    [SerializeField] private List<Transform> _thiefWaypoints;

    private void Awake()
    {
        _waypoints = new List<Transform>(GetComponentsInChildren<Transform>());
    }

    public Vector3 GetRandomWaypoint()
    {
        int index = Random.Range(0, _waypoints.Count);
        return _waypoints[index].position;
    }

    public Vector3 GetRandomWaypoint(string type)
    {
        if(type.CompareTo("Thief") == 0)
        {
            int index = Random.Range(0, _thiefWaypoints.Count);
            return _thiefWaypoints[index].position;
        }

        return Vector3.zero;
    }

    public bool IsCharacterInPlace(Vector3 characterPosition, Vector3 waypointPosition)
    {
        characterPosition.y = 0;
        waypointPosition.y = 0;

        float distance = Vector3.Distance(characterPosition, waypointPosition);

        if (distance < 0.5f)
        {
            return true;
        }

        return false;
    }
}
