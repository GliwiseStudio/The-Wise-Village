using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Locator/Locator Configuration", fileName = "LocatorConfigurationSO")]
public class LocatorConfigurationSO : ScriptableObject
{
    [SerializeField] private List<PlaceOfInterest> _placesOfInterest;
    private IDictionary<string, Vector3> _placesOfInteresStorage;

    public void Initialize()
    {
        InitializeStorage();
    }

    private void InitializeStorage()
    {
        _placesOfInteresStorage = new Dictionary<string, Vector3>();

        foreach (PlaceOfInterest place in _placesOfInterest)
        {
            bool addedSuccesfully = _placesOfInteresStorage.TryAdd(place.Name, place.Position);

#if UNITY_EDITOR
            if(!addedSuccesfully)
            {
                Debug.LogWarning($"The place of interest called {place.Name} already exists in the storage. Is it duplicate?");
            }
#endif
        }
    }

    public Vector3 GetPlaceOfInterestPositionFromName(string name)
    {
        Vector3 returnPosition;
        _placesOfInteresStorage.TryGetValue(name, out returnPosition);
        return returnPosition;
    }
}
