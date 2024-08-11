using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

public class CloudEntity : StageEntity
{
    VFX_ParticleSystemHandler _cloudParticleHandler;
    VFX_ParticleSystemHandler _cloudBurstParticleHandler;

    CloudEntitySettings _cloudSettings => (CloudEntitySettings)settings;

    public override void Initialize(EntitySettings settings)
    {
        base.Initialize(settings);

        // Assign a random color from the weights
        currentColorDataObject = _cloudSettings.GetRandomColorFromWeights();
        currentGradientData = new VFX_GradientData(currentColorDataObject, 0.0f);

        CreateCloudParticles();

        OnTriggerEntered += HandleTriggerEntered;
    }

    public override void LoadSettings(EntitySettings settings)
    {
        base.LoadSettings(settings);
    }

    void HandleTriggerEntered(Collider other)
    {
        if (other.GetComponent<PlaneEntity>())
        {
            PlaneEntity plane = other.GetComponent<PlaneEntity>();
            plane.CollectNewColor(currentColorDataObject);

            if (Application.isPlaying)
            {
                CreateCloudBurstParticles();
                Destroy(gameObject);
            }
        }
    }

    void CreateCloudParticles()
    {
        ParticleSystem cloudParticles = VFX_Manager.Instance.cloudParticles;

        _cloudParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudParticles, transform);

        // Apply the gradient to the particle system
        _cloudParticleHandler.ApplyGradient(currentGradientData.gradient);
        _cloudParticleHandler.Play();
    }

    public void CreateCloudBurstParticles()
    {
        if (_cloudBurstParticleHandler != null) return;

        ParticleSystem cloudBurstParticles = VFX_Manager.Instance.cloudBurstParticles;
        _cloudBurstParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudBurstParticles, null);

        _cloudBurstParticleHandler.ApplyGradient(currentGradientData.gradient);
        _cloudBurstParticleHandler.Play();
    }

}
