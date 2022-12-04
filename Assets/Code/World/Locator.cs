using UnityEngine;

public class Locator : MonoBehaviour
{
    [SerializeField] private LocatorConfigurationSO _configuration;
    [SerializeField] private float _distanceRange = 0.5f;

    private void Awake()
    {
        _configuration.Initialize();
    }

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

    public Vector3 GetPlaceOfInterestPositionFromName(string name)
    {
        return _configuration.GetPlaceOfInterestPositionFromName(name);
    }
}
