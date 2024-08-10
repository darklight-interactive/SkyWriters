using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class VFX_Manager : MonoBehaviourSingleton<VFX_Manager>
{
    const string OBJECT_PREFIX = "<*>";

    public static VFX_ColorPalette ColorPalette => Instance.colorPalette;

    // ---------------- Properties ----------------
    [Header("Game VFX Settings")]
    [SerializeField, Expandable] VFX_ColorPalette _colorPalette;
    public VFX_ColorPalette colorPalette { get => _colorPalette; set => _colorPalette = value; }

    [SerializeField] VFX_GradientData _defaultGradient;
    public VFX_GradientData defaultGradientData => _defaultGradient;



    [Header("Cloud VFX")]
    [SerializeField] ParticleSystem _cloudParticles;
    [SerializeField] ParticleSystem _cloudBurstParticles;
    [SerializeField] ParticleSystem _cloudRingParticles;
    public ParticleSystem cloudParticles => _cloudParticles;
    public ParticleSystem cloudBurstParticles => _cloudBurstParticles;
    public ParticleSystem cloudRingParticles => _cloudRingParticles;


    [Header("Plane VFX")]
    [SerializeField] ParticleSystem _contrailParticles;
    [SerializeField] ParticleSystem _explosionParticles;
    public ParticleSystem contrailParticles => _contrailParticles;
    public ParticleSystem explosionParticles => _explosionParticles;

    public override void Initialize() { }

    #region =============================== [[ STATIC METHODS ]] =============================== >>

    public static VFX_ParticleSystemHandler CreateParticleSystemHandler(ParticleSystem particleSystem, Transform parent = null)
    {
        // Create the particle system game object
        GameObject go = Instantiate(particleSystem, parent).gameObject;
        go.name = OBJECT_PREFIX + particleSystem.name;
        go.transform.parent = parent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        // Add or get the VFX_ParticleSystemHandler component
        VFX_ParticleSystemHandler handler;
        if (go.TryGetComponent(out VFX_ParticleSystemHandler existingHandler))
        {
            handler = existingHandler;
        }
        else
        {
            handler = go.AddComponent<VFX_ParticleSystemHandler>();
        }

        return handler;
    }

    public static VFX_ParticleSystemHandler CreateParticleSystemHandler(ParticleSystem particleSystem, Vector3 position, Transform parent = null)
    {
        VFX_ParticleSystemHandler handler = CreateParticleSystemHandler(particleSystem, parent);
        handler.transform.position = position;

        return handler;
    }

    /// <summary>
    /// Create a new gradient from the given colors
    /// </summary>
    /// <param name="colors">
    ///     The colors to create the gradient from
    /// </param>
    /// <returns>
    ///     A new gradient with the given colors
    /// </returns>
    public static Gradient CreateGradient(Color[] colors)
    {
        Gradient gradient = new Gradient();

        int numColors = colors.Length > 8 ? 8 : colors.Length; // Only allow up to 8 colors
        GradientColorKey[] colorKeys = new GradientColorKey[numColors];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[numColors];
        for (int i = 0; i < numColors; i++)
        {
            colorKeys[i].color = colors[i];
            colorKeys[i].time = (float)i / (colors.Length - 1);

            alphaKeys[i].alpha = colors[i].a;
            alphaKeys[i].time = (float)i / (colors.Length - 1);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    /// <summary>
    /// Create a new gradient from the given VFX_ColorData objects
    /// </summary>
    /// <param name="colors">
    ///     The VFX_ColorData objects to create the gradient from
    /// </param>
    /// <returns>
    ///     A new gradient with the given colors
    /// </returns>
    public static Gradient CreateGradient(VFX_ColorData[] colors)
    {
        Color[] colorArray = new Color[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colorArray[i] = colors[i].Color;
        }

        return CreateGradient(colorArray);
    }
    #endregion
}