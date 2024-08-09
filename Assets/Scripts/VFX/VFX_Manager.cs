using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;

public class VFX_Manager : MonoBehaviourSingleton<VFX_Manager>
{
    const string OBJECT_PREFIX = "<VFX>";

    public override void Initialize() { }

    public static ParticleSystem InstantiateParticleSystem(ParticleSystem particleSystem)
    {
        ParticleSystem newParticleSystem = Instantiate(particleSystem);

        // Set the new particle system's parent to the VFX Manager
        newParticleSystem.transform.SetParent(Instance.transform);

        // Apply the default transform values
        newParticleSystem.transform.localPosition = Vector3.zero;
        newParticleSystem.transform.localRotation = Quaternion.identity;
        newParticleSystem.transform.localScale = Vector3.one;

        // Assign values to the game object
        newParticleSystem.gameObject.SetActive(true);
        newParticleSystem.gameObject.name = $"{OBJECT_PREFIX} {particleSystem.name}";

        return newParticleSystem;
    }

}