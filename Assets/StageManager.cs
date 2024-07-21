using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageManager : MonoBehaviourSingleton<StageManager>
{
    SphereCollider _collider => GetComponent<SphereCollider>();
    [SerializeField] float _stageRadius = 100;

    [Header("Objects")]
    [SerializeField] GameObject _planePrefab;
    [SerializeField] GameObject _cloudPrefab;

    public override void Initialize()
    {
        _collider.isTrigger = true;
        _collider.radius = _stageRadius;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlaneController>())
        {
            Vector3 planePosition = other.transform.position;
            Debug.Log($"Plane exited the stage at {planePosition}");

            Vector3 antipodalPoint = GetAntipodalPoint(planePosition);
            other.transform.position = antipodalPoint;
            Debug.Log($"Plane teleported to {antipodalPoint}");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);
    }

    public Vector3 GetAntipodalPoint(Vector3 point)
    {
        // Assuming the sphere is centered at the origin of the world space
        Vector3 center = transform.position;
        Vector3 direction = point - center;
        Vector3 antipodalPoint = center - direction;
        return antipodalPoint;
    }

    public void GetRandomPointInStage(out Vector3 point)
    {
        Vector3 center = transform.position;
        Vector3 randomDirection = Random.insideUnitSphere;
        point = center + randomDirection * _stageRadius;
    }

    public void SpawnRandomCloud()
    {
        Vector3 cloudPosition;
        GetRandomPointInStage(out cloudPosition);
        Instantiate(_cloudPrefab, cloudPosition, Quaternion.identity);
    }

}
