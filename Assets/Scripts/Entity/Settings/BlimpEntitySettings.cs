using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/Entity/BlimpSettings")]
public class BlimpEntitySettings : EntitySettings
{
    [Header("Blimp Settings")]
    [SerializeField] CloudEntitySettings _exhaustCloudSettings;
    [SerializeField] float _exhaustSpawnDelay = 1f;
    public CloudEntitySettings exhaustCloudSettings => _exhaustCloudSettings;
    public float exhaustSpawnDelay => _exhaustSpawnDelay;
}