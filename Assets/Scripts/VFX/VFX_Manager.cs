using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class VFX_Manager : MonoBehaviourSingleton<VFX_Manager>
{
    const string OBJECT_PREFIX = "<VFX>";

    public static VFX_ColorPalette ColorPalette => Instance.colorPalette;

    // ---------------- Properties ----------------
    [Header("Game VFX Settings")]
    [SerializeField, Expandable] VFX_ColorPalette _colorPalette;
    public VFX_ColorPalette colorPalette { get => _colorPalette; set => _colorPalette = value; }


    [Header("Cloud VFX")]
    [SerializeField] VFX_ParticleSystemHandler _cloudParticles;
    [SerializeField] VFX_ParticleSystemHandler _cloudBurstParticles;
    [SerializeField] VFX_ParticleSystemHandler _cloudRingParticles;


    [Header("Plane VFX")]
    [SerializeField] VFX_ParticleSystemHandler _contrailParticles;
    [SerializeField] VFX_ParticleSystemHandler _explosionParticles;

    public override void Initialize() { }

    #region =============================== [[ STATIC METHODS ]] =============================== >>
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
        GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            colorKeys[i].color = colors[i];
            colorKeys[i].time = (float)i / (colors.Length - 1);

            alphaKeys[i].alpha = colors[i].a;
            alphaKeys[i].time = (float)i / (colors.Length - 1);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }
    #endregion
}