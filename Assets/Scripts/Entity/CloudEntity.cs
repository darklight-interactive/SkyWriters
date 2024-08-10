using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

public class CloudEntity : StageEntity
{
    VFX_ParticleSystemHandler _cloudParticleHandler;
    VFX_ParticleSystemHandler _cloudBurstParticleHandler;

    [SerializeField] VFX_ColorData _mainColor;

    public override void Initialize()
    {
        base.Initialize();

        if (_mainColor == null)
            _mainColor = VFX_Manager.ColorPalette.whiteColor;

        DestroyAllParticles();
        CreateCloudParticles();

        OnTriggerEntered += HandleTriggerEntered;
    }

    void HandleTriggerEntered(Collider other)
    {
        if (other.GetComponent<PlaneEntity>())
        {
            PlaneEntity plane = other.GetComponent<PlaneEntity>();
            plane.CollectNewColor(_mainColor);

            if (Application.isPlaying)
            {
                CreateCloudBurstParticles();
                Destroy(gameObject);
            }
        }
    }

    Gradient GetCurrentGradient()
    {
        return VFX_Manager.Instance.defaultGradientData.CreateModifiedGradient(0, _mainColor);
    }

    public void SetMainColor(VFX_ColorData colorData)
    {
        _mainColor = colorData;
        Initialize();
    }

    void CreateCloudParticles()
    {
        ParticleSystem cloudParticles = VFX_Manager.Instance.cloudParticles;

        _cloudParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudParticles, transform);

        _cloudParticleHandler.ApplyGradientToParticleSystem(GetCurrentGradient());
        _cloudParticleHandler.Play();
    }

    public void CreateCloudBurstParticles()
    {
        if (_cloudBurstParticleHandler != null) return;

        ParticleSystem cloudBurstParticles = VFX_Manager.Instance.cloudBurstParticles;
        _cloudBurstParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudBurstParticles, null);

        _cloudBurstParticleHandler.ApplyGradientToParticleSystem(GetCurrentGradient());
        _cloudBurstParticleHandler.Play();
    }

}
