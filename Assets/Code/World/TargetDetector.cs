using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class TargetDetector
{
    private Transform _transform;
    private float _detectionRadius;
    private string[] _targetLayerMaskName;

    public TargetDetector(Transform towerTransform, float detectionRadius, string targetLayerMaskString)
    {
        _transform = towerTransform;
        SetRadius(detectionRadius);
        _targetLayerMaskName = new string[1];
        _targetLayerMaskName[0] = targetLayerMaskString;
    }

    public TargetDetector(Transform towerTransform, float detectionRadius, string[] targetLayerMasksString)
    {
        _transform = towerTransform;
        SetRadius(detectionRadius);
        _targetLayerMaskName = targetLayerMasksString;
    }

    public TargetDetector(float detectionRadius, string[] targetLayerMasksString)
    {
        SetRadius(detectionRadius);
        _targetLayerMaskName = targetLayerMasksString;
    }

    public TargetDetector(float detectionRadius, string targetLayerMaskString)
    {
        SetRadius(detectionRadius);
        _targetLayerMaskName = new string[1];
        _targetLayerMaskName[0] = targetLayerMaskString;
    }

    public TargetDetector(string targetLayerMasksString)
    {
        _targetLayerMaskName = new string[1];
        _targetLayerMaskName[0] = targetLayerMasksString;
    }

    public TargetDetector(string[] targetLayerMasksString)
    {
        _targetLayerMaskName = targetLayerMasksString;
    }

    public TargetDetector()
    {

    }

    public void SetTransform(Transform transform)
    {
        _transform = transform;
    }

    public void SetRadius(float newRadius)
    {
        if (newRadius == -1f)
        {
            _detectionRadius = Mathf.Infinity;
        }
        else
        {
            _detectionRadius = newRadius;
        }
    }

    public void SetTargetLayers(string[] layers)
    {
        _targetLayerMaskName = layers;
    }

    public Transform DetectTarget()
    {
        Collider enemyFound = FindNearest();
        if (enemyFound == null)
        {
            return null;
        }

        return enemyFound.transform;
    }

    public GameObject DetectTargetGameObject()
    {
        Collider enemyFound = FindNearest();
        if (enemyFound == null)
        {
            return null;
        }

        return enemyFound.gameObject;
    }

    public Collider DetectTargetCollider()
    {
        Collider enemyFound = FindNearest();
        if (enemyFound == null)
        {
            return null;
        }

        return enemyFound;
    }

    private Collider FindNearest()
    {
        Collider[] enemiesFound = Physics.OverlapSphere(_transform.position, _detectionRadius, LayerMask.GetMask(_targetLayerMaskName));
        if (enemiesFound.Length == 0)
        {
            return null;
        }

        float distance = GetSquareDistanceToPlayer(enemiesFound[0].transform.position);
        int closestIndex = 0;
        float currentEnemyDistance;

        for (int i = 1; i < enemiesFound.Length; i++)
        {
            currentEnemyDistance = GetSquareDistanceToPlayer(enemiesFound[i].transform.position);
            if (distance > currentEnemyDistance)
            {
                distance = currentEnemyDistance;
                closestIndex = i;
            }
        }

        return enemiesFound[closestIndex];
    }

    private float GetSquareDistanceToPlayer(Vector3 enemyPosition)
    {
        return (enemyPosition - _transform.position).sqrMagnitude;
    }

    public bool IsTargetInRange(Vector3 targetPosition)
    {
        if (GetSquareDistanceToPlayer(targetPosition) > (_detectionRadius * _detectionRadius))
        {
            return false;
        }

        return true;
    }

    public IReadOnlyList<Transform> GetAllTargetsInRange()
    {
        Collider[] targets = Physics.OverlapSphere(_transform.position, _detectionRadius, LayerMask.GetMask(_targetLayerMaskName));

        List<Transform> targetTransforms = new List<Transform>();

        for (int i = 0; i < targets.Length; i++)
        {
            targetTransforms.Add(targets[i].transform);
        }

        return targetTransforms;
    }

    public Vector3 GetPositionFromClickInLayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 clickPoint = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        if (Physics.Raycast(ray, out RaycastHit info, Mathf.Infinity, LayerMask.GetMask(_targetLayerMaskName)))
        {
            clickPoint = info.point;
        }

        return clickPoint;
    }

    public GameObject GetGameObjectFromClickInLayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GameObject clickPoint = null;

        if (Physics.Raycast(ray, out RaycastHit info, Mathf.Infinity, LayerMask.GetMask(_targetLayerMaskName)))
        {
            clickPoint = info.transform.gameObject;
        }

        return clickPoint;
    }
}
