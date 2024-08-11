using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Editor;
using UnityEngine.VFX;
using System.Collections;




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
    [SerializeField, Expandable] VFX_GradientData _gradientData;
    public new ParticleSystem particleSystem
    {
        get
        {
            if (_particleSystem == null)
                _particleSystem = GetComponent<ParticleSystem>();
            return _particleSystem;
        }
        set => _particleSystem = value;
    }

    void Start() => Refresh();
    public void Refresh()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _initialized = LoadModules();
        if (!_initialized)
        {
            Debug.LogError("Failed to load particle system modules", this);
            return;
        }
    }

    public void Play() => _particleSystem.Play();
    public void Stop() => _particleSystem.Stop();

    public void ApplyGradient(Gradient gradient)
    {
        if (_particleSystem == null) return;
        if (!_initialized) return;

        _main = _particleSystem.main;
        _main.startColor = gradient.Evaluate(0);

        _colorOverLifetime = _particleSystem.colorOverLifetime;
        _colorOverLifetime.color = gradient;
    }

    public void StopAndDestroyOnComplete()
    {
        StartCoroutine(StopAndDestroyRoutine());
    }

    private IEnumerator StopAndDestroyRoutine()
    {
        particleSystem.Stop();
        particleSystem.name += " (Stopping)";

        // Wait until the particle system has no more particles
        while (particleSystem.IsAlive(true))
        {
            yield return null;
        }

        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
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

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play"))
            _script.Play();
        if (GUILayout.Button("Stop"))
            _script.Stop();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Refresh"))
        {
            _script.Refresh();
        }

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Refresh();
        }
    }
}
#endif
