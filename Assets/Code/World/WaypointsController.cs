using System.Collections.Generic;
using UnityEngine;

public class WaypointsController : MonoBehaviour
{
    [SerializeField] private List<Transform> _policeWaypoints;
    [SerializeField] private List<Transform> _thiefWaypoints;
    [SerializeField] private List<Transform> _thiefEscapingWaypoints;
    [SerializeField] private List<Transform> _villagerWaypoints;

    public Vector3 GetRandomWaypoint(string type)
    {
        int index = 0;
        switch (type)
        {
            case "Thief":
                index = Random.Range(0, _thiefWaypoints.Count);
                return _thiefWaypoints[index].position;
            case "Police":
                index = Random.Range(0, _policeWaypoints.Count);
                return _policeWaypoints[index].position;
            case "ThiefEscaping":
                index = Random.Range(0, _thiefEscapingWaypoints.Count);
                return _thiefEscapingWaypoints[index].position;
            case "Villager":
                index = Random.Range(0, _villagerWaypoints.Count);
                return _villagerWaypoints[index].position;
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
