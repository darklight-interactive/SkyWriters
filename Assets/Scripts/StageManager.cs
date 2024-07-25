using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;

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

[RequireComponent(typeof(Collider))]
public class StageManager : MonoBehaviourSingleton<StageManager>
{
    // -------------- Private Fields --------------
    CapsuleCollider _collider => GetComponent<CapsuleCollider>();

    // -------------- Serialized Fields --------------
    [SerializeField] float _stageRadius = 100;

    [Header("Prefabs")]
    [SerializeField] GameObject _planePrefab;
    [SerializeField] GameObject _cloudPrefab;

    [Header("Cloud Particle Data")]
    public List<CloudParticleData> cloudParticleData;

    public override void Initialize()
    {
        _collider.isTrigger = true;
        _collider.radius = _stageRadius;

        StartCoroutine(CloudSpawnRoutine());
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

    // On stage exit handler
    void OnTriggerExit(Collider other)
    {
        Transform otherTransform = other.transform;
        Vector3 antipodalPoint = GetAntipodalPoint(otherTransform.position);

        // If the object is a plane, teleport it to the antipodal point
        if (other.gameObject.GetComponent<PlaneController>())
        {
            otherTransform.position = antipodalPoint;
        }

        // If the object is a cloud, destroy it
        if (other.gameObject.GetComponent<CloudInteractable>())
        {
            Destroy(other.gameObject);
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
        Vector3 randomPoint = Random.insideUnitSphere * _stageRadius;
        randomPoint.x = -_stageRadius;
        randomPoint.y = transform.position.y;
        return randomPoint;
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

    public void SpawnRandomCloud()
    {
        GameObject cloud = Instantiate(_cloudPrefab, GetRandomPointOnLeftSideOfStage(), Quaternion.identity);
        CloudParticleData randomCloudData = cloudParticleData[Random.Range(0, cloudParticleData.Count)];
        cloud.GetComponent<CloudInteractable>().SetCloudData(randomCloudData);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);
    }

}
