using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

public class CloudEntity : StageEntity
{

    VFX_ParticleSystemHandler _cloudParticleHandler;
    VFX_ParticleSystemHandler _cloudBurstParticleHandler;

    public override void Initialize(EntitySettings settings)
    {
        base.Initialize(settings);

        CreateCloudParticles();

        OnTriggerEntered += HandleTriggerEntered;
    }

    void HandleTriggerEntered(Collider other)
    {
        if (other.GetComponent<PlaneEntity>())
        {
            PlaneEntity plane = other.GetComponent<PlaneEntity>();
            plane.CollectNewColor(currentColor);

            if (Application.isPlaying)
            {
                CreateCloudBurstParticles();
                Destroy(gameObject);
            }
        }
    }

    Gradient GetCurrentGradient()
    {
        return VFX_Manager.Instance.defaultGradientData.CreateModifiedGradient(0, currentColor);
    }

    public void SetMainColor(VFX_ColorData colorData)
    {
        currentColor = colorData;
    }

    void CreateCloudParticles()
    {
        ParticleSystem cloudParticles = VFX_Manager.Instance.cloudParticles;

        _cloudParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudParticles, transform);

        _cloudParticleHandler.ApplyGradient(GetCurrentGradient());
        _cloudParticleHandler.Play();
    }

    public void CreateCloudBurstParticles()
    {
        if (_cloudBurstParticleHandler != null) return;

        ParticleSystem cloudBurstParticles = VFX_Manager.Instance.cloudBurstParticles;
        _cloudBurstParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudBurstParticles, null);

        _cloudBurstParticleHandler.ApplyGradient(GetCurrentGradient());
        _cloudBurstParticleHandler.Play();
    }

}
