using UnityEngine;

[System.Serializable]
public class PlaceOfInterest
{
    [SerializeField] private string _name;
    [SerializeField] private Vector3 _position;
    [SerializeField] private float _distanceOffset = 2.5f;
    public string Name => _name;
    public Vector3 Position => _position;
    public float DistanceOffset => _distanceOffset;
}
