using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(ParticleSystem))]
public class CloudInteractable : MonoBehaviour
{
    SphereCollider _collider => GetComponent<SphereCollider>();
    ParticleSystem _particleSystem => GetComponent<ParticleSystem>();
    CloudParticleData _cloudParticleData;

    [SerializeField] float _radius = 1.0f;
    [SerializeField] float _speed = 10.0f;

    public void SetCloudData(CloudParticleData cloudParticleData)
    {
        _cloudParticleData = cloudParticleData;
        SetColorOverLifetime(_cloudParticleData.ToGradient());
    }

    void SetColorOverLifetime(Gradient gradient)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _particleSystem.colorOverLifetime;
        colorOverLifetime.color = gradient;
    }

    // ================== Unity Events ==================
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlaneController>())
        {
            StageManager.Instance.SpawnRandomCloud();
            Destroy(gameObject);
        }
    }

    // =================== Unity Methods ======================
    void Update()
    {
        // Move the cloud to the right
        transform.position += Vector3.right * _speed * Time.deltaTime;
    }
}
