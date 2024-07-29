using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Behaviour;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Represents the colors that can be used in the game.
/// </summary>
public enum GameColors
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Orange,
    Brown,
    White
}

[System.Serializable]
public class CloudParticleData
{
    public GameColors color;
    public Color startColor = Color.white;
    public Color middleColor = Color.white;
    public Color endColor = Color.white;

    public CloudParticleData(Color startColor, Color middleColor, Color endColor)
    {
        this.startColor = startColor;
        this.middleColor = middleColor;
        this.endColor = endColor;
    }

    public Gradient ToGradient()
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[3]
        {
            new GradientColorKey(startColor, 0.0f),
            new GradientColorKey(middleColor, 0.5f),
            new GradientColorKey(endColor, 1.0f)
        };

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3]
        {
            new GradientAlphaKey(startColor.a, 0.0f),
            new GradientAlphaKey(middleColor.a, 0.5f),
            new GradientAlphaKey(endColor.a, 1.0f)
        };

        gradient.SetKeys(colorKeys, alphaKeys);

        return gradient;
    }
}

[ExecuteAlways, RequireComponent(typeof(Collider))]
public class StageManager : MonoBehaviourSingleton<StageManager>
{
    // -------------- Serialized Fields --------------

    [Header("Stage Settings")]
    [SerializeField] private float _stageRadius = 1000;
    private float _stageDiameter => _stageRadius * 2;

    [Header("Stage Data")]
    [SerializeField] private List<Collider> _collidersInStage;
    [SerializeField] private List<PlaneController> _planesInStage;
    [SerializeField] private List<CloudInteractable> _cloudsInStage;

    [Header("Cloud Particle Data")]
    public List<CloudParticleData> cloudParticleData;

    [Header("Prefabs")]
    [SerializeField] GameObject _planePrefab;
    [SerializeField] GameObject _cloudPrefab;

    public override void Initialize()
    {
        //StartCoroutine(CloudSpawnRoutine());
    }

    public void Update()
    {
        _collidersInStage = Physics.OverlapSphere(transform.position, _stageRadius).ToList();
        _planesInStage = new List<PlaneController>();
        _cloudsInStage = new List<CloudInteractable>();

        // Update the collider references
        foreach (Collider collider in _collidersInStage)
        {
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);
    }


    [Button]
    public void SpawnRandomCloud()
    {
        GameObject cloud = Instantiate(_cloudPrefab, GetRandomPointOnLeftSideOfStage(), Quaternion.identity);
        CloudParticleData randomCloudData = cloudParticleData[Random.Range(0, cloudParticleData.Count)];
        cloud.GetComponent<CloudInteractable>().SetCloudData(randomCloudData);
    }

    public bool IsColliderInStage(Collider other)
    {
        return _collidersInStage.Contains(other);
    }

    public void TeleportColliderToAntipodalPoint(Collider collider)
    {
        Transform otherTransform = collider.transform;
        Vector3 antipodalPoint = GetAntipodalPoint(otherTransform.position);
        otherTransform.position = antipodalPoint;
    }

    IEnumerator CloudSpawnRoutine()
    {
        while (true)
        {
            float randomTime = Random.Range(1.0f, 5.0f);
            yield return new WaitForSeconds(randomTime);
            SpawnRandomCloud();
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

    public Vector3 GetRandomPosOutStage()
    {
        Vector3 randomPoint = GetRandomPointOutsideInnerRangeAndWithinOuterRange(_stageRadius, _stageRadius * 1.25f);
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

    Vector3 GetRandomPointOutsideInnerRangeAndWithinOuterRange(float innerRange, float outerRange)
    {
        // Generate a random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2);

        // Generate a random distance outside the inner range but within the outer range
        float distance = Random.Range(innerRange, outerRange);

        // Convert polar coordinates to Cartesian coordinates
        float x = distance * Mathf.Cos(angle);
        float z = distance * Mathf.Sin(angle);

        // Return the point as a Vector3 (assuming y = 0 for 2D plane)
        return new Vector3(x, 0, z);
    }





}
