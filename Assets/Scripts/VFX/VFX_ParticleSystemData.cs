using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Editor;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Darklight/VFX/ParticleSystemData")]
public class VFX_ParticleSystemData : ScriptableObject
{
    [SerializeField] ParticleSystem _particleSystem;

    #region ================================== [[ MODULE DATA ]] ================================== >>
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
    #endregion

    // ---------------- Serialized Fields ----------------
    [SerializeField, ShowOnly] bool _loaded;
    [SerializeField, ShowOnly] string _name;
    public VFX_ParticleSystemData() { }
    public VFX_ParticleSystemData(ParticleSystem particleSystem)
    {
        this._particleSystem = particleSystem;
        LoadModules();
    }

    public void Refresh()
    {
        _loaded = LoadModules();
        _name = this.name;
    }

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

    void Reset()
    {
        _loaded = false;
    }

    public ParticleSystem CreateInstance()
    {
        if (_particleSystem == null) return null;

        ParticleSystem ps = Instantiate(_particleSystem);
        ps.transform.position = Vector3.zero;
        ps.transform.rotation = Quaternion.identity;
        ps.name = $">> VFX Particle System: {this.name} <<";

        LoadModules();
        return ps;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(VFX_ParticleSystemData))]
public class VFX_ParticleSystemDataCustomEditor : Editor
{
    SerializedObject _serializedObject;
    VFX_ParticleSystemData _script;

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (VFX_ParticleSystemData)target;
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (GUILayout.Button("Create Instance"))
        {
            _script.CreateInstance();
        }

        if (GUILayout.Button("Refresh"))
        {
            _script.Refresh();
        }

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Refresh();
        }
    }
}
#endif
