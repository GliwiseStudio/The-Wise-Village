using UnityEngine;

public class Locator : MonoBehaviour
{
    [SerializeField] private LocatorConfigurationSO _configuration;

    private void Awake()
    {
        _configuration.Initialize();
    }

    public bool IsCharacterInPlace(Vector3 characterPosition, string placeOfInterestName)
    {
        Vector3 placeOfInterestPosition = _configuration.GetPlaceOfInterestPositionFromName(placeOfInterestName);
        characterPosition.y = 0;
        placeOfInterestPosition.y = 0;

        float distance = Vector3.Distance(characterPosition, placeOfInterestPosition);

        if (distance < _configuration.GetPlaceOfInterestRangeOffsetFromName(placeOfInterestName))
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
