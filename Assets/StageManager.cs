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
    Brown
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
    SphereCollider _collider => GetComponent<SphereCollider>();

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
        GameObject cloud = Instantiate(_cloudPrefab, GetRandomPosInStage(), Quaternion.identity);
        CloudParticleData randomCloudData = cloudParticleData[Random.Range(0, cloudParticleData.Count)];
        cloud.GetComponent<CloudInteractable>().SetCloudData(randomCloudData);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);
    }

}
