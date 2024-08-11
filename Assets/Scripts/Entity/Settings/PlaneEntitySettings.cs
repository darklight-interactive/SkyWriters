using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/Entity/PlaneSettings")]
public class PlaneEntitySettings : EntitySettings
{
    [Header("Plane Settings")]
    public float accelerationSpeed = 1f;
    public float speedMultiplier_slow = 0.8f;
    public float speedMultiplier_fast = 1.5f;
}