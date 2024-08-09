using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Editor;
using UnityEngine.VFX;



#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(ParticleSystem))]
public class VFX_ParticleSystemHandler : MonoBehaviour
{
    #region ----------- Module Data ----------------------
    ParticleSystem.MainModule _main;
    ParticleSystem.EmissionModule _emission;
    ParticleSystem.ShapeModule _shape;
    ParticleSystem.VelocityOverLifetimeModule _velocityOverLifetime;
    ParticleSystem.LimitVelocityOverLifetimeModule _limitVelocityOverLifetime;
    ParticleSystem.InheritVelocityModule _inheritVelocity;
    ParticleSystem.ForceOverLifetimeModule _forceOverLifetime;
    ParticleSystem.ColorOverLifetimeModule _colorOverLifetime;
    ParticleSystem.ColorBySpeedModule _colorBySpeed;
    ParticleSystem.SizeOverLifetimeModule _sizeOverLifetime;
    ParticleSystem.SizeBySpeedModule _sizeBySpeed;
    ParticleSystem.RotationOverLifetimeModule _rotationOverLifetime;
    ParticleSystem.RotationBySpeedModule _rotationBySpeed;
    ParticleSystem.ExternalForcesModule _externalForces;
    ParticleSystem.NoiseModule _noise;
    ParticleSystem.CollisionModule _collision;
    ParticleSystem.TriggerModule _trigger;
    ParticleSystem.SubEmittersModule _subEmitters;
    ParticleSystem.TextureSheetAnimationModule _textureSheetAnimation;
    ParticleSystem.LightsModule _lights;
    ParticleSystem.TrailModule _trails;
    ParticleSystem.CustomDataModule _customData;
    bool LoadModules()
    {
        if (_particleSystem == null) return false;

        _main = _particleSystem.main;
        _emission = _particleSystem.emission;
        _shape = _particleSystem.shape;
        _velocityOverLifetime = _particleSystem.velocityOverLifetime;
        _limitVelocityOverLifetime = _particleSystem.limitVelocityOverLifetime;
        _inheritVelocity = _particleSystem.inheritVelocity;
        _forceOverLifetime = _particleSystem.forceOverLifetime;
        _colorOverLifetime = _particleSystem.colorOverLifetime;
        _colorBySpeed = _particleSystem.colorBySpeed;
        _sizeOverLifetime = _particleSystem.sizeOverLifetime;
        _sizeBySpeed = _particleSystem.sizeBySpeed;
        _rotationOverLifetime = _particleSystem.rotationOverLifetime;
        _rotationBySpeed = _particleSystem.rotationBySpeed;
        _externalForces = _particleSystem.externalForces;
        _noise = _particleSystem.noise;
        _collision = _particleSystem.collision;
        _trigger = _particleSystem.trigger;
        _subEmitters = _particleSystem.subEmitters;
        _textureSheetAnimation = _particleSystem.textureSheetAnimation;
        _lights = _particleSystem.lights;
        _trails = _particleSystem.trails;
        _customData = _particleSystem.customData;

        return true;
    }
    #endregion

    // ---------------- Serialized Fields ----------------
    [SerializeField, ShowOnly] bool _initialized;
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField, Expandable] VFX_ColorData _colorData;

    void Start() => Refresh();
    public void Refresh()
    {
        _particleSystem = GetComponent<ParticleSystem>();


        bool modulesLoaded = LoadModules();
        bool colorDataSet = SetColorData(_colorData);
        _initialized = modulesLoaded && colorDataSet;
    }

    public bool SetColorData(VFX_ColorData colorData)
    {
        // If the color data is null, set it to white
        if (colorData == null)
        {
            colorData = VFX_Manager.ColorPalette.whiteColor;
        }

        // Store the data
        _colorData = colorData;
        _colorData.Refresh();

        // Set the color values
        _main.startColor = _colorData.Color;
        _colorOverLifetime.color = VFX_Manager.CreateGradient(new Color[] { _colorData.Color });

        return true;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(VFX_ParticleSystemHandler))]
public class VFX_ParticleSystemDataCustomEditor : Editor
{
    SerializedObject _serializedObject;
    VFX_ParticleSystemHandler _script;

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (VFX_ParticleSystemHandler)target;
        _script.Refresh();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Refresh();
        }
    }
}
#endif
