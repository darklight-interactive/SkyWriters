using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(ParticleSystem))]
public class CloudEntity : StageEntity
{
    ParticleSystem _particleSystem => GetComponent<ParticleSystem>();
    CloudData _gradientData;

    public void SetCloudGradient(CloudData cloudParticleData)
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
            stageManager.SpawnEntityRandomly_InStage<CloudEntity>();

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
