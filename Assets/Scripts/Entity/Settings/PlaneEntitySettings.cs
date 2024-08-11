using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/Entity/PlaneSettings")]
public class PlaneEntitySettings : EntitySettings
{
    [Header("Plane Settings")]
    public float accelerationSpeed = 1f;
}