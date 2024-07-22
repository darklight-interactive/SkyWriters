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


    /// <summary>
    /// Returns the antipodal point of a given point on the circumference of the stage.
    /// </summary>
    /// <param name="point">
    ///     The point on the circumference of the stage.
    /// </param>
    /// <returns></returns>
    Vector3 GetAntipodalPoint(Vector3 point)
    {
        Vector3 center = transform.position; // This transform's position is the center of the stage
        Vector3 directionXZ = point - center; // Get the direction vector from the center to the point
        Vector3 antipodalPoint = center - directionXZ; // Get the antipodal point by reversing the direction vector
        return new Vector3(antipodalPoint.x, center.y, antipodalPoint.z);
    }

    /// <summary>
    /// Returns a random point within the stage.
    /// </summary>
    /// <returns>
    ///     A Vector3 representing a random point within the stage.
    ///     The y value of the vector is the same as the stage's center.
    /// </returns>
    public Vector3 GetRandomPosInStage()
    {
        Vector3 randomPoint = Random.insideUnitSphere * _stageRadius;
        randomPoint.y = transform.position.y;
        return randomPoint;
    }

    public void SpawnRandomCloud()
    {
        Instantiate(_cloudPrefab, GetRandomPosInStage(), Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);
    }

}
