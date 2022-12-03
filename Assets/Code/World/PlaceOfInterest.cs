using UnityEngine;

[System.Serializable]
public class PlaceOfInterest
{
    [SerializeField] private string _name;
    [SerializeField] private Vector3 _position;
    public string Name => _name;
    public Vector3 Position => _position;
}
