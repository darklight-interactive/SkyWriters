using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Behaviour;
using UnityEngine;
using NaughtyAttributes;
using Darklight.UnityExt.Editor;



[ExecuteAlways]
public class StageManager : MonoBehaviourSingleton<StageManager>
{

    // -------------- Static Fields ------------------------
    public static void AssignEntityToStage(StageEntity entity, float height = 1)
    {
        float stageHeight = Instance._stageHeight + (height / 2);
        entity.position = new Vector3(entity.position.x, height, entity.position.z);
    }


    // -------------- Private Serialized Fields --------------
    [Header("Stage Settings")]
    [ShowOnly, SerializeField] float _stageHeight;
    [ShowOnly, SerializeField] private float _stageRadius = 1000;
    [SerializeField, Range(10, 1000)] private float _spawnRadiusOffset = 100;


    [Header("Stage Data")]
    [SerializeField] private List<Collider> _stageColliders;
    [SerializeField] private List<Collider> _spawnAreaColliders;
    [SerializeField] private List<PlaneController> _planesInStage;
    [SerializeField] private List<CloudInteractable> _cloudsInStage;

    [Header("Cloud Data")]
    [SerializeField] private List<CloudGradientData> _cloudGradients;

    [Header("Prefabs")]
    [SerializeField] GameObject _planePrefab;
    [SerializeField] GameObject _cloudPrefab;

    public override void Initialize()
    {
        _stageHeight = transform.position.y;
    }

    public void Update()
    {
        _stageColliders = Physics.OverlapSphere(transform.position, _stageRadius).ToList();
        _spawnAreaColliders = Physics.OverlapSphere(transform.position, _stageRadius + _spawnRadiusOffset).ToList();

        _planesInStage = new List<PlaneController>();
        _cloudsInStage = new List<CloudInteractable>();

        // Update the collider references
        foreach (Collider collider in _spawnAreaColliders)
        {
            PlaneController planeController = collider.gameObject.GetComponent<PlaneController>();

            if (collider.gameObject.GetComponent<PlaneController>())
            {
                if (!_planesInStage.Contains(collider.gameObject.GetComponent<PlaneController>()))
                {
                    _planesInStage.Add(collider.gameObject.GetComponent<PlaneController>());
                }
            }

            if (collider.gameObject.GetComponent<CloudInteractable>())
            {
                if (!_cloudsInStage.Contains(collider.gameObject.GetComponent<CloudInteractable>()))
                {
                    _cloudsInStage.Add(collider.gameObject.GetComponent<CloudInteractable>());
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw the stage radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);

        // Draw the spawn offset
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _stageRadius + _spawnRadiusOffset);
    }


    #region ================= [[ CLOUD MANAGEMENT ]] ================= >>

    public void SpawnCloudAt(Vector3 position)
    {
        GameObject cloud = Instantiate(_cloudPrefab, position, Quaternion.identity);
        CloudGradientData randomCloudData = _cloudGradients[Random.Range(0, _cloudGradients.Count)];
        cloud.GetComponent<CloudInteractable>().SetCloudData(randomCloudData);
    }

    [Button]
    public void SpawnRandomCloud()
    {
        Vector3 randomSpawnPos = GetRandomPointOnLeftSideOfStage();
        SpawnCloudAt(randomSpawnPos);
    }

    #endregion


    // ---------------------------------------- Public Methods ---------------------------------------- >>

    public bool IsColliderInStage(Collider other)
    {
        return _stageColliders.Contains(other);
    }

    public bool IsColliderInSpawnArea(Collider other)
    {
        return _spawnAreaColliders.Contains(other);
    }

    /// <summary>
    /// Returns the antipodal point of a given point on the circumference of the stage.
    /// </summary>
    /// <param name="point">
    ///     The point on the circumference of the stage.
    /// </param>
    /// <returns></returns>
    public Vector3 GetAntipodalPoint(Vector3 point)
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

    Vector3 GetRandomPointOnLeftSideOfStage()
    {
        float leftBound = transform.position.x - _stageRadius;
        float upperBound = transform.position.z + _stageRadius;
        float lowerBound = transform.position.z - _stageRadius;
        return new Vector3(leftBound, transform.position.y, Random.Range(lowerBound, upperBound));
    }
}
