using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "ScriptableObjects/Locator/Locator Configuration", fileName = "LocatorConfigurationSO")]
public class LocatorConfigurationSO : ScriptableObject
{
    [SerializeField] private List<PlaceOfInterest> _placesOfInterest;
    private IDictionary<string, PlaceOfInterest> _placesOfInteresStorage;

    public void Initialize()
    {
        InitializeStorage();
    }

    private void InitializeStorage()
    {
        _placesOfInteresStorage = new Dictionary<string, PlaceOfInterest>();

        foreach (PlaceOfInterest place in _placesOfInterest)
        {
            bool addedSuccesfully = _placesOfInteresStorage.TryAdd(place.Name, place);

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
        PlaceOfInterest returnPlaceOfInterest;
        _placesOfInteresStorage.TryGetValue(name, out returnPlaceOfInterest);

        Assert.IsNotNull(returnPlaceOfInterest, $"[LocatorConfigurationSO at GetPlaceOfInterestPositionFromName] : The place of interest called {name} could not be found");

        return returnPlaceOfInterest.Position;
    }

    public float GetPlaceOfInterestRangeOffsetFromName(string name)
    {
        PlaceOfInterest returnPlaceOfInterest;
        _placesOfInteresStorage.TryGetValue(name, out returnPlaceOfInterest);

        Assert.IsNotNull(returnPlaceOfInterest, $"[LocatorConfigurationSO at GetPlaceOfInterestRangeOffsetFromName] : The place of interest called {name} could not be found");

        return returnPlaceOfInterest.DistanceOffset;
    }
}
