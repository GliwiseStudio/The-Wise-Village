using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character/CharacterConfiguration", fileName = "XXXConfiguration")]
public class CharacterConfigurationSO : ScriptableObject
{
    [SerializeField] private float _movementSpeed = 3f;
    
    public float MovementSpeed => _movementSpeed;
}
