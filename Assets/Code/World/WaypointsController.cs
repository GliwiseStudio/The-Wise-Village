using System.Collections.Generic;
using UnityEngine;

public class WaypointsController : MonoBehaviour
{
    [SerializeField] private List<Transform> _policeWaypoints;
    [SerializeField] private List<Transform> _thiefWaypoints;

    private void Awake()
    {
    }

    public Vector3 GetRandomWaypoint(string type)
    {
        if(type.CompareTo("Thief") == 0)
        {
            int index = Random.Range(0, _thiefWaypoints.Count);
            return _thiefWaypoints[index].position;
        } 
        else if(type.CompareTo("Police") == 0)
        {
            int index = Random.Range(0, _policeWaypoints.Count);
            return _policeWaypoints[index].position;
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
