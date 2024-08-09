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
    [SerializeField] VFX_GradientData _baseGradient;
    [SerializeField] Gradient _currentGradient;

    public override void Initialize()
    {
        base.Initialize();

        if (_mainColor == null)
            _mainColor = VFX_Manager.ColorPalette.whiteColor;

        if (_baseGradient == null)
            _baseGradient = VFX_Manager.Instance.defaultGradient;

        _currentGradient = _baseGradient.CreateModifiedGradient(0, _mainColor);
        CreateCloudParticles(_currentGradient);
    }

    public void CreateCloudParticles(Gradient gradient)
    {
        ParticleSystem cloudParticles = VFX_Manager.Instance.cloudParticles;

        if (_cloudParticleHandler == null)
            _cloudParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudParticles, transform);

        _cloudParticleHandler.SetGradient(gradient);
        _cloudParticleHandler.Play();
    }

    public void CreateCloudBurstParticles()
    {
        if (_cloudBurstParticleHandler != null) return;

        ParticleSystem cloudBurstParticles = VFX_Manager.Instance.cloudBurstParticles;
        _cloudBurstParticleHandler = VFX_Manager.CreateParticleSystemHandler(cloudBurstParticles, transform);
        _cloudBurstParticleHandler.SetGradientData(_baseGradient);
        _cloudBurstParticleHandler.Play();
    }

}
