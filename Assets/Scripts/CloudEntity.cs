using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(ParticleSystem))]
public class CloudEntity : StageEntity
{
    ParticleSystem _particleSystem => GetComponent<ParticleSystem>();
    CloudGradientData _gradientData;

    public override void Initialize(Type entityType = Type.CLOUD)
    {
        base.Initialize();

        // Remove the cloud object if it is out of bounds
        _respawnOnExit = false;
    }

    public void SetCloudGradient(CloudGradientData cloudParticleData)
    {
        _gradientData = cloudParticleData;
        SetColorOverLifetime(_gradientData.ToGradient());
    }

    void SetColorOverLifetime(Gradient gradient)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _particleSystem.colorOverLifetime;
        colorOverLifetime.color = gradient;
    }

    // ================== Unity Events ==================
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlaneEntity>())
        {
            _stageManager.SpawnEntityRandomly_InStage<CloudEntity>();

            // Set the contrail color to the cloud's color
            PlaneEntity planeController = other.gameObject.GetComponent<PlaneEntity>();

            if (planeController == null)
            {
                Debug.LogError("PlaneController not found on the plane object.");
                return;
            }

            if (_gradientData == null)
            {
                Debug.LogError("CloudParticleData not set on the cloud object.");
                Destroy(gameObject);
                return;
            }

            planeController.CreateNewContrail(_gradientData.ToGradient());
            Destroy(gameObject);
        }
    }
}
