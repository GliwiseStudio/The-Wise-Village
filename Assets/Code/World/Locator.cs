using UnityEngine;

public class Locator : MonoBehaviour
{
    [SerializeField] private LocatorConfigurationSO _configuration;
    [SerializeField] private float _distanceRange = 0.5f;

    public bool IsCharacterInPlace(Vector3 characterPosition, string placeOfInterestName)
    {
        Vector3 placeOfInterestPosition = _configuration.GetPlaceOfInterestPositionFromName(placeOfInterestName);
        float distance = Vector3.Distance(characterPosition, placeOfInterestPosition);

        if(distance < _distanceRange)
        {
            return true;
        }

        return false;
    }
}
