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

    Vector3 GetAntipodalPoint(Vector3 point)
    {
        // Assuming the circle is centered at the origin of the world space on the XZ plane
        Vector3 center = transform.position;
        Vector3 directionXZ = new Vector3(point.x - center.x, 0, point.z - center.z);
        Vector3 antipodalPoint = center - directionXZ;
        return new Vector3(antipodalPoint.x, point.y, antipodalPoint.z); // Keep the same Y coordinate
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
